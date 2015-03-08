using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Contracts;
using System.Reflection;
using System.Web;
using DVM4T.Reflection;
using DVM4T.Exceptions;
using System.Collections;
using DVM4T.Core;

namespace DVM4T.Attributes
{
    /// <summary>
    /// A Base class for an Attribute identifying a Property that represents some part of a View Model
    /// </summary>
    public abstract class ModelPropertyAttributeBase : Attribute, IPropertyAttribute
    {
        public abstract IEnumerable GetPropertyValues(IViewModelData modelData, IModelProperty property, IViewModelFactory factory = null);
        /// <summary>
        /// When overriden in a derived class, this property returns the expected return type of the View Model property.
        /// </summary>
        /// <remarks>Primarily used for debugging purposes. This property is used to throw an accurate exception at run time if
        /// the property return type does not match with the expected type.</remarks>
        public abstract Type ExpectedReturnType { get; }


        public Core.Binding.IModelMapping ComplexTypeMapping
        {
            get;
            set;
        }
    }

    /// <summary>
    /// A Base class for an Attribute identifying a Property that represents a Field
    /// </summary>
    /// <remarks>
    /// The field can be content, metadata, or the metadata of a template
    /// </remarks>
    public abstract class FieldAttributeBase : ModelPropertyAttributeBase, IFieldAttribute
    {
        protected bool allowMultipleValues = false;
        protected bool inlineEditable = false;
        protected bool mandatory = false; //probably don't need this one
        protected bool isMetadata = false;
        /// <summary>
        /// Creates a new Field Attribute
        /// </summary>
        public FieldAttributeBase()
        { }
        
        public override IEnumerable GetPropertyValues(IViewModelData modelData, IModelProperty property, IViewModelFactory factory = null)
        {
            IEnumerable result = null;
            if (modelData != null)
            {
                //need null checks on Template
                IFieldsData fields = null;
                if (IsTemplateMetadata && modelData is ITemplatedViewModelData)
                {
                    var templateData = modelData as ITemplatedViewModelData;
                    fields = templateData.Template != null ? templateData.Template.Metadata : null;
                }
                else if (IsMetadata)
                {
                    fields = modelData.Metadata;
                }
                else if (modelData is IContentPresentationData)
                {
                    fields = (modelData as IContentPresentationData).Content;
                }
                else
                {
                    fields = modelData.Metadata;
                }


                if (String.IsNullOrEmpty(FieldName)) FieldName = GetFieldName(property.Name); //Convention over configuration by default -- Field name = Property name
                if (fields != null && fields.ContainsKey(FieldName))
                {
                    var template = modelData is ITemplatedViewModelData ? (modelData as ITemplatedViewModelData).Template
                        : null;
                    result = this.GetFieldValues(fields[FieldName], property, template, factory);
                }
            }
            return result;
        }
        private string GetFieldName(string propertyName)
        {
            return propertyName.Substring(0, 1).ToLowerInvariant() + propertyName.Substring(1); //lowercase the first letter
        }
        /// <summary>
        /// When overriden in a derived class, this method should return the value of the View Model property from a Field object
        /// </summary>
        /// <param name="field">The Field</param>
        /// <param name="propertyType">The concrete type of the view model property for this attribute</param>
        /// <param name="template">The Component Template to use</param>
        /// <param name="factory">The View Model Builder</param>
        /// <returns></returns>
        public abstract IEnumerable GetFieldValues(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null);

        /// <summary>
        /// The Tridion schema field name for this property
        /// </summary>
        public string FieldName
        {
            get;
            set;
        }
        /// <summary>
        /// Is a multi value field.
        /// </summary>
        public bool AllowMultipleValues
        {
            get
            {
                return allowMultipleValues;
            }
            set { allowMultipleValues = value; }
        }
        /// <summary>
        /// Is inline editable. For semantic use only.
        /// </summary>
        public bool InlineEditable
        {
            get
            {
                return inlineEditable;
            }
            set
            {
                inlineEditable = value;
            }
        }
        /// <summary>
        /// Is a mandatory field. For semantic use only.
        /// </summary>
        public bool Mandatory
        {
            get
            {
                return mandatory;
            }
            set
            {
                mandatory = value;
            }
        }
        /// <summary>
        /// Is a metadata field. False indicates this is a content field.
        /// </summary>
        public bool IsMetadata
        {
            get { return isMetadata; }
            set { isMetadata = value; }
        }

