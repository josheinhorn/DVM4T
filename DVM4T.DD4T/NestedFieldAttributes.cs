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
    ///     [LinkedComponentField(FieldName = "content", LinkedComponentTypes = new Type[] { typeof(GeneralContentViewModel) }, AllowMultipleValues = true)]
    ///     public ViewModelList&lt;GeneralContentViewModel&gt; Content { get; set; }
    ///     
    /// To create a single linked component using the default DD4T type:
    ///     [LinkedComponentField(FieldName = "internalLink")]
    ///     public IComponent InternalLink { get; set; }
    /// </example>
    /// <remarks>
    /// This requires the Property to be a concrete type with a constructor that implements ICollection&lt;T&gt; or is T[]
    /// </remarks>
    public class LinkedComponentFieldAttribute : NestedModelFieldAttributeBase
    {
        public override IEnumerable GetRawValues(IFieldData field)
        {
            return field.Values.Cast<Dynamic.IComponent>();
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
            try
            {
                result = factory.FindViewModelByAttribute<IDefinedModelAttribute>(data, LinkedComponentTypes);
            }
            catch (ViewModelTypeNotFoundException)
            {
                result = null;
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
}
