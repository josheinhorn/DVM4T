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
using System.Collections;

namespace DVM4T.DD4T.Attributes
{
    /// <summary>
    /// An Attribute for a Property representing the Link Resolved URL for a Linked or Multimedia Component
    /// </summary>
    /// <remarks>Uses the default DD4T GetResolvedUrl helper method</remarks>
    public class ResolvedUrlFieldAttribute : FieldAttributeBase
    {
        //public ResolvedUrlFieldAttribute(string fieldName) : base(fieldName) { }
        public override IEnumerable GetFieldValues(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
        {
            IEnumerable fieldValue = null;
            var linkedComponentValues = field.Values.Cast<Dynamic.IComponent>().ToList();
            if (linkedComponentValues != null && linkedComponentValues.Count > 0)
            {
                //fieldValue = AllowMultipleValues ? (object)linkedComponentValues.Select(x => x.GetResolvedUrl())
                    //: (object)linkedComponentValues[0].GetResolvedUrl();
                fieldValue = linkedComponentValues.Select(x => x.GetResolvedUrl());
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<string>) : typeof(string); }
        }
    }
   
    /// <summary>
    /// A Multimedia component field
    /// </summary>
    public class MultimediaFieldAttribute : FieldAttributeBase
    {
        //public MultimediaFieldAttribute(string fieldName) : base(fieldName) { }
        public override IEnumerable GetFieldValues(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
        {
            IEnumerable fieldValue = null;
            var mmValues = field.Values.Cast<Dynamic.IComponent>().Select(x => x.Multimedia).ToList();
            if (mmValues != null && mmValues.Count > 0)
            {
                fieldValue = mmValues;
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
        //public TextFieldAttribute(string fieldName) : base(fieldName) { }
        public override IEnumerable GetFieldValues(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
        {
            IEnumerable fieldValue = null;
            var values = field.Values.Cast<string>().ToList();
            if (values != null && values.Count > 0)
            {
                //if (AllowMultipleValues)
                //{
                    if (IsBooleanValue)
                        fieldValue = values.Select(v => { bool b; return bool.TryParse(v, out b) && b; }).ToList();
                    else fieldValue = values;
                //}
                //else
                //{
                //    if (IsBooleanValue)
                //    {
                //        bool b;
                //        fieldValue = bool.TryParse(values[0], out b) && b;
                //    }
                //    else fieldValue = values[0];
                //}
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
        //public RichTextFieldAttribute(string fieldName) : base(fieldName) { }
        public override IEnumerable GetFieldValues(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
        {
            IEnumerable fieldValue = null;
            var values = field.Values.Cast<string>().ToList();
            if (values != null && values.Count > 0)
            {
                //if (AllowMultipleValues)
                //{
                    fieldValue = values.Select(v => v.ResolveRichText()).ToList(); //Hidden dependency on DD4T implementation
                //}
                //else
                //{
                //    fieldValue = values[0].ResolveRichText(); //Hidden dependency on DD4T implementation
                //}
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
        //public NumberFieldAttribute(string fieldName) : base(fieldName) { }
        public override IEnumerable GetFieldValues(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
        {
            IEnumerable fieldValue = null;
            var values = field.Values.Cast<double>().ToList();
            if (values != null && values.Count > 0)
            {
                //if (AllowMultipleValues)
                //{
                    fieldValue = values;
                //}
                //else
                //{
                //    fieldValue = values[0];
                //}
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
        //public DateFieldAttribute(string fieldName) : base(fieldName) { }
        public override IEnumerable GetFieldValues(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
        {
            IEnumerable fieldValue = null;
            var values = field.Values.Cast<DateTime>().ToList();
            if (values != null && values.Count > 0)
            {
                //if (AllowMultipleValues)
                //{
                    fieldValue = values;
                //}
                //else
                //{
                //    fieldValue = values[0];
                //}
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
        //public KeywordKeyFieldAttribute(string fieldName) : base(fieldName) { }
        public override IEnumerable GetFieldValues(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
        {
            IEnumerable value = null;
            var values = field.Values.Cast<Dynamic.IKeyword>().ToList();
            if (values != null && values.Count > 0)
            {
                //if (AllowMultipleValues)
                //{
                    if (IsBooleanValue)
                        value = values.Select(k => { bool b; return bool.TryParse(k.Key, out b) && b; }).ToList();
                    else value = values.Select(k => k.Key);
                //}
                //else
                //{
                //    if (IsBooleanValue)
                //    {
                //        bool b;
                //        value = bool.TryParse(values[0].Key, out b) && b;
                //    }
                //    else value = values[0].Key;
                //}
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
        //public NumericKeywordKeyFieldAttribute(string fieldName) : base(fieldName) { }
        public override IEnumerable GetFieldValues(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
        {
            IEnumerable value = null;
            var values = field.Values.Cast<Dynamic.IKeyword>().ToList();
            if (values != null && values.Count > 0)
            {
                //if (AllowMultipleValues)
                //{
                    value = values.Select(k => { double i; double.TryParse(k.Key, out i); return i; }).ToList();
                //}
                //else
                //{
                //    double i;
                //    double.TryParse(values[0].Key, out i);
                //    value = i;
                //}
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
        public override IEnumerable GetPropertyValues(IComponentData component, IModelProperty property, ITemplateData template, IViewModelFactory factory = null)
        {
            IEnumerable result = null;
            if (component != null && component.MultimediaData != null)
            {
                result = new string[] { component.MultimediaData.Url };
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
        public override IEnumerable GetPropertyValues(IComponentData component, IModelProperty property, ITemplateData template, IViewModelFactory factory = null)
        {
            IMultimediaData result = null;
            if (component != null && component.MultimediaData != null)
            {
                result = component.MultimediaData;
            }
            return new IMultimediaData[] { result };
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
        public override IEnumerable GetPropertyValues(IComponentData component, IModelProperty property, ITemplateData template, IViewModelFactory factory = null)
        {
            return new string[] { component.Title };
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

        public override IEnumerable GetPropertyValues(IComponentData component, IModelProperty property, ITemplateData template, IViewModelFactory factory = null)
        {
            Dynamic.IMultimedia result = null;
            if (component != null && component.MultimediaData != null)
            {
                result = component.MultimediaData.BaseData as Dynamic.IMultimedia;
            }
            return new Dynamic.IMultimedia[] { result };
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
        public override System.Collections.IEnumerable GetPresentationValues(IList<IComponentPresentationData> cps, IModelProperty property, IViewModelFactory factory = null)
        {
            //IList<IViewModel> result = factory.ModelResolver.ResolveModel(propertyType) as IList<IViewModel>;
            var result = factory.ModelResolver.ResolveInstance(property.PropertyType);
            var add = property.AddToCollection; //factory.ModelResolver.ReflectionHelper.BuildAddMethod(propertyType);

            string view = null;
            foreach (var cp in cps)
            {
                if (cp.Template != null && cp.Template.Metadata != null && cp.Template.Metadata.ContainsKey("view"))
                {
                    view = cp.Template.Metadata["view"].Values.Cast<string>().FirstOrDefault(); //DD4T Convention for view name
                    if (view != null && view.StartsWith(ViewPrefix))
                    {
                        //result.Add(factory.BuildViewModel((cp)));
                        object model = null;
                        if (ComplexTypeMapping != null)
                        {
                            model = factory.BuildMappedModel(cp, ComplexTypeMapping);
                        }
                        else model = factory.BuildViewModel((cp));
                        add(result, model);
                    }
                }
            }
            return (System.Collections.IEnumerable)result;
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



    public class KeywordDataAttribute : ModelPropertyAttributeBase
    {
        public override IEnumerable GetPropertyValues(IViewModelData modelData, IModelProperty property, IViewModelFactory factory = null)
        {
            IEnumerable result = null;
            if (modelData != null && modelData is IKeywordData)
            {
                result = new IKeywordData[] { modelData as IKeywordData };
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
        //public TextEnumFieldAttribute(string fieldName, Type genericType)
        //    : base(fieldName)
        //{
        //    if (!genericType.IsEnum)
        //    {
        //        throw new ArgumentException("genericType must be an enumerated type");
        //    }
        //    this.genericType = genericType;
        //    genericHelperType = typeof(GenericEnumHelper<>).MakeGenericType(genericType);
        //}
        public override IEnumerable GetFieldValues(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
        {
            genericHelper = factory.ModelResolver.ResolveInstance(genericHelperType);
            safeParse = genericHelperType.GetMethod("SafeParse"); //GARBAGE: Needs reflection optimization
            IEnumerable fieldValue = null;
            var values = field.Values.Cast<string>().ToList();
            if (values != null && values.Count > 0)
            {

                //if (AllowMultipleValues)
                //{
                    var list = (IList<object>)factory.ModelResolver.ResolveInstance(property.ModelType); //A trick to get around generics
                    foreach (var value in values)
                    {
                        list.Add(safeParse.Invoke(genericHelper, new object[] { value }));
                    }
                    fieldValue = list;
                //}
                //else
                //{
                //    fieldValue = safeParse.Invoke(genericHelper, new object[] { values[0] }); //GARBAGE: Needs reflection optimization
                //}
            }
            return fieldValue;
        }

        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<>).MakeGenericType(genericType) : genericType; }
        }

        public class GenericEnumHelper<TEnum> where TEnum : struct
        {
            public TEnum SafeParse(object value)
            {
                TEnum b;
                Enum.TryParse<TEnum>(value.ToString(), out b);
                return b;
            }
        }
    }

  
}
