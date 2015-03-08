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
    /// An embedded schema field
    /// </summary>
    public class EmbeddedSchemaFieldAttribute : NestedModelFieldAttributeBase
    {
        //public EmbeddedSchemaFieldAttribute(string fieldName, Type embeddedModelType)
        //    : base(fieldName)
        //{
        //    EmbeddedModelType = embeddedModelType;
        //}

        public override IEnumerable GetRawValues(IFieldData field)
        {
            return field.Values.Cast<Dynamic.IFieldSet>();
        }

        public Type EmbeddedModelType
        {
            get;
            set;
        }

        protected override IViewModelData BuildModelData(object value, IFieldData field, ITemplateData template)
        {
            return Dependencies.DataFactory.GetModelData(
                                (IFieldSet)value,
                                field.EmbeddedSchema.BaseData as ISchema, //Assuming it's DD4T implemented
                                template.BaseData as IComponentTemplate);
        }

        protected override Type GetModelType(IViewModelData data, IViewModelFactory factory)
        {
            return EmbeddedModelType;
        }

        protected override bool ReturnRawData
        {
            get { return false; }
        }
    }

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
    public class LinkedComponentFieldAttribute : NestedModelFieldAttributeBase
    {
     
        //public LinkedComponentFieldAttribute(string fieldName)
        //    : base(fieldName)
        //{ }
        //public LinkedComponentFieldAttribute(string fieldName, Type[] linkedComponentTypes)
        //    : base(fieldName)
        //{
        //    LinkedComponentTypes = linkedComponentTypes;
        //}

        public override IEnumerable GetRawValues(IFieldData field)
        {
            return field.Values.Cast<Dynamic.IComponent>();
            //return linkedComps.Select(x => Dependencies.DataFactory.GetModelData(x));
        }
        public Type[] LinkedComponentTypes //Is there anyway to enforce the types passed to this?
        {
            get;
            set;
        }
        protected override IViewModelData BuildModelData(object value, IFieldData field, ITemplateData template)
        {
            var component = (IComponent)value;
            var componentData = Dependencies.DataFactory.GetModelData(component);
            return Dependencies.DataFactory.GetModelData(
                Dependencies.DD4TFactory.BuildComponentPresentation(componentData, template));
        }

        protected override Type GetModelType(IViewModelData data, IViewModelFactory factory)
        {
            Type result = null;
            if (!ReturnRawData)
            {
                try
                {
                    result = factory.FindViewModelByAttribute<IDefinedModelAttribute>(data, LinkedComponentTypes);
                }
                catch (ViewModelTypeNotFoundException)
                {
                    result = null;
                }
            }
            return result;
        }

        protected override bool ReturnRawData
        {
            get
            {
                return LinkedComponentTypes == null;
            }
        }
    }

    public class KeywordFieldAttribute : NestedModelFieldAttributeBase
    {
        //public KeywordFieldAttribute(string fieldName)
        //    : base(fieldName)
        //{ }
        public override IEnumerable GetRawValues(IFieldData field)
        {
            return field.Values.Cast<Dynamic.IKeyword>();
        }
        public Type KeywordType { get; set; }
        protected override IViewModelData BuildModelData(object value, IFieldData field, ITemplateData template)
        {
            var keyword = (IKeyword)value;
            var categoryName = (field.BaseData as IField).CategoryName;
            return Dependencies.DataFactory.GetModelData(keyword, categoryName);
        }

        protected override Type GetModelType(IViewModelData data, IViewModelFactory factory)
        {
            return KeywordType;
        }

        protected override bool ReturnRawData
        {
            get { return KeywordType == null; }
        }
    }


