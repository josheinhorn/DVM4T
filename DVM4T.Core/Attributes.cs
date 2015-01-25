using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Contracts;
using System.Reflection;
using System.Web;
using DVM4T.Reflection;
using DVM4T.Exceptions;

namespace DVM4T.Attributes
{
    /// <summary>
    /// The Base class for all Field Attributes. Inherit this class to create custom attributes for decorating Domain View Models.
    /// </summary>
    public abstract class FieldAttributeBase : Attribute, IFieldAttribute
    {
        protected readonly string fieldName;
        protected bool allowMultipleValues = false;
        protected bool inlineEditable = false;
        protected bool mandatory = false; //probably don't need this one
        protected bool isMetadata = false;
        /// <summary>
        /// Base Constructor
        /// </summary>
        /// <param name="fieldName">The Tridion schema field name for this property</param>
        public FieldAttributeBase(string fieldName)
        {
            this.fieldName = fieldName;
        }
        /// <summary>
        /// When overriden in a derived class, this method should return the value of the View Model property from a Field object
        /// </summary>
        /// <param name="field">The Field</param>
        /// <param name="propertyType">The concrete type of the view model property for this attribute</param>
        /// <param name="template">The Component Template to use</param>
        /// <param name="builder">The View Model Builder</param>
        /// <returns></returns>
        public abstract object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null);
        /// <summary>
        /// When overriden in a derived class, this property returns the expected return type of the View Model property.
        /// </summary>
        /// <remarks>Primarily used for debugging purposes. This property is used to throw an accurate exception at run time if
        /// the property return type does not match with the expected type.</remarks>
        public abstract Type ExpectedReturnType { get; }
        /// <summary>
        /// The Tridion schema field name for this property
        /// </summary>
        public string FieldName { get { return fieldName; } }
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
    }
    /// <summary>
    /// A View Model.
    /// </summary>
    public class ViewModelAttribute : Attribute, IViewModelAttribute
    {
        private string schemaName;
        private bool inlineEditable = false;
        private bool isDefault = false;
        private string componentTemplateName;
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

        public string SchemaName
        {
            get
            {
                return schemaName;
            }
        }

        /// <summary>
        /// The name of the Component Template. For semantic purposes only.
        /// </summary>
        public string ComponentTemplateName //TODO: Use custom CT Metadata fields instead of CT Name
        {
            get { return componentTemplateName; }
            set { componentTemplateName = value; }
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
    }


}
