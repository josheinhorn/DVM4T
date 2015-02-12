using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Attributes;
using Dynamic = DD4T.ContentModel;
using DVM4T.Contracts;
using DVM4T.Reflection;
using DD4T.Mvc.Html;
using System.Web.Mvc;

namespace DVM4T.DD4T.Attributes
{
    /// <summary>
    /// A Component Link Field
    /// </summary>
    /// <example>
    /// To create a multi value linked component with a custom return Type:
    ///     [LinkedComponentField("content", LinkedComponentTypes = new Type[] { typeof(GeneralContentViewModel) }, AllowMultipleValues = true)]
    ///     public ViewModelList'GeneralContentViewModel' Content { get; set; }
    ///     
    /// To create a single linked component using the default DD4T type:
    ///     [LinkedComponentField("internalLink")]
    ///     public IComponent InternalLink { get; set; }
    /// </example>
    public class LinkedComponentFieldAttribute : FieldAttributeBase
    {
        protected Type[] linkedComponentTypes;

        /// <summary>
        /// A Linked Component Field
        /// </summary>
        /// <param name="fieldName">Tridion schema field name</param>
        public LinkedComponentFieldAttribute(string fieldName) : base(fieldName) { }
        /// <summary>
        /// The possible return types for this linked component field. Each of these types must implement the 
        /// return type of this property or its generic type if multi-value. If not used, the default DD4T
        /// Component object will be returned.
        /// </summary>
        public Type[] LinkedComponentTypes //Is there anyway to enforce the types passed to this?
        {
            get
            {
                return linkedComponentTypes;
            }
            set
            {
                linkedComponentTypes = value;
            }
        }
        public override object GetFieldValue(IFieldData field, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            var linkedComponentValues = field.Values.Cast<Dynamic.IComponent>().ToList();
            if (linkedComponentValues != null && linkedComponentValues.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    if (linkedComponentTypes == null)
                    {
                        fieldValue = field.Values;
                    }
                    else
                    {
                        var linkedComps = linkedComponentValues.Select(x => new Component(x));
                        //Property must implement IList<IComponentPresentationViewModel> -- use ComponentViewModelList<T>
                        IList<IViewModel> list =
                            (IList<IViewModel>)ReflectionUtility.ReflectionCache.CreateInstance(propertyType);

                        foreach (var component in linkedComps)
                        {
                            var model = BuildLinkedComponent(component, template, builder);
                            if (model != null)
                                list.Add(model);
                        }
                        fieldValue = list;
                    }
                }
                else
                {
                    fieldValue = linkedComponentTypes == null ? (object)linkedComponentValues[0]
                        : (object)BuildLinkedComponent(new Component(linkedComponentValues[0]), template, builder);
                }
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get
            {
                if (AllowMultipleValues)
                {
                    return linkedComponentTypes == null ? typeof(IList<Dynamic.IComponent>) : typeof(IList<IComponentPresentationViewModel>);
                }
                else
                {
                    return linkedComponentTypes == null ? typeof(Dynamic.IComponent) : typeof(IComponentPresentationViewModel);
                }
            }
        }
        private IComponentPresentationViewModel BuildLinkedComponent(IComponentData component, IComponentTemplateData template, IViewModelBuilder builder)
        {
            IComponentPresentationData linkedCp = new ComponentPresentation
            (
                component as Component,
                template as ComponentTemplate
            );
            //need to determine schema to choose the Type
            Type type = GetViewModelType(component.Schema, builder, template);
            //linkedModel = BuildCPViewModel(linkedType, linkedCp);
            if (type == null) return null;
            else return builder.BuildCPViewModel(type, linkedCp);
        }
        private Type GetViewModelType(ISchemaData schema, IViewModelBuilder builder, IComponentTemplateData template = null)
        {
            //Create some algorithm to determine the proper view model type, perhaps build a static collection of all Types with the
            //View Model Attribute and set the key to the schema name + template name?
            if (schema == null) throw new ArgumentNullException("schema");
            //string ctName;
            string viewModelKey = builder.ViewModelKeyProvider.GetViewModelKey(template);
            ViewModelAttribute key = new ViewModelAttribute(schema.Title, false)
            {
                ViewModelKeys = String.IsNullOrEmpty(viewModelKey) ? null : new string[] { viewModelKey }
            };
            foreach (var type in LinkedComponentTypes)
            {
                ViewModelAttribute modelAttr = ReflectionUtility.ReflectionCache.GetCustomAttribute<ViewModelAttribute>(type);

                if (modelAttr != null && key.Equals(modelAttr))
                    return type;
            }
            return null; //no matching types found, return null
            //throw new ViewModelTypeNotFoundExpception(schema.Title, viewModelKey);
        }
    }