        public bool IsTemplateMetadata { get; set; }

    }
    /// <summary>
    /// A Base class for an Attribute identifying a Property that represents some part of a Component 
    /// </summary>
    public abstract class ComponentAttributeBase : ModelPropertyAttributeBase, IComponentAttribute
    {
        public override IEnumerable GetPropertyValues(IViewModelData modelData, IModelProperty property, IViewModelFactory factory = null)
        {
            IEnumerable result = null;
            if (modelData != null)
            {
                if (modelData is IComponentPresentationData)
                {
                    var cpData = modelData as IComponentPresentationData;
                    if (cpData != null)
                    {
                        result = GetPropertyValues(cpData.Component, property,
                            cpData.Template, factory);
                    }
                }
                else if (modelData is IComponentData) //Not all components come with Templates
                {
                    result = GetPropertyValues(modelData as IComponentData, property, null, factory);
                }
            }
            return result;
        }
        /// <summary>
        /// When overriden in a derived class, this gets the value of the Property for a given Component
        /// </summary>
        /// <param name="component">Component for the View Model</param>
        /// <param name="propertyType">Actual return type for the Property</param>
        /// <param name="template">Component Template</param>
        /// <param name="factory">View Model factory</param>
        /// <returns>The Property value</returns>
        public abstract IEnumerable GetPropertyValues(IComponentData component, IModelProperty property, ITemplateData template, IViewModelFactory factory = null);
    }

    /// <summary>
    ///  A Base class for an Attribute identifying a Property that represents some part of a Component Template
    /// </summary>
    public abstract class ComponentTemplateAttributeBase : ModelPropertyAttributeBase, ITemplateAttribute
    {
        /// <summary>
        /// When overriden in a derived class, this gets the value of the Property for a given Component Template
        /// </summary>
        /// <param name="template">Component Template for the View Model</param>
        /// <param name="propertyType">Actual return type for the Property</param>
        /// <param name="factory">View Model factory</param>
        /// <returns>The Property value</returns>
        public abstract object GetPropertyValues(ITemplateData template, Type propertyType, IViewModelFactory factory = null);

        public override IEnumerable GetPropertyValues(IViewModelData modelData, IModelProperty property, IViewModelFactory factory = null)
        {
            IEnumerable result = null;
            if (modelData is IContentPresentationData
                && (modelData as IContentPresentationData).Template is ITemplateData)
            {
                var templateData = (modelData as IContentPresentationData).Template as ITemplateData;
                result = this.GetPropertyValues(templateData, property, factory);
            }
            return result;
        }
    }

    /// <summary>
    /// A Base class for an Attribute identifying a Property that represents some part of a Page
    /// </summary>
    public abstract class PageAttributeBase : ModelPropertyAttributeBase, IPageAttribute
    {
        /// <summary>
        /// When overriden in a derived class, this gets the value of the Property for a given Page
        /// </summary>
        /// <param name="page">Page for the View Model</param>
        /// <param name="propertyType">Actual return type for the Property</param>
        /// <param name="factory">View Model factory</param>
        /// <returns>The Property value</returns>
        public abstract object GetPropertyValues(IPageData page, Type propertyType, IViewModelFactory factory = null);

        public override IEnumerable GetPropertyValues(IViewModelData modelData, IModelProperty property, IViewModelFactory factory = null)
        {
            IEnumerable result = null;
            if (modelData is IPageData)
            {
                var pageModel = (modelData as IPageData);
                result = this.GetPropertyValues(pageModel, property, factory);
            }
            return result;
        }
    }

