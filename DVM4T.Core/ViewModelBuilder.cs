using DVM4T.Attributes;
using DVM4T.Contracts;
using DVM4T.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DVM4T.Core
{
    /// <summary>
    /// Core implementation of View Model Builder 
    /// <remarks>
    /// If using View Model Type inference, this should be used as a singleton due to the reflection overhead 
    /// involved in LoadViewModels.
    /// </remarks>
    /// </summary>
    public class ViewModelBuilder : IViewModelBuilder
    {
        //private IDictionary<ViewModelAttribute, Type> viewModels2 = new Dictionary<ViewModelAttribute, Type>();
        private IDictionary<IModelAttribute, Type> viewModels = new Dictionary<IModelAttribute, Type>();
        private IList<Assembly> loadedAssemblies = new List<Assembly>();
        private readonly IViewModelResolver resolver;
        public IViewModelKeyProvider keyProvider;
        /// <summary>
        /// New View Model Builder
        /// </summary>
        /// <param name="keyProvider">A View Model Key provider</param>
        public ViewModelBuilder(IViewModelKeyProvider keyProvider, IViewModelResolver resolver)
        {
            if (keyProvider == null) throw new ArgumentNullException("keyProvider");
            if (resolver == null) throw new ArgumentNullException("resolver");
            this.keyProvider = keyProvider;
            this.resolver = resolver;
        }
        #region IViewModelBuilder
        public IViewModelKeyProvider ViewModelKeyProvider { get { return keyProvider; } }
        /// <summary>
        /// Loads View Model Types from an Assembly. Use minimally due to reflection overhead.
        /// </summary>
        /// <param name="assembly"></param>
        public void LoadViewModels(Assembly assembly) //We assume we have a singleton of this instance, otherwise we incur a lot of overhead
        {
            if (!loadedAssemblies.Contains(assembly))
            {
                loadedAssemblies.Add(assembly);
                IModelAttribute viewModelAttr;
                foreach (var type in assembly.GetTypes())
                {
                    viewModelAttr = resolver.GetCustomAttribute<IModelAttribute>(type);
                    if (viewModelAttr != null && !viewModels.ContainsKey(viewModelAttr))
                    {
                        viewModels.Add(viewModelAttr, type);
                    }
                }
            }
        }
        public Type FindViewModelByAttribute<T>(IViewModelData data, Type[] typesToSearch = null) where T : IModelAttribute
        {
            //Anyway to speed this up? Better than just a straight up loop?
            typesToSearch = typesToSearch ?? viewModels.Where(x => x.Key is T).Select(x => x.Value).ToArray();
            foreach (var type in typesToSearch)
            {
                T modelAttr = resolver.GetCustomAttribute<T>(type);

                if (modelAttr != null && modelAttr.IsMatch(data, ViewModelKeyProvider))
                    return type;
            }
            throw new ViewModelTypeNotFoundException(data);
        }
        public IViewModel BuildCPViewModel(Type type, IComponentPresentationData cp)
        {
            IViewModel viewModel = null;
            //TODO: This is a Constructor Constraint, look at moving to Dependency/Constructor Injection
            var modelData = new ComponentPresentationViewModelData(cp, this);
            viewModel = resolver.ResolveModel(type, modelData);
            viewModel.ModelData = modelData;
            IFieldsData fields = cp.Component.Fields;
            ProcessViewModel(viewModel, type, cp.ComponentTemplate);
            return viewModel;
        }
        public T BuildCPViewModel<T>(IComponentPresentationData cp) where T : class, IViewModel
        {
            Type type = typeof(T);
            return (T)BuildCPViewModel(type, cp);
        }
        public IViewModel BuildCPViewModel(IComponentPresentationData cp)
        {
            IViewModel result = null;
            if (cp == null) throw new ArgumentNullException("cp");
            var modelData = new ComponentPresentationViewModelData(cp, this);
            Type type = FindViewModelByAttribute<IContentModelAttribute>(modelData);
            
            if (type != null)
            {
                result = BuildCPViewModel(type, cp);
            }
            return result;
        }
        public T BuildEmbeddedViewModel<T>(IFieldsData embeddedFields, ISchemaData schema, ITemplateData template) where T : class, IViewModel
        {
            Type type = typeof(T);
            return (T)BuildEmbeddedViewModel(type, embeddedFields, schema, template);
        }
        public IViewModel BuildEmbeddedViewModel(IFieldsData embeddedFields, ISchemaData schema, ITemplateData template)
        {
            if (embeddedFields == null) throw new ArgumentNullException("embeddedFields");
            if (schema == null) throw new ArgumentNullException("schema");
            if (template == null) throw new ArgumentNullException("template");
            var modelData = new ContentViewModelData(embeddedFields, schema, template, this);
            IViewModel result = null;
            Type type = FindViewModelByAttribute<IContentModelAttribute>(modelData);
            //var type = viewModels.Where(x => x.Key.Equals(key)).Select(x => x.Value).FirstOrDefault();
            //Type type = viewModels.ContainsKey(key) ? viewModels[key] : null; //Keep an eye on this -- GetHashCode isn't the same as Equals
            if (type != null)
            {
                result = (IViewModel)BuildEmbeddedViewModel(type, embeddedFields, schema, template);
            }
            return result;
        }
        public IViewModel BuildEmbeddedViewModel(Type type, IFieldsData embeddedFields, ISchemaData schema, ITemplateData template)
        {
            if (type == null) throw new ArgumentNullException("type");
            //TODO: This is a Constructor Constraint, look at moving to Dependency/Constructor Injection
            var modelData = new ContentViewModelData(embeddedFields, schema, template, this);
            IViewModel viewModel = resolver.ResolveModel(type, modelData);
            viewModel.ModelData = modelData;
            ProcessViewModel(viewModel, type, template);
            return viewModel;
        }
        public T BuildPageViewModel<T>(IPageData page) where T : IViewModel
        {
            Type type = typeof(T);
            return (T)BuildPageViewModel(type, page);
        }
        public IViewModel BuildPageViewModel(Type type, IPageData page)
        {
            if (type == null) throw new ArgumentNullException("type");
            //TODO: This is a Constructor Constraint, look at moving to Dependency/Constructor Injection
            var modelData = new PageViewModelData(page, this);
            IViewModel viewModel = resolver.ResolveModel(type, modelData);
            viewModel.ModelData = modelData;
            ProcessViewModel(viewModel, type, page.PageTemplate);
            return viewModel;
        }
        public IViewModel BuildPageViewModel(IPageData page)
        {
            IViewModel result = null;
            if (page == null) throw new ArgumentNullException("page");
            var modelData = new PageViewModelData(page, this);
            Type type = FindViewModelByAttribute<IPageModelAttribute>(modelData);

            if (type != null)
            {
                result = BuildPageViewModel(type, page);
            }
            return result;
        }
        #endregion

        #region Private methods


        private void ProcessViewModel(IViewModel viewModel, Type type, ITemplateData template)
        {
            //PropertyInfo[] props = type.GetProperties();
            var props = resolver.GetModelProperties(type);
            IPropertyAttribute propAttribute;
            object value = null;
            foreach (var prop in props)
            {
                propAttribute = prop.PropertyAttribute;//prop.GetCustomAttributes(typeof(FieldAttributeBase), true).FirstOrDefault() as FieldAttributeBase;
                if (propAttribute != null) //It has a FieldAttribute
                {
                    value = propAttribute.GetPropertyValue(viewModel, prop.PropertyType, this); //delegate all the real work to the Property Attribute object itself. Allows for custom attribute types to easily be added
                    if (value != null)
                    {
                        try
                        {
                            prop.Set(viewModel, value);
                        }
                        catch (Exception e)
                        {
                            if (e is TargetException || e is InvalidCastException)
                                throw new PropertyTypeMismatchException(prop, propAttribute, value);
                            else throw e;
                        }
                    }
                }
            }
        }
        private string[] GetViewModelKey(IViewModelData model)
        {
            string key = keyProvider.GetViewModelKey(model);
            return String.IsNullOrEmpty(key) ? null : new string[] { key };
        }
        #endregion



    }

}