    /// <summary>
    /// An embedded schema field
    /// </summary>
    public class EmbeddedSchemaFieldAttribute : FieldAttributeBase
    {
        protected Type embeddedSchemaType;
        /// <summary>
        /// Embedded Schema Field
        /// </summary>
        /// <param name="fieldName">The Tridion schema field name</param>
        /// <param name="embeddedSchemaType">The View Model type for this embedded field set</param>
        public EmbeddedSchemaFieldAttribute(string fieldName, Type embeddedSchemaType)
            : base(fieldName)
        {
            this.embeddedSchemaType = embeddedSchemaType;
        }
        public Type EmbeddedSchemaType
        {
            get
            {
                return embeddedSchemaType;
            }
        }

        public override object GetFieldValue(IFieldData field, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            var embeddedValues = field.Values.Cast<Dynamic.FieldSet>().Select(x => new FieldSet(x)).ToList();
            if (embeddedValues != null && embeddedValues.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    //Property must implement IList<IEmbeddedSchemaViewModel> -- use EmbeddedViewModelList<T>
                    IList<IViewModel> list = (IList<IViewModel>)ReflectionUtility.ReflectionCache.CreateInstance(propertyType);
                    foreach (var fieldSet in embeddedValues)
                    {
                        list.Add(builder.BuildEmbeddedViewModel(
                        EmbeddedSchemaType,
                        fieldSet, template));
                    }
                    fieldValue = list;
                }
                else
                {
                    fieldValue = builder.BuildEmbeddedViewModel(EmbeddedSchemaType, embeddedValues[0], template);
                }
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<IEmbeddedSchemaViewModel>) : typeof(IEmbeddedSchemaViewModel); }
        }
    }

    /// <summary>
    /// A Multimedia component field
    /// </summary>
    public class MultimediaFieldAttribute : FieldAttributeBase
    {
        public MultimediaFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            var mmValues = field.Values.Cast<Dynamic.IComponent>().Select(x => x.Multimedia).ToList();
            if (mmValues != null && mmValues.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    fieldValue = mmValues;
                }
                else
                {
                    fieldValue = mmValues[0];
                }
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<Dynamic.IMultimedia>) : typeof(Dynamic.IMultimedia); }
        }
    }

    /// <summary>
    /// A text field
    /// </summary>
    public class TextFieldAttribute : FieldAttributeBase, ICanBeBoolean
    {
        public TextFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            var values = field.Values.Cast<string>().ToList();
            if (values != null && values.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    if (IsBooleanValue)
                        fieldValue = values.Select(v => { bool b; return bool.TryParse(v, out b) && b; }).ToList();
                    else fieldValue = values;
                }
                else
                {
                    if (IsBooleanValue)
                    {
                        bool b;
                        fieldValue = bool.TryParse(values[0], out b) && b;
                    }
                    else fieldValue = values[0];
                }
            }
            return fieldValue;
        }

        /// <summary>
        /// Set to true to parse the text into a boolean value.
        /// </summary>
        public bool IsBooleanValue { get; set; }
        public override Type ExpectedReturnType
        {
            get
            {
                if (AllowMultipleValues)
                    return IsBooleanValue ? typeof(IList<bool>) : typeof(IList<string>);
                else return IsBooleanValue ? typeof(bool) : typeof(string);
            }
        }
    }

    /// <summary>
    /// A Rich Text field
    /// </summary>
    public class RichTextFieldAttribute : FieldAttributeBase
    {
        public RichTextFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            var values = field.Values.Cast<string>().ToList();
            if (values != null && values.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    fieldValue = values.Select(v => v.ResolveRichText()).ToList();
                }
                else
                {
                    fieldValue = values[0].ResolveRichText();
                }
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<MvcHtmlString>) : typeof(MvcHtmlString); }
        }
    }

    /// <summary>
    /// A Number field
    /// </summary>
    public class NumberFieldAttribute : FieldAttributeBase
    {
        public NumberFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            var values = field.Values.Cast<double>().ToList();
            if (values != null && values.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    fieldValue = values;
                }
                else
                {
                    fieldValue = values[0];
                }
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<double>) : typeof(double); }
        }

    }
    /// <summary>
    /// A Date/Time field
    /// </summary>
    public class DateFieldAttribute : FieldAttributeBase
    {
        public DateFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            var values = field.Values.Cast<DateTime>().ToList();
            if (values != null && values.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    fieldValue = values;
                }
                else
                {
                    fieldValue = values[0];
                }
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<DateTime>) : typeof(DateTime); }
        }
    }
    /// <summary>
    /// A Keyword field
    /// </summary>
    public class KeywordFieldAttribute : FieldAttributeBase
    {
        public KeywordFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            var values = field.Values.Cast<Dynamic.IKeyword>().ToList();
            if (values != null && values.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    fieldValue = values;
                }
                else
                {
                    fieldValue = values[0];
                }
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<Dynamic.IKeyword>) : typeof(Dynamic.IKeyword); }
        }
    }

    /// <summary>
    /// The Key of a Keyword field. 
    /// </summary>
    public class KeywordKeyFieldAttribute : FieldAttributeBase, ICanBeBoolean
    {
        /// <summary>
        /// The Key of a Keyword field.
        /// </summary>
        /// <param name="fieldName">Tridion schema field name</param>
        public KeywordKeyFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            object value = null;
            var values = field.Values.Cast<Dynamic.IKeyword>().ToList();
            if (values != null && values.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    if (IsBooleanValue)
                        value = values.Select(k => { bool b; return bool.TryParse(k.Key, out b) && b; }).ToList();
                    else value = values.Select(k => k.Key);
                }
                else
                {
                    if (IsBooleanValue)
                    {
                        bool b;
                        value = bool.TryParse(values[0].Key, out b) && b;
                    }
                    else value = values[0].Key;
                }
            }
            return value;
        }

        /// <summary>
        /// Set to true to parse the Keyword Key into a boolean value.
        /// </summary>
        public bool IsBooleanValue { get; set; }
        public override Type ExpectedReturnType
        {
            get
            {
                if (AllowMultipleValues)
                    return IsBooleanValue ? typeof(IList<bool>) : typeof(IList<string>);
                else return IsBooleanValue ? typeof(bool) : typeof(string);
            }
        }
    }

    public class NumericKeywordKeyFieldAttribute : FieldAttributeBase
    {
        public NumericKeywordKeyFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            object value = null;
            var values = field.Values.Cast<Dynamic.IKeyword>().ToList();
            if (values != null && values.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    value = values.Select(k => { double i; double.TryParse(k.Key, out i); return i; }).ToList();
                }
                else
                {
                    double i;
                    double.TryParse(values[0].Key, out i);
                    value = i;
                }
            }
            return value;
        }

        public override Type ExpectedReturnType
        {
            get
            {
                return AllowMultipleValues ? typeof(IList<double>) : typeof(double);
            }
        }
    }

    public class MultimediaUrlAttribute : ComponentAttributeBase
    {
        public override object GetPropertyValue(IComponentData component, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            string result = null;
            if (component != null && component.MultimediaData != null)
            {
                result = component.MultimediaData.Url;
            }
            return result;
        }

        public override Type ExpectedReturnType
        {
            get { return typeof(String); }
        }
    }

    public class MultimediaAttribute : ComponentAttributeBase
    {

        public override object GetPropertyValue(IComponentData component, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            IMultimediaData result = null;
            if (component != null && component.MultimediaData != null)
            {
                result = component.MultimediaData;
            }
            return result;
        }

        public override Type ExpectedReturnType
        {
            get { return typeof(IMultimediaData); }
        }
    }

    public class ComponentTitleAttribute : ComponentAttributeBase
    {
        public override object GetPropertyValue(IComponentData component, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            return component.Title;
        }

        public override Type ExpectedReturnType
        {
            get { return typeof(String); }
        }
    }

    public class DD4TMultimediaAttribute : ComponentAttributeBase
    {
        //Example of using the BaseData object

        public override object GetPropertyValue(IComponentData component, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null)
        {
            Dynamic.IMultimedia result = null;
            if (component != null && component.MultimediaData != null)
            {
                result = component.MultimediaData.BaseData as Dynamic.IMultimedia;
            }
            return result;
        }

        public override Type ExpectedReturnType
        {
            get { return typeof(Dynamic.IMultimedia); }
        }
    }

    //A test class for re-using existing TextField in combo with CT Metadata
    public class TemplateMetaTextFieldAttribute : ComponentTemplateMetadataFieldAttributeBase, ICanBeBoolean
    {
        private IFieldAttribute textField;
        public TemplateMetaTextFieldAttribute(string fieldName) : base(fieldName)
        {
            this.textField = new TextFieldAttribute(fieldName);
        }
        public bool IsBooleanValue
        {
            get;
            set;
        }

        protected override IFieldAttribute BaseFieldAttribute
        {
            get { return textField; }
        }

    }

 
}
