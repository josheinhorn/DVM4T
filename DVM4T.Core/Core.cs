using DVM4T.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using DVM4T.Attributes;
using System.Web;
using DVM4T.Exceptions;
using DVM4T.Reflection;
using DVM4T.Core;
using System.Web.Configuration;
using System.Linq.Expressions;
using DVM4T.Core.Binding;
using System.Collections;

namespace DVM4T.Core
{
    /// <summary>
    /// A static container class for default implementations of the View Model Framework
    /// </summary>
    public static class ViewModelDefaults
    {
        //Singletons
        private static readonly IViewModelKeyProvider keyProvider =
            new WebConfigViewModelKeyProvider("DVM4T.ViewModelKeyFieldName");

        private static readonly IReflectionHelper reflectionHelper = new ReflectionOptimizer();
        private static readonly IViewModelResolver resolver = new DefaultViewModelResolver(reflectionHelper);
        //private static readonly IViewModelBuilder viewModelBuilder = new ViewModelBuilder(keyProvider, resolver);
        private static readonly IViewModelFactory factory = new ViewModelFactory(keyProvider, resolver);
        //private static readonly ITypeResolver typeResolver = new DefaultResolver(reflectionHelper);
        /// <summary>
        /// Default View Model Builder. 
        /// <remarks>
        /// Set View Model Key Component Template Metadata field in Web config
        /// with key "DVM4T.ViewModelKeyFieldName". Defaults to field name "viewModelKey".
        /// </remarks>
        /// </summary>
        public static IViewModelFactory Factory { get { return factory; } }
        /// <summary>
        /// For legacy support - renamed to Factory
        /// </summary>
        [Obsolete("Use ViewModelDefaults.Factory instead")]
        public static IViewModelFactory Builder { get { return factory; } }
        /// <summary>
        /// Default View Model Key Provider. 
        /// <remarks>
        /// Gets View Model Key from Component Template Metadata with field
        /// name specified in Web config App Settings wtih key "DVM4T.ViewModelKeyFieldName".
        /// Defaults to field name "viewModelKey".
        /// </remarks>
        /// </summary>
        public static IViewModelKeyProvider ViewModelKeyProvider { get { return keyProvider; } }
        /// <summary>
        /// Default View Model Resolver
        /// </summary>
        /// <remarks>Resolves View Models with default parameterless constructor. If none 
        /// exists, it will throw an Exception.</remarks>
        public static IViewModelResolver ModelResolver { get { return resolver; } }
        /// <summary>
        /// Optimized Reflection Helper that caches resuslts of resource-heavy tasks (e.g. MemberInfo.GetCustomAttributes)
        /// </summary>
        public static IReflectionHelper ReflectionCache { get { return reflectionHelper; } }
        /// <summary>
        /// Creates a new Model Mapping object
        /// </summary>
        /// <typeparam name="T">Type of model for the model mapping</typeparam>
        /// <returns>New Model Mapping</returns>
        public static IModelMapping CreateModelMapping<T>() where T : class
        {
            return new DefaultModelMapping(resolver, reflectionHelper, typeof(T));
        }
        //public static ITypeResolver TypeResolver { get { return typeResolver; } }

    }

    /// <summary>
    /// Base View Model Key Provider implementation with no external dependencies. Set protected 
    /// string ViewModelKeyField to CT Metadata Field name to use to retrieve View Model Keys.
    /// </summary>
    public abstract class ViewModelKeyProviderBase : IViewModelKeyProvider
    {
        protected string ViewModelKeyField = string.Empty;
        public string GetViewModelKey(IViewModelData modelData)
        {
            string result = null;
            var templatedData = modelData as ITemplatedViewModelData;
            if (templatedData != null
                    && templatedData.Template != null
                    && templatedData.Template.Metadata != null
                    && templatedData.Template.Metadata.ContainsKey(ViewModelKeyField))
            {
                result = templatedData.Template.Metadata[ViewModelKeyField].Values.Cast<string>().FirstOrDefault();
            }
            else if (modelData is IKeywordData)
            {
                var keyword = modelData as IKeywordData;
                if (keyword.Metadata != null && keyword.MetadataSchema != null) //If there is a metadata schema
                {
                    result = keyword.MetadataSchema.Title;
                }
                else if (keyword != null && keyword.Category != null) //If there's no metadata schema fall back to the category title
                    result = keyword.Category.Title;
            }
            else if (modelData != null && modelData.Metadata != null && modelData.Metadata.ContainsKey(ViewModelKeyField))
            {
                result = modelData.Metadata[ViewModelKeyField].Values.Cast<string>().FirstOrDefault();
            }

            return result;
        }
    }

   