    /// <summary>
    /// A Base class for an Attribute identifying a Property that represents a set of Component Presentations
    /// </summary>
    /// <remarks>The View Model must be a Page</remarks>
    public abstract class ComponentPresentationsAttributeBase : ModelPropertyAttributeBase //For use in a PageModel
    {
        //Really leaving the bulk of the work to implementer -- they must both find out if the CP matches this attribute and then construct an object with it
        /// <summary>
        /// When overriden in a derived class, this gets a set of values representing Component Presentations of a Page
        /// </summary>
        /// <param name="cps">Component Presentations for the Page Model</param>
        /// <param name="propertyType">Actual return type of the Property</param>
        /// <param name="factory">A View Model factory</param>
        /// <returns>The Property value</returns>
        public abstract IEnumerable GetPresentationValues(IList<IComponentPresentationData> cps, IModelProperty property, IViewModelFactory factory = null);

        public override IEnumerable GetPropertyValues(IViewModelData modelData, IModelProperty property, IViewModelFactory factory = null)
        {
            IEnumerable result = null;
            if (modelData is IPageData)
            {
                var cpModels = (modelData as IPageData).ComponentPresentations;
                result = GetPresentationValues(cpModels, property, factory);
            }
            return result;
        }
    }


    /// <summary>
    /// An Attribute for identifying a Content View Model
    /// </summary>
    public class ViewModelAttribute : Attribute, IDefinedModelAttribute //Should be re-named ContentViewModelAttribute
    {
        //TODO: De-couple this from the Schema name specifically? What would make sense?
        //TOOD: Possibly change this to use purely ViewModelKey and make that an object, leave it to the key provider to assign objects with logical equals overrides

        private string schemaName;
        private bool inlineEditable = false;
        private bool isDefault = false;
        private string[] viewModelKeys;
        /// <summary>
        /// View Model
        /// </summary>
        /// <param name="schemaName">Tridion schema name for component type for this View Model</param>
        /// <param name="isDefault">Is this the default View Model for this schema. If true, Components
        /// with this schema will use this class if no other View Models' Keys match.</param>
        public ViewModelAttribute(string schemaName, bool isDefault)
        {
            this.schemaName = schemaName;
            this.isDefault = isDefault;
        }

        //Using Schema Name ties each View Model to a single Tridion Schema -- this is probably ok in 99% of cases
        //Using schema name doesn't allow us to de-couple the Model itself from Tridion however (neither does requiring
        //inheritance of IViewModel!)
        //Possible failure: if the same model was meant to represent similar parts of multiple schemas (should however
        //be covered by decent Schema design i.e. use of Embedded Schemas and Linked Components. Same fields shouldn't
        //occur repeatedly)

        public string SchemaName
        {
            get
            {
                return schemaName;
            }
        }

        /// <summary>
        /// Identifiers for further specifying which View Model to use for different presentations.
        /// </summary>
        public string[] ViewModelKeys
        {
            get { return viewModelKeys; }
            set { viewModelKeys = value; }
        }
        /// <summary>
        /// Is inline editable. Only for semantic use.
        /// </summary>
        public bool InlineEditable
        {
            get
            {
                return inlineEditable;
            }
            set
            {
                inlineEditable = value;
            }
        }

        /// <summary>
        /// Is the default View Model for the schema. If set to true, this will be the View Model to use for a given schema if no View Model ID is specified.
        /// </summary>
        public bool IsDefault { get { return isDefault; } }

        public override int GetHashCode()
        {
            return base.GetHashCode(); //no need to override the hash code
        }
        public override bool Equals(object obj)
        {
            if (obj != null && obj is ViewModelAttribute)
            {
                ViewModelAttribute key = (ViewModelAttribute)obj;
                if (this.ViewModelKeys != null && key.ViewModelKeys != null)
                {
                    //if both have a ViewModelKey set, use both ViewModelKey and schema
                    //Check for a match anywhere in both lists
                    var match = from i in this.ViewModelKeys
                                join j in key.ViewModelKeys
                                on i equals j
                                select i;
                    //Schema names match and there is a matching view model ID
                    if (this.SchemaName == key.SchemaName && match.Count() > 0)
                        return true;
                }
                //Note: if the parent of a linked component is using a View Model Key, the View Model
                //for that linked component must either be Default with no View Model Keys, or it must
                //have the View Model Key of the parent View Model
                if (((this.ViewModelKeys == null || this.ViewModelKeys.Length == 0) && key.IsDefault) //this set of IDs is empty and the input is default
                    || ((key.ViewModelKeys == null || key.ViewModelKeys.Length == 0) && this.IsDefault)) //input set of IDs is empty and this is default
                //if (key.IsDefault || this.IsDefault) //Fall back to default if the view model key isn't found -- useful for linked components
                {
                    //Just compare the schema names
                    return this.SchemaName == key.SchemaName;
                }
            }
            return false;
        }


