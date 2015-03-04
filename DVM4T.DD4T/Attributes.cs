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
using DVM4T.Core;
using DVM4T.Exceptions;
using DD4T.ContentModel;
using System.Reflection;

namespace DVM4T.DD4T.Attributes
{
    /// <summary>
    /// A Component Link Field
    /// </summary>
    /// <example>
    /// To create a multi value linked component with a custom return Type:
    ///     [LinkedComponentField("content", LinkedComponentTypes = new Type[] { typeof(GeneralContentViewModel) }, AllowMultipleValues = true)]
    ///     public ViewModelList&lt;GeneralContentViewModel&gt; Content { get; set; }
    ///     
    /// To create a single linked component using the default DD4T type:
    ///     [LinkedComponentField("internalLink")]
    ///     public IComponent InternalLink { get; set; }
    /// </example>
    /// <remarks>
    /// This requires the Property to be a concrete type with a parameterless constructor that implements IList&lt;IViewModel&gt;.
    /// ViewModelList&lt;T&gt; is the recommended adapter to use.
    /// </remarks>
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
        //public IModelMapping<T> Mapping { get; set; } //Can't do this
        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
        {
            object fieldValue = null;
            var linkedComps = field.Values.Cast<Dynamic.IComponent>();
            var linkedComponentValues = linkedComps.Select(x => Dependencies.DataFactory.GetModelData(x)).ToList();
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
                        //Property must implement IList<IComponentPresentationViewModel> -- use ComponentViewModelList<T>
                        var propValue = factory.ModelResolver.ResolveModel(propertyType);
                        IList<IViewModel> vmList = null;
                        //IList<object> objList = null;
                        //if (propValue is IList<IViewModel>)
                        vmList = (IList<IViewModel>)propValue;
                        //else
                        //    objList = (IList<object>)propValue; //will throw InvalidCastException if Property doesn't implement this
                        
                        foreach (var component in linkedComponentValues)
                        {
                            var model = BuildLinkedComponent(component, template, factory);
                            if (model != null)
                            {
                                //if (vmList != null)
                                    vmList.Add(model);
                                //else
                                //    objList.Add(model);
                            }
                        }
                        fieldValue = vmList; // ?? (object)objList;
                    }
                }
                else
                {
                    fieldValue = linkedComponentTypes == null ? (object)linkedComps.First()
                        : (object)BuildLinkedComponent(linkedComponentValues[0], template, factory);
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
                    return linkedComponentTypes == null ? typeof(IList<Dynamic.IComponent>) : typeof(IList<object>);
                }
                else
                {
                    return linkedComponentTypes == null ? typeof(Dynamic.IComponent) : typeof(object);
                }
            }
        }
        private IViewModel BuildLinkedComponent(IComponentData component, ITemplateData template, IViewModelFactory factory)
        {
            IComponentPresentationData linkedCp = Dependencies.DataFactory.GetModelData(
                Dependencies.DD4TFactory.BuildComponentPresentation(component, template)); //Lots of dependencies here
            Type type = GetViewModelType(linkedCp, factory);
            if (type == null) return null;
            else return factory.BuildViewModel(type, linkedCp);
        }
        private Type GetViewModelType(IComponentPresentationData cp, IViewModelFactory factory)
        {
            //Create some algorithm to determine the proper view model type, perhaps build a static collection of all Types with the
            //View Model Attribute and set the key to the schema name + template name?
            if (cp == null) throw new ArgumentNullException("cp");
            //string ctName;
            Type result = null;
            //if (LinkedComponentTypes.Length > 1)
            //{
                try
                {
                    result = factory.FindViewModelByAttribute<IDefinedModelAttribute>(
                        cp, LinkedComponentTypes);
                }
                catch (ViewModelTypeNotFoundException)
                {
                    result = null; //return null if it's not found instead of throwing exception
                }
            //}
            //else if (LinkedComponentTypes.Length == 1)
            //{
            //    result = linkedComponentTypes[0]; 
            //}
            return result;
        }
    }
    /// <summary>
    /// An Attribute for a Property representing the Link Resolved URL for a Linked or Multimedia Component
    /// </summary>
    /// <remarks>Uses the default DD4T GetResolvedUrl helper method</remarks>
    public class ResolvedUrlFieldAttribute : FieldAttributeBase
    {
        public ResolvedUrlFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
        {
            object fieldValue = null;
            var linkedComponentValues = field.Values.Cast<Dynamic.IComponent>().ToList();
            if (linkedComponentValues != null && linkedComponentValues.Count > 0)
            {
                fieldValue = AllowMultipleValues ? (object)linkedComponentValues.Select(x => x.GetResolvedUrl())
                    : (object)linkedComponentValues[0].GetResolvedUrl();
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<string>) : typeof(string); }
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

        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
        {
            object fieldValue = null;
            var embeddedValues = field.Values.Cast<Dynamic.IFieldSet>().ToList();
            if (embeddedValues != null && embeddedValues.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    //Property must implement IList<IEmbeddedSchemaViewModel> -- use ViewModelList<T>
                    //IList<IViewModel> list = (IList<IViewModel>)factory.ModelResolver.ResolveModel(propertyType); //Dependency!! Get this out
                    var propValue = factory.ModelResolver.ResolveModel(propertyType); //hidden dependency!! get this out
                    //How to not copy and paste all this identical code? 2 different generic lists...

                    IList<IViewModel> vmList = null;
                    
                    vmList = (IList<IViewModel>)propValue;
                    
                    
                    foreach (var fieldSet in embeddedValues)
                    {
                        var model = factory.BuildViewModel(
                            EmbeddedSchemaType,
                            Dependencies.DataFactory.GetModelData(
                                fieldSet,
                                field.EmbeddedSchema.BaseData as ISchema, //Assuming it's DD4T implemented
                                template.BaseData as IComponentTemplate));  //Assuming it's DD4T implemented
                        if (model != null)
                        {
                            vmList.Add(model);
                        }
                    }
                    fieldValue = vmList;
                }
                else
                {
                    fieldValue = factory.BuildViewModel(EmbeddedSchemaType,
                        Dependencies.DataFactory.GetModelData(
                                embeddedValues[0],
                                field.EmbeddedSchema.BaseData as ISchema, //Assuming it's DD4T implemented
                                template.BaseData as IComponentTemplate));  //Assuming it's DD4T implemented
                }
            }
            return fieldValue;
        }
       
        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<IViewModel>) : typeof(IViewModel); }
        }
    }
    /// <summary>
    /// A Multimedia component field
    /// </summary>
    public class MultimediaFieldAttribute : FieldAttributeBase
    {
        public MultimediaFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
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
        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
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
        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
        {
            object fieldValue = null;
            var values = field.Values.Cast<string>().ToList();
            if (values != null && values.Count > 0)
            {
                if (AllowMultipleValues)
                {
                    fieldValue = values.Select(v => v.ResolveRichText()).ToList(); //Hidden dependency on DD4T implementation
                }
                else
                {
                    fieldValue = values[0].ResolveRichText(); //Hidden dependency on DD4T implementation
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
        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
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
        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
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
    /// The Key of a Keyword field. 
    /// </summary>
    public class KeywordKeyFieldAttribute : FieldAttributeBase, ICanBeBoolean
    {
        /// <summary>
        /// The Key of a Keyword field.
        /// </summary>
        /// <param name="fieldName">Tridion schema field name</param>
        public KeywordKeyFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
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
    /// <summary>
    /// The Key of a Keyword as a number
    /// </summary>
    public class NumericKeywordKeyFieldAttribute : FieldAttributeBase
    {
        public NumericKeywordKeyFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
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
    /// <summary>
    /// The URL of the Multimedia data of the view model
    /// </summary>
    public class MultimediaUrlAttribute : ComponentAttributeBase
    {
        public override object GetPropertyValue(IComponentData component, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
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
    /// <summary>
    /// The Multimedia data of the view model
    /// </summary>
    public class MultimediaAttribute : ComponentAttributeBase
    {
        public override object GetPropertyValue(IComponentData component, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
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
    /// <summary>
    /// The title of the Component (if the view model represents a Component)
    /// </summary>
    public class ComponentTitleAttribute : ComponentAttributeBase
    {
        public override object GetPropertyValue(IComponentData component, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
        {
            return component.Title;
        }

        public override Type ExpectedReturnType
        {
            get { return typeof(String); }
        }
    }
    /// <summary>
    /// A DD4T IMultimedia object representing the multimedia data of the model
    /// </summary>
    public class DD4TMultimediaAttribute : ComponentAttributeBase
    {
        //Example of using the BaseData object

        public override object GetPropertyValue(IComponentData component, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
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
    /// <summary>
    /// Component Presentations filtered by the DD4T CT Metadata "view" field
    /// </summary>
    /// <remarks>Use with ViewModelList</remarks>
    public class PresentationsByViewAttribute : ComponentPresentationsAttributeBase
    {
        public override System.Collections.IEnumerable GetPresentationValues(IList<IComponentPresentationData> cps, Type propertyType, IViewModelFactory factory = null)
        {
            IList<IViewModel> result = factory.ModelResolver.ResolveModel(propertyType) as IList<IViewModel>;
            string view = null;
            foreach (var cp in cps)
            {
                if (cp.Template != null && cp.Template.Metadata != null && cp.Template.Metadata.ContainsKey("view"))
                {
                    view = cp.Template.Metadata["view"].Values.Cast<string>().FirstOrDefault(); //DD4T Convention for view name
                    if (view != null && view.StartsWith(ViewPrefix))
                    {
                        result.Add(factory.BuildViewModel((cp)));
                    }
                }
            }
            return result;
        }
        private readonly string viewStartsWith;
        public PresentationsByViewAttribute(string viewStartsWith)
        {
            this.viewStartsWith = viewStartsWith;
        }

        public string ViewPrefix { get { return viewStartsWith; } }
        public override Type ExpectedReturnType
        {
            get { return typeof(IList<IViewModel>); }
        }
    }

    public class KeywordFieldAttribute : FieldAttributeBase
    {
        public KeywordFieldAttribute(string fieldName)
            : base(fieldName)
        {
        }
        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
        {
            object fieldValue = null;
            if (field != null)
            {
                var keywords = field.Values.Cast<Dynamic.IKeyword>().ToList();
                string categoryName = null;
                var dd4tField = field.BaseData as IField;
                if (dd4tField != null)
                {
                    categoryName = dd4tField.CategoryName;
                }
                if (keywords != null && keywords.Count > 0)
                {
                    if (AllowMultipleValues)
                    {
                        if (KeywordType == null)
                            fieldValue = keywords;
                        else
                        {
                            //Property must implement IList<IEmbeddedSchemaViewModel> -- use ViewModelList<T>
                            IList<IViewModel> list = (IList<IViewModel>)factory.ModelResolver.ResolveModel(propertyType);
                            foreach (var keyword in keywords)
                            {
                                list.Add(factory.BuildViewModel(
                                    KeywordType,
                                    Dependencies.DataFactory.GetModelData(keyword, categoryName)));
                            }
                            fieldValue = list;
                        }
                    }
                    else
                    {
                        fieldValue = KeywordType == null ? (object)keywords[0] :
                            factory.BuildViewModel(KeywordType, Dependencies.DataFactory.GetModelData(keywords[0], categoryName));
                    }
                }
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get
            { //return AllowMultipleValues ? typeof(IList<IViewModel>) : typeof(IViewModel); }

                if (AllowMultipleValues)
                {
                    return KeywordType == null ? typeof(IList<Dynamic.IKeyword>) : typeof(IList<IViewModel>);
                }
                else
                {
                    return KeywordType == null ? typeof(Dynamic.IKeyword) : typeof(IViewModel);
                }
            }
        }

        public Type KeywordType
        {
            get;
            set;
        }
    }

    public class KeywordDataAttribute : ModelPropertyAttributeBase
    {
        public override object GetPropertyValue(IViewModelData modelData, Type propertyType, IViewModelFactory factory = null)
        {
            object result = null;
            if (modelData != null && modelData is IKeywordData)
            {
                result = modelData as IKeywordData;
            }
            return result;
        }

        public override Type ExpectedReturnType
        {
            get { return typeof(IKeywordData); }
        }
    }
    
    /// <summary>
    /// Text field that is parsed into an Enum. Currently NOT optimized - relies heavily on reflection methods.
    /// Use sparingly - this is more or less a Proof of Concept.
    /// </summary>
    /// <remarks>
    /// Requires Property to be a concrete type with a parameterless constructor that implements IList&lt;object&gt;
    /// </remarks>
    public class TextEnumFieldAttribute : FieldAttributeBase
    {
        private Type genericType;
        private object genericHelper;
        private MethodInfo safeParse; //GARBAGE
        private Type genericHelperType;
        public TextEnumFieldAttribute(string fieldName, Type genericType) : base(fieldName)
        {
            if (!genericType.IsEnum)
            {
                throw new ArgumentException("genericType must be an enumerated type");
            }
            this.genericType = genericType;
            genericHelperType = typeof(GenericEnumHelper<>).MakeGenericType(genericType);
        }
        public override object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelFactory factory = null)
        {
            genericHelper = factory.ModelResolver.ResolveModel(genericHelperType);
            safeParse = genericHelperType.GetMethod("SafeParse"); //GARBAGE: Needs reflection optimization
            object fieldValue = null;
            var values = field.Values.Cast<string>().ToList();
            if (values != null && values.Count > 0)
            {
                
                if (AllowMultipleValues)
                {
                    var list = (IList<object>)factory.ModelResolver.ResolveModel(propertyType); //A trick to get around generics
                    foreach (var value in values)
                    {
                        list.Add(safeParse.Invoke(genericHelper, new object[] { value }));
                    }
                    fieldValue = list;
                }
                else
                {
                    fieldValue = safeParse.Invoke(genericHelper, new object[] { values[0] }); //GARBAGE: Needs reflection optimization
                }
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<>).MakeGenericType(genericType) : genericType; }
        }

        public class GenericEnumHelper<TEnum> where TEnum: struct
        {
            public TEnum SafeParse(object value)
            {
                TEnum b;
                Enum.TryParse<TEnum>(value.ToString(), out b);
                return b; 
            }
        }
    }
    
    public class EmbeddedModelAttribute<T> : NestedModelFieldAttributeBase<T> where T: class
    {
        public EmbeddedModelAttribute(string fieldName, IModelMapping<T> mapping)
            : base(fieldName, mapping)
        { }
       
        public override object[] GetValues(IFieldData field)
        {
           return field.Values.Cast<Dynamic.IFieldSet>().ToArray();
        }

        public override object BuildModel(IViewModelFactory factory, object value, IFieldData field, ITemplateData template)
        {
            return factory.BuildMappedModel<T>(
                            Dependencies.DataFactory.GetModelData(
                                (IFieldSet)value,
                                field.EmbeddedSchema.BaseData as ISchema, //Assuming it's DD4T implemented
                                template.BaseData as IComponentTemplate),
                                ModelMapping);  //Assuming it's DD4T implemented
        }
    }

    public class LinkedModelAttribute<T> : NestedModelFieldAttributeBase<T> where T : class
    {
        public LinkedModelAttribute(string fieldName, IModelMapping<T> mapping)
            : base(fieldName, mapping)
        { }
        public override object[] GetValues(IFieldData field)
        {
            var linkedComps = field.Values.Cast<Dynamic.IComponent>();
            return linkedComps.Select(x => Dependencies.DataFactory.GetModelData(x)).ToArray();
        }

        public override object BuildModel(IViewModelFactory factory, object value, IFieldData field, ITemplateData template)
        {
            var component = (IComponentData)value;
            IComponentPresentationData linkedCp = Dependencies.DataFactory.GetModelData(
                Dependencies.DD4TFactory.BuildComponentPresentation(component, template)); //Lots of dependencies here
            return factory.BuildMappedModel<T>(linkedCp, ModelMapping);
        }
    }

    public class KeywordModelAttribute<T> : NestedModelFieldAttributeBase<T> where T : class
    {
        public KeywordModelAttribute(string fieldName, IModelMapping<T> mapping)
            : base(fieldName, mapping)
        { }
        public override object[] GetValues(IFieldData field)
        {
            return field.Values.Cast<Dynamic.IKeyword>().ToArray();
        }

        public override object BuildModel(IViewModelFactory factory, object value, IFieldData field, ITemplateData template)
        {
            var keyword = (IKeyword)value;
            var categoryName = (field.BaseData as IField).CategoryName;
            return factory.BuildMappedModel<T>(
                            Dependencies.DataFactory.GetModelData(keyword, categoryName),
                            ModelMapping);
        }
    }
}
