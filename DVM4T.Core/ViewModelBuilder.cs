using DVM4T.Attributes;
using DVM4T.Contracts;
using DVM4T.Exceptions;
using DVM4T.Reflection;
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
        private IDictionary<ViewModelAttribute, Type> viewModels = new Dictionary<ViewModelAttribute, Type>();
        private IList<Assembly> loadedAssemblies = new List<Assembly>();
        public IViewModelKeyProvider keyProvider;
        /// <summary>
        /// New View Model Builder
        /// </summary>
        /// <param name="keyProvider">A View Model Key provider</param>
        public ViewModelBuilder(IViewModelKeyProvider keyProvider)
        {
            if (keyProvider == null) throw new ArgumentNullException("keyProvider");
            this.keyProvider = keyProvider;
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
                ViewModelAttribute viewModelAttr;
                foreach (var type in assembly.GetTypes())
                {
                    viewModelAttr = ReflectionUtility.ReflectionCache.GetCustomAttribute<ViewModelAttribute>(type);
                    if (viewModelAttr != null && !viewModels.ContainsKey(viewModelAttr))
                    {
                        viewModels.Add(viewModelAttr, type);
                    }
                }
            }
        }
        public IComponentPresentationViewModel BuildCPViewModel(Type type, IComponentPresentationData cp)
        {
            IComponentPresentationViewModel viewModel = null;
            viewModel = (IComponentPresentationViewModel)ReflectionUtility.ReflectionCache.CreateInstance(type);
            viewModel.ComponentPresentation = cp;
            viewModel.ModelData = new ViewModelData(cp, this);
            IFieldsData fields = cp.Component.Fields;
            ProcessViewModel(viewModel, type, cp.ComponentTemplate);
            return viewModel;
        }
        public T BuildCPViewModel<T>(IComponentPresentationData cp) where T : class, IComponentPresentationViewModel
        {
            Type type = typeof(T);
            return (T)BuildCPViewModel(type, cp);
        }
        public IComponentPresentationViewModel BuildCPViewModel(IComponentPresentationData cp)
        {
            if (cp == null) throw new ArgumentNullException("cp");
            var key = new ViewModelAttribute(cp.Component.Schema.Title, false)
            {
                ViewModelKeys = GetViewModelKey(cp.ComponentTemplate)
            };
            IComponentPresentationViewModel result = null;
            //var type = viewModels.Where(x => x.Key.Equals(key)).Select(x => x.Value).FirstOrDefault();
            Type type = viewModels.ContainsKey(key) ? viewModels[key] : null; //Keep an eye on this -- GetHashCode isn't the same as Equals
            if (type != null)
            {
                result = BuildCPViewModel(type, cp);
            }
            else
            {
                throw new ViewModelTypeNotFoundExpception(key.SchemaName, key.ViewModelKeys.FirstOrDefault());
            }
            return result;
        }
        public T BuildEmbeddedViewModel<T>(IFieldsData embeddedFields, IComponentTemplateData template) where T : class, IEmbeddedSchemaViewModel
        {
            Type type = typeof(T);
            return (T)BuildEmbeddedViewModel(type, embeddedFields, template);
        }
        public IEmbeddedSchemaViewModel BuildEmbeddedViewModel(IFieldsData embeddedFields, ISchemaData schema, IComponentTemplateData template)
        {
            if (embeddedFields == null) throw new ArgumentNullException("embeddedFields");
            if (schema == null) throw new ArgumentNullException("schema");
            if (template == null) throw new ArgumentNullException("template");
            var key = new ViewModelAttribute(schema.Title, false)
            {
                ViewModelKeys = GetViewModelKey(template)
            };
            IEmbeddedSchemaViewModel result = null;
            //var type = viewModels.Where(x => x.Key.Equals(key)).Select(x => x.Value).FirstOrDefault();
            Type type = viewModels.ContainsKey(key) ? viewModels[key] : null; //Keep an eye on this -- GetHashCode isn't the same as Equals
            if (type != null)
            {
                result = (IEmbeddedSchemaViewModel)BuildEmbeddedViewModel(type, embeddedFields, template);
            }
            else
            {
                throw new ViewModelTypeNotFoundExpception(key.SchemaName, key.ViewModelKeys.FirstOrDefault());
            }
            return result;
        }
        public IEmbeddedSchemaViewModel BuildEmbeddedViewModel(Type type, IFieldsData embeddedFields, IComponentTemplateData template)
        {
            IEmbeddedSchemaViewModel viewModel = (IEmbeddedSchemaViewModel)ReflectionUtility.ReflectionCache.CreateInstance(type);
            viewModel.ModelData = new ViewModelData(embeddedFields, template, this);
            ProcessViewModel(viewModel, type, template);
            return viewModel;
        }
        #endregion

        #region Private methods


        private void ProcessViewModel(IViewModel viewModel, Type type, IComponentTemplateData template)
        {
            //PropertyInfo[] props = type.GetProperties();
            var props = ReflectionUtility.ReflectionCache.GetModelProperties(type);
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
        private string[] GetViewModelKey(IComponentTemplateData template)
        {
            string key = keyProvider.GetViewModelKey(template);
            return String.IsNullOrEmpty(key) ? null : new string[] { key };
        }
        #endregion
    }

}
