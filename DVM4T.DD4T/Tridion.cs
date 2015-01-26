using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM = DVM4T.Contracts;
using DD4T.ContentModel;
using System.Collections;
namespace DVM4T.DD4T
{
    public abstract class TridionItemBase : DVM.ITridionItem
    {
        protected abstract IItem Item { get; }

        public string TcmUri
        {
            get
            {
                return Item.Id;
            }
        }

        public string Title
        {
            get
            {
                return Item.Title;
            }
        }

        public int PublicationId
        {
            get
            {
                return new TcmUri(Item.Id).PublicationId;
            }
        }

    }

    public class Schema : TridionItemBase, DVM.ISchema
    {
        private ISchema schema;
        protected override IItem Item
        {
            get { return schema; }
        }

        public Schema(ISchema schema)
        {
            this.schema = schema;
        }

        public string RootElementName
        {
            get { return schema.RootElementName; }
        }
    }
    public class Field : DVM.IField
    {
        private IField field;
        private DVM.ISchema embeddedSchema;
        private string fieldType;
        private string xpath;
        private string name;

        public Field(IField field)
        {
            this.field = field;
            this.embeddedSchema = field.EmbeddedSchema != null ? new Schema(field.EmbeddedSchema)
                : null;
            this.fieldType = field.FieldType.ToString();
            this.xpath = field.XPath;
            this.name = field.Name;
        }
        public IEnumerable Value
        {
            get
            {
                IEnumerable value = null;
                switch (field.FieldType)
                {
                    case FieldType.ComponentLink:
                        value = field.LinkedComponentValues;
                        break;
                    case FieldType.Date:
                        value = field.DateTimeValues;
                        break;
                    case FieldType.Embedded:
                        value = field.EmbeddedValues;
                        break;
                    case FieldType.ExternalLink:
                        value = field.Values;
                        break;
                    case FieldType.Keyword:
                        value = field.Keywords;
                        break;
                    case FieldType.MultiLineText:
                        value = field.Values;
                        break;
                    case FieldType.MultiMediaLink:
                        value = field.LinkedComponentValues;
                        break;
                    case FieldType.Number:
                        value = field.NumericValues;
                        break;
                    case FieldType.Text:
                        value = field.Values;
                        break;
                    case FieldType.Xhtml:
                        value = field.Values;
                        break;
                    default:
                        break;
                }
                return value;
            }
        }

        public DVM.ISchema EmbeddedSchema
        {
            get { return embeddedSchema; }
        }

        public string FieldTypeString
        {
            get { return fieldType; }
        }

        public string XPath
        {
            get { return xpath; }
        }

        public string Name
        {
            get { return name; }
        }

        public string RootElementName
        {
            get { return this.embeddedSchema.RootElementName; }
        }

        //How the hell to implement this for DD4T?
        public DVM.IXPathableNode Node
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    public class FieldSet : DVM.IFieldSet
    {
        private IDictionary<string, DVM.IField> fields;
        private DVM.IField CreateField(IField field)
        {
            return new Field(field);
        }
        public FieldSet(IFieldSet fieldset)
        {
            if (fieldset == null)
                fields = new Dictionary<string, DVM.IField>();
            else fields = fieldset.ToDictionary(x => x.Key, y => CreateField(y.Value));
        }
        public DVM.IField this[string key]
        {
            get { return fields[key]; }
        }

        public bool ContainsKey(string key)
        {
            return fields.ContainsKey(key);
        }
    }
    public class Component : TridionItemBase, DVM.IComponent
    {
        private DVM.IFieldSet fields;
        private DVM.IFieldSet metadataFields;
        protected IComponent dd4t;
        private DVM.ISchema schema;
        protected override IItem Item
        {
            get { return dd4t; }
        }
        public Component(IComponent component)
        {
            this.dd4t = component;
            this.schema = new Schema(dd4t.Schema);
            this.fields = new FieldSet(dd4t.Fields);
            this.metadataFields = new FieldSet(dd4t.MetadataFields);
        }

        public DVM.IFieldSet Fields
        {
            get { return fields; }
        }

        public DVM.IFieldSet MetadataFields
        {
            get
            {
                return metadataFields;   
            }
        }

        public DVM.ISchema Schema
        {
            get
            {
                return schema;
            }
        }
    }

    public class ComponentTemplate : TridionItemBase, DVM.IComponentTemplate
    {
        private readonly IComponentTemplate template;
        private readonly DVM.IFieldSet metadataFields;
        protected override IItem Item
        {
            get { return template; }
        }
        public ComponentTemplate(IComponentTemplate template)
        {
            this.template = template;
            this.metadataFields = new FieldSet(template.MetadataFields);
        }
        public DVM.IFieldSet MetadataFields
        {
            get
            {
                return metadataFields;
            }
        }


        public DateTime RevisionDate
        {
            get { return template.RevisionDate; }
        }
    }

    public class ComponentPresentation : DVM.IComponentPresentation
    {
        private DVM.IComponent component;
        private DVM.IComponentTemplate template;

        public ComponentPresentation(IComponentPresentation cp)
        {
            component = new Component(cp.Component);
            template = new ComponentTemplate(cp.ComponentTemplate);
        }
        public ComponentPresentation(DVM.IComponent component, DVM.IComponentTemplate template)
        {
            this.component = component;
            this.template = template;
        }
        public DVM.IComponent Component
        {
            get { return component; }
        }

        public DVM.IComponentTemplate ComponentTemplate
        {
            get { return template; }
        }
    }
}