        public bool IsMatch(IViewModelData data, IViewModelKeyProvider provider)
        {
            var key = provider.GetViewModelKey(data);
            return IsMatch(data, key);
        }


        public bool IsMatch(IViewModelData data, string key)
        {
            bool result = false;
            if (data is IDefinedData)
            {
                var definedData = data as IDefinedData;
                var compare = new ViewModelAttribute(definedData.Schema.Title, false)
                {
                    ViewModelKeys = key == null ? null : new string[] { key }
                };
                result = this.Equals(compare);
            }
            return result;
        }
    }

    /// <summary>
    /// An Attribute for identifying a Page View Model
    /// </summary>
    public class PageViewModelAttribute : Attribute, IPageModelAttribute
    {
        public PageViewModelAttribute(string[] viewModelKeys)
        {
            ViewModelKeys = viewModelKeys;
        }
        public string[] ViewModelKeys
        {
            get;
            set;
        }

        public bool IsMatch(IViewModelData data, IViewModelKeyProvider provider)
        {
            string key = provider.GetViewModelKey(data);
            return IsMatch(data, key);
        }


        public bool IsMatch(IViewModelData data, string key)
        {

            bool result = false;
            if (data is IPageData)
            {
                var contentData = data as IPageData;
                return ViewModelKeys.Any(x => x.Equals(key));
            }
            return result;
        }
    }

    /// <summary>
    /// An Attribute for identifying a Keyword View Model
    /// </summary>
    public class KeywordViewModelAttribute : Attribute, IKeywordModelAttribute
    {
        public KeywordViewModelAttribute(string[] viewModelKeys)
        {
            ViewModelKeys = viewModelKeys;
        }
        /// <summary>
        /// View Model Keys for this Keyword
        /// </summary>
        /// <remarks>Common View Model Keys for Keywords are Metadata Schema Title or Category Title.</remarks>
        public string[] ViewModelKeys
        {
            get;
            set;
        }
        public bool IsMatch(IViewModelData data, IViewModelKeyProvider provider)
        {
            string key = provider.GetViewModelKey(data);
            return IsMatch(data, key);
        }

        public bool IsMatch(IViewModelData data, string key)
        {
            bool result = false;
            if (data is IKeywordData)
            {
                return ViewModelKeys.Any(x => x.Equals(key));
            }
            return result;
        }
    }

    public abstract class NestedModelFieldAttributeBase : FieldAttributeBase
    {
        public override IEnumerable GetFieldValues(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
        {
            IEnumerable fieldValue = null;
            var values = GetRawValues(field);
            if (values != null)
            {
                if (ComplexTypeMapping == null && ReturnRawData)
                    fieldValue = values;
                else
                    fieldValue = values.Cast<object>()
                        .Select(value => BuildModel(factory, BuildModelData(value, field, template)))
                    .Where(value => value != null);
            }
            return fieldValue;
        }

        protected virtual object BuildModel(IViewModelFactory factory, IViewModelData data)
        {
            object result = null;
            if (ComplexTypeMapping != null)
            {
                result = factory.BuildMappedModel(data, ComplexTypeMapping);
            }
            else
            {
                var modelType = GetModelType(data, factory);
                result = modelType != null ? factory.BuildViewModel(modelType, data) : null;
            }
            return result;
        }

        public abstract IEnumerable GetRawValues(IFieldData field);

        protected abstract IViewModelData BuildModelData(object value, IFieldData field, ITemplateData template);
        protected abstract Type GetModelType(IViewModelData data, IViewModelFactory factory);
        protected abstract bool ReturnRawData { get; }

        public override Type ExpectedReturnType
        {
            get
            {
                if (ComplexTypeMapping != null)
                    return AllowMultipleValues ? typeof(ICollection<>) : typeof(object);
                else return AllowMultipleValues ? typeof(ICollection<IViewModel>) : typeof(IViewModel);
            }
        }
    }
}
