using DVM4T.Contracts;
using DVM4T.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DVM4T.Core
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly IDictionary<IModelAttribute, Type> viewModels = new Dictionary<IModelAttribute, Type>();
        private readonly IList<Assembly> loadedAssemblies = new List<Assembly>();
        private readonly IViewModelResolver resolver;
        private readonly IViewModelKeyProvider keyProvider;
        /// <summary>
        /// New View Model Builder
        /// </summary>
        /// <param name="keyProvider">A View Model Key provider</param>
        public ViewModelFactory(IViewModelKeyProvider keyProvider, IViewModelResolver resolver)
        {
            if (keyProvider == null) throw new ArgumentNullException("keyProvider");
            if (resolver == null) throw new ArgumentNullException("resolver");
            this.keyProvider = keyProvider;
            this.resolver = resolver;
        }
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

                if (modelAttr != null && modelAttr.IsMatch(data, keyProvider.GetViewModelKey(data)))
                    return type;
            }
            throw new ViewModelTypeNotFoundException(data);
        }        
        public IViewModel BuildViewModel(IViewModelData modelData)
        {
            return BuildViewModelByAttribute<IModelAttribute>(modelData);
        }

        public IViewModel BuildViewModelByAttribute<T>(IViewModelData modelData) where T : IModelAttribute
        {
            IViewModel result = null;
            Type type = FindViewModelByAttribute<T>(modelData);
            if (type != null)
            {
                result = BuildViewModel(type, modelData);
            }
            return result;
        }
        public IViewModel BuildViewModel(Type type, IViewModelData modelData)
        {
            IViewModel viewModel = null;
            viewModel = resolver.ResolveModel(type, modelData);
            viewModel.ModelData = modelData;
            ProcessViewModel(viewModel, type, modelData.Template);
            return viewModel;
        }

        T BuildViewModel<T>(IViewModelData modelData)
        {
            return (T)BuildViewModel(typeof(T), modelData);
        }

        T IViewModelFactory.BuildViewModel<T>(IViewModelData modelData)
        {
            return (T)BuildViewModel(typeof(T), modelData);
        }

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
