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
        private static readonly IViewModelBuilder viewModelBuilder = new ViewModelBuilder(keyProvider);
        /// <summary>
        /// Default View Model Builder. 
        /// <remarks>
        /// Set View Model Key Component Template Metadata field in Web config
        /// with key "DVM4T.ViewModelKeyFieldName". Defaults to field name "viewModelKey".
        /// </remarks>
        /// </summary>
        public static IViewModelBuilder Builder { get { return viewModelBuilder; } }
        /// <summary>
        /// Default View Model Key Provider. 
        /// <remarks>
        /// Gets View Model Key from Component Template Metadata with field
        /// name specified in Web config App Settings wtih key "DVM4T.ViewModelKeyFieldName".
        /// Defaults to field name "viewModelKey".
        /// </remarks>
        /// </summary>
        public static IViewModelKeyProvider ViewModelKeyProvider { get { return keyProvider; } }
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
            if (modelData != null
                    && modelData.Template != null
                    && modelData.Template.Metadata != null
                    && modelData.Template.Metadata.ContainsKey(ViewModelKeyField))
            {
                result = modelData.Template.Metadata[ViewModelKeyField].Values.Cast<string>().FirstOrDefault();
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

    #region View Model Data
    public abstract class ViewModelDataBase : IViewModelData
    {
        public IViewModelBuilder Builder
        {
            get;
            protected set;
        }

        public IFieldsData Metadata
        {
            get;
            protected set;
        }

        public ITemplateData Template
        {
            get;
            protected set;
        }

        public int PublicationId
        {
            get;
            protected set;
        }

        public object BaseData
        {
            get;
            protected set;
        }
    }
    public class PageViewModelData : ViewModelDataBase, IPageViewModelData
    {
        public PageViewModelData(IPageData pageData, IViewModelBuilder builder)
        {
            Builder = builder;
            Metadata = pageData.Metadata;
            PublicationId = pageData.PublicationId;
            Template = pageData.PageTemplate;
            Page = pageData;
            BaseData = pageData;
        }

        public IPageData Page
        {
            get;
            private set;
        }
    }
    public class ComponentPresentationViewModelData : ContentViewModelData, IComponentPresentationViewModelData
    {
        public ComponentPresentationViewModelData(IComponentPresentationData cpData, IViewModelBuilder builder)
            : base (cpData, builder)
        {
            ComponentPresentation = cpData;
        }
        public IComponentPresentationData ComponentPresentation
        {
            get;
            private set;
        }
    }
    public class ContentViewModelData : ViewModelDataBase, IContentViewModelData
    {
        public ContentViewModelData(IComponentPresentationData cpData, IViewModelBuilder builder)
        {
            Builder = builder;
            Metadata = cpData.Component.MetadataFields;
            PublicationId = cpData.Component.PublicationId;
            Template = cpData.ComponentTemplate;
            Schema = cpData.Component.Schema;
            ContentData = cpData.Component.Fields;
            BaseData = cpData;
        }
        public ContentViewModelData(IFieldsData fieldsData, ISchemaData schema, ITemplateData template, IViewModelBuilder builder)
        {
            Builder = builder;
            Metadata = null;
            PublicationId = template.PublicationId;
            Template = template;
            Schema = schema;
            ContentData = fieldsData;
            BaseData = fieldsData;
        }
        public ISchemaData Schema
        {
            get;
            private set;
        }

        public IFieldsData ContentData
        {
            get;
            private set;
        }
    }
    #endregion
}
