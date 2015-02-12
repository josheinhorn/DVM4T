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
            new WebConfigViewModelKeyProvider("ViewModelKeyFieldName");
        private static readonly IViewModelBuilder viewModelBuilder = new ViewModelBuilder(keyProvider);
        /// <summary>
        /// Default View Model Builder. 
        /// <remarks>
        /// Set View Model Key Component Template Metadata field in Web config
        /// with key "ViewModelKeyFieldName". Defaults to field name "viewModelKey".
        /// </remarks>
        /// </summary>
        public static IViewModelBuilder Builder { get { return viewModelBuilder; } }
        /// <summary>
        /// Default View Model Key Provider. 
        /// <remarks>
        /// Gets View Model Key from Component Template Metadata with field
        /// name specified in Web config App Settings wtih key "ViewModelKeyFieldName".
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
        public string GetViewModelKey(IComponentTemplateData template)
        {
            string result = null;
            if (template != null
                && template.MetadataFields != null
                && template.MetadataFields.ContainsKey(ViewModelKeyField))
            {
                result = template.MetadataFields[ViewModelKeyField].Values.Cast<string>().FirstOrDefault();
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

    public class ViewModelData : IViewModelData
    {
        public ViewModelData(IComponentPresentationData cpData, IViewModelBuilder builder)
        {
            Builder = builder;
            Content = cpData.Component.Fields;
            Metadata = cpData.Component.MetadataFields;
            ComponentTemplate = cpData.ComponentTemplate;
            PublicationId = cpData.Component.PublicationId;
        }
        public ViewModelData(IFieldsData fields, IComponentTemplateData template, IViewModelBuilder builder)
        {
            Builder = builder;
            Content = fields;
            Metadata = null;
            ComponentTemplate = template;
            PublicationId = template.PublicationId;
        }

        public IViewModelBuilder Builder
        {
            get;
            private set;
        }

        public IFieldsData Content
        {
            get;
            private set;
        }

        public IFieldsData Metadata
        {
            get;
            private set;
        }

        public IComponentTemplateData ComponentTemplate
        {
            get;
            private set;
        }

        public int PublicationId
        {
            get;
            private set;
        }
    }
}