#region Deprecated
    //public class KeywordFieldAttribute : FieldAttributeBase
    //{
    //    public KeywordFieldAttribute(string fieldName)
    //        : base(fieldName)
    //    {
    //    }
    //    public override IEnumerable GetFieldValue(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
    //    {
    //        object fieldValue = null;
    //        if (field != null)
    //        {
    //            var keywords = field.Values.Cast<Dynamic.IKeyword>().ToList();
    //            string categoryName = null;
    //            var dd4tField = field.BaseData as IField;
    //            if (dd4tField != null)
    //            {
    //                categoryName = dd4tField.CategoryName;
    //            }
    //            if (keywords != null && keywords.Count > 0)
    //            {
    //                if (AllowMultipleValues)
    //                {
    //                    if (KeywordType == null)
    //                        fieldValue = keywords;
    //                    else
    //                    {
    //                        //Property must implement IList<IEmbeddedSchemaViewModel> -- use ViewModelList<T>
    //                        IList<IViewModel> list = (IList<IViewModel>)factory.ModelResolver.ResolveModel(propertyType);
    //                        foreach (var keyword in keywords)
    //                        {
    //                            list.Add(factory.BuildViewModel(
    //                                KeywordType,
    //                                Dependencies.DataFactory.GetModelData(keyword, categoryName)));
    //                        }
    //                        fieldValue = list;
    //                    }
    //                }
    //                else
    //                {
    //                    fieldValue = KeywordType == null ? (object)keywords[0] :
    //                        factory.BuildViewModel(KeywordType, Dependencies.DataFactory.GetModelData(keywords[0], categoryName));
    //                }
    //            }
    //        }
    //        return fieldValue;
    //    }

    //    public override Type ExpectedReturnType
    //    {
    //        get
    //        { //return AllowMultipleValues ? typeof(IList<IViewModel>) : typeof(IViewModel); }

    //            if (AllowMultipleValues)
    //            {
    //                return KeywordType == null ? typeof(IList<Dynamic.IKeyword>) : typeof(IList<IViewModel>);
    //            }
    //            else
    //            {
    //                return KeywordType == null ? typeof(Dynamic.IKeyword) : typeof(IViewModel);
    //            }
    //        }
    //    }

    //    public Type KeywordType
    //    {
    //        get;
    //        set;
    //    }
    //}

    
    //public class LinkedComponentFieldAttribute : FieldAttributeBase
    //{
    //    public LinkedComponentFieldAttribute()
    //        : base(null)
    //    { }
    //    protected Type[] linkedComponentTypes;

    //    /// <summary>
    //    /// A Linked Component Field
    //    /// </summary>
    //    /// <param name="fieldName">Tridion schema field name</param>
    //    public LinkedComponentFieldAttribute(string fieldName) : base(fieldName) { }
    //    /// <summary>
    //    /// The possible return types for this linked component field. Each of these types must implement the 
    //    /// return type of this property or its generic type if multi-value. If not used, the default DD4T
    //    /// Component object will be returned.
    //    /// </summary>
    //    public Type[] LinkedComponentTypes //Is there anyway to enforce the types passed to this?
    //    {
    //        get
    //        {
    //            return linkedComponentTypes;
    //        }
    //        set
    //        {
    //            linkedComponentTypes = value;
    //        }
    //    }
    //    //public IModelMapping<T> Mapping { get; set; } //Can't do this
    //    public override IEnumerable GetFieldValue(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
    //    {
    //        object fieldValue = null;
    //        var linkedComps = field.Values.Cast<Dynamic.IComponent>();
    //        var linkedComponentValues = linkedComps.Select(x => Dependencies.DataFactory.GetModelData(x)).ToList();
    //        if (linkedComponentValues != null && linkedComponentValues.Count > 0)
    //        {
    //            if (AllowMultipleValues)
    //            {
    //                if (ComplexTypeMapping == null && linkedComponentTypes == null)
    //                {
    //                    fieldValue = field.Values;
    //                }
    //                else
    //                {
    //                    //Property must implement ICollection<> and must be a concrete type

    //                    var propValue = factory.ModelResolver.ResolveModel(propertyType);
    //                    var add = factory.ModelResolver.ReflectionHelper.BuildAddMethod(propertyType);

    //                    //IList<IViewModel> vmList = null;
    //                    //vmList = (IList<IViewModel>)propValue;

    //                    foreach (var component in linkedComponentValues)
    //                    {
    //                        var model = BuildLinkedComponent(component, template, factory);
    //                        if (model != null)
    //                        {
    //                            //vmList.Add(model);
    //                            add(propValue, model);
    //                        }
    //                    }
    //                    fieldValue = propValue;
    //                }
    //            }
    //            else
    //            {
    //                fieldValue = linkedComponentTypes == null ? (object)linkedComps.First()
    //                    : (object)BuildLinkedComponent(linkedComponentValues[0], template, factory);
    //            }
    //        }
    //        return fieldValue;
    //    }

    //    public override Type ExpectedReturnType
    //    {
    //        get
    //        {
    //            if (AllowMultipleValues)
    //            {
    //                return linkedComponentTypes == null ? typeof(IList<Dynamic.IComponent>) : typeof(IList<object>);
    //            }
    //            else
    //            {
    //                return linkedComponentTypes == null ? typeof(Dynamic.IComponent) : typeof(object);
    //            }
    //        }
    //    }
    //    private object BuildLinkedComponent(IComponentData component, ITemplateData template, IViewModelFactory factory)
    //    {
    //        IComponentPresentationData linkedCp = Dependencies.DataFactory.GetModelData(
    //            Dependencies.DD4TFactory.BuildComponentPresentation(component, template)); //Lots of dependencies here
    //        object result = null;
    //        if (ComplexTypeMapping != null) //This attribute was built with the Binding namespace and should use mappings instead of attributes
    //        {
    //            result = factory.BuildMappedModel(linkedCp, ComplexTypeMapping);
    //        }
    //        else
    //        {
    //            Type type = GetViewModelType(linkedCp, factory);
    //            if (type == null) return null;
    //            else result = factory.BuildViewModel(type, linkedCp);
    //        }
    //        return result;
    //    }
    //    private Type GetViewModelType(IComponentPresentationData cp, IViewModelFactory factory)
    //    {
    //        //Create some algorithm to determine the proper view model type, perhaps build a static collection of all Types with the
    //        //View Model Attribute and set the key to the schema name + template name?
    //        if (cp == null) throw new ArgumentNullException("cp");
    //        //string ctName;
    //        Type result = null;
    //        //if (LinkedComponentTypes.Length > 1)
    //        //{
    //        try
    //        {
    //            result = factory.FindViewModelByAttribute<IDefinedModelAttribute>(
    //                cp, LinkedComponentTypes);
    //        }
    //        catch (ViewModelTypeNotFoundException)
    //        {
    //            result = null; //return null if it's not found instead of throwing exception
    //        }
    //        //}
    //        //else if (LinkedComponentTypes.Length == 1)
    //        //{
    //        //    result = linkedComponentTypes[0]; 
    //        //}
    //        return result;
    //    }
    //}

   
    //public class EmbeddedSchemaFieldAttribute : FieldAttributeBase
    //{
    //    protected Type embeddedSchemaType;
    //    /// <summary>
    //    /// Embedded Schema Field
    //    /// </summary>
    //    /// <param name="fieldName">The Tridion schema field name</param>
    //    /// <param name="embeddedSchemaType">The View Model type for this embedded field set</param>
    //    public EmbeddedSchemaFieldAttribute(string fieldName, Type embeddedSchemaType)
    //        : base(fieldName)
    //    {
    //        this.embeddedSchemaType = embeddedSchemaType;
    //    }
    //    public Type EmbeddedSchemaType
    //    {
    //        get
    //        {
    //            return embeddedSchemaType;
    //        }
    //    }

    //    public override IEnumerable GetFieldValue(IFieldData field, IModelProperty property, ITemplateData template,IViewModelFactory factory = null)
    //    {
    //        object fieldValue = null;
    //        var embeddedValues = field.Values.Cast<Dynamic.IFieldSet>().ToList();
    //        if (embeddedValues != null && embeddedValues.Count > 0)
    //        {
    //            if (AllowMultipleValues)
    //            {
    //                //Property must implement IList<IEmbeddedSchemaViewModel> -- use ViewModelList<T>
    //                //IList<IViewModel> list = (IList<IViewModel>)factory.ModelResolver.ResolveModel(propertyType); //Dependency!! Get this out
    //                var propValue = factory.ModelResolver.ResolveInstance(propertyType); //hidden dependency!! get this out
    //                var add = factory.ModelResolver.ReflectionHelper.BuildAddMethod(propertyType);

    //                //How to not copy and paste all this identical code? 2 different generic lists...

    //                //IList<IViewModel> vmList = null;
    //                //vmList = (IList<IViewModel>)propValue;

    //                foreach (var fieldSet in embeddedValues)
    //                {
    //                    var model = BuildModel(factory, fieldSet, field, template);
    //                    if (model != null)
    //                    {
    //                        //vmList.Add(model);
    //                        add(propValue, model);
    //                    }
    //                }
    //                fieldValue = propValue;
    //            }
    //            else
    //            {
    //                fieldValue = BuildModel(factory, embeddedValues[0], field, template);  //Assuming it's DD4T implemented
    //            }
    //        }
    //        return fieldValue;
    //    }
    //    private object BuildModel(IViewModelFactory factory, IFieldSet fieldSet, IFieldData field, ITemplateData template)
    //    {
    //        object result = null;
    //        IViewModelData data = Dependencies.DataFactory.GetModelData(
    //                    fieldSet,
    //                    field.EmbeddedSchema.BaseData as ISchema, //Assuming it's DD4T implemented
    //                    template.BaseData as IComponentTemplate); //Assuming it's DD4T implemented
    //        if (ComplexTypeMapping != null)
    //        {
    //            result = factory.BuildMappedModel(data, ComplexTypeMapping);
    //        }
    //        else
    //        {
    //            result = factory.BuildViewModel(EmbeddedSchemaType, data);
    //        }
    //        return result;
    //    }
    //    public override Type ExpectedReturnType
    //    {
    //        get { return AllowMultipleValues ? typeof(IList<IViewModel>) : typeof(IViewModel); }
    //    }
    //}
#endregion
}