    /// <summary>
    /// Implementation of View Model Key Provider that uses the Web Config app settings 
    /// to retrieve the name of the Component Template Metadata field for the view model key.
    /// Default CT Metadata field name is "viewModelKey"
    /// </summary>
    public class WebConfigViewModelKeyProvider : ViewModelKeyProviderBase
    {
        public WebConfigViewModelKeyProvider(string webConfigKey)
        {
            ViewModelKeyField = WebConfigurationManager.AppSettings[webConfigKey];
            if (string.IsNullOrEmpty(ViewModelKeyField)) ViewModelKeyField = "viewModelKey"; //Default value
        }
    }

    /// <summary>
    /// Data structure for efficient use of Properties marked with a IPropertyAttribute Custom Attribute
    /// </summary>
    public class ModelProperty : IModelProperty
    {
        /// <summary>
        /// Name of the Property
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Setter delegate
        /// </summary>
        public Action<object, object> Set { get; set; }
        /// <summary>
        /// Getter delegate
        /// </summary>
        public Func<object, object> Get { get; set; }
        /// <summary>
        /// The DVM4T PropertyAttribute of the Property
        /// </summary>
        public IPropertyAttribute PropertyAttribute { get; set; }
        /// <summary>
        /// The return Type of the Property
        /// </summary>
        public Type PropertyType { get; set; }
        public Type ModelType { get; set; }
        public bool IsEnumerable { get; set; }
        public bool IsCollection { get; set; }
        public Action<object, object> AddToCollection
        {
            get;
            set;
        }
        public bool IsArray
        {
            get;
            set;
        }
        public Func<IEnumerable, Array> ToArray { get; set; }
    }

    #region Old View Model Data Basic Implementations
    //public abstract class ViewModelDataBase : IViewModelData
    //{
    //    public IFieldsData Metadata
    //    {
    //        get;
    //        protected set;
    //    }

    //    public ITemplateData Template
    //    {
    //        get;
    //        protected set;
    //    }

    //    public int PublicationId
    //    {
    //        get;
    //        protected set;
    //    }

    //    public object BaseData
    //    {
    //        get;
    //        protected set;
    //    }
    //}
    //public class PageViewModelData : ViewModelDataBase, IPageViewModelData
    //{
    //    public PageViewModelData(IPageData pageData)
    //    {
    //        Metadata = pageData.Metadata;
    //        PublicationId = pageData.PublicationId;
    //        Template = pageData.Template;
    //        Page = pageData;
    //        BaseData = pageData;
    //    }

    //    public IPageData Page
    //    {
    //        get;
    //        private set;
    //    }
    //}
    //public class ComponentPresentationViewModelData : ContentViewModelData, IComponentPresentationViewModelData
    //{
    //    public ComponentPresentationViewModelData(IContentPresentationData cpData)
    //        : base (cpData)
    //    {
    //        ComponentPresentation = cpData;
    //    }
    //    public IContentPresentationData ComponentPresentation
    //    {
    //        get;
    //        private set;
    //    }
    //}
    //public class ContentViewModelData : ViewModelDataBase, IContentViewModelData
    //{
    //    public ContentViewModelData(IContentPresentationData cpData)
    //    {
    //        Metadata = cpData.Metadata;
    //        PublicationId = cpData.PublicationId;
    //        Template = cpData.Template;
    //        Schema = cpData.Schema;
    //        Content = cpData.Content;
    //        BaseData = cpData;
    //    }
    //    public ContentViewModelData(IFieldsData fieldsData, ISchemaData schema, ITemplateData template)
    //    {
    //        Metadata = null;
    //        PublicationId = template.PublicationId;
    //        Template = template;
    //        Schema = schema;
    //        Content = fieldsData;
    //        BaseData = fieldsData;
    //    }
    //    public ISchemaData Schema
    //    {
    //        get;
    //        private set;
    //    }

    //    public IFieldsData Content
    //    {
    //        get;
    //        private set;
    //    }
    //}
    #endregion
}
