using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DM = DVM4T.Contracts;
using DD4T.ContentModel;
using System.Collections;
namespace DVM4T.DD4T
{
    public abstract class TridionItemBase : DM.ITridionItem
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

    public class Schema : TridionItemBase, DM.ISchema
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
    public class Field : DM.IField
    {
        private IField field;
        private DM.ISchema embeddedSchema;
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

        public DM.ISchema EmbeddedSchema
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
        public DM.IXPathableNode Node
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

    public class FieldSet : DM.IFieldSet
    {
        private IDictionary<string, DM.IField> fields;
        private DM.IField CreateField(IField field)
        {
            return new Field(field);
        }
        public FieldSet(IFieldSet fieldset)
        {
            if (fieldset == null)
                fields = new Dictionary<string, DM.IField>();
            else fields = fieldset.ToDictionary(x => x.Key, y => CreateField(y.Value));
        }
        public DM.IField this[string key]
        {
            get { return fields[key]; }
        }

        public bool ContainsKey(string key)
        {
            return fields.ContainsKey(key);
        }
    }
    public class Component : TridionItemBase, DM.IComponent
    {
        private DM.IFieldSet fields;
        private DM.IFieldSet metadataFields;
        protected IComponent dd4t;
        private DM.ISchema schema;
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

        public DM.IFieldSet Fields
        {
            get { return fields; }
        }

        public DM.IFieldSet MetadataFields
        {
            get
            {
                return metadataFields;   
            }
        }

        public DM.ISchema Schema
        {
            get
            {
                return schema;
            }
        }
    }

    public class ComponentTemplate : TridionItemBase, DM.IComponentTemplate
    {
        private readonly IComponentTemplate template;
        private readonly DM.IFieldSet metadataFields;
        protected override IItem Item
        {
            get { return template; }
        }
        public ComponentTemplate(IComponentTemplate template)
        {
            this.template = template;
            this.metadataFields = new FieldSet(template.MetadataFields);
        }
        public DM.IFieldSet MetadataFields
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

    public class ComponentPresentation : DM.IComponentPresentation
    {
        private DM.IComponent component;
        private DM.IComponentTemplate template;

        public ComponentPresentation(IComponentPresentation cp)
        {
            component = new Component(cp.Component);
            template = new ComponentTemplate(cp.ComponentTemplate);
        }
        public ComponentPresentation(DM.IComponent component, DM.IComponentTemplate template)
        {
            this.component = component;
            this.template = template;
        }
        public DM.IComponent Component
        {
            get { return component; }
        }

        public DM.IComponentTemplate ComponentTemplate
        {
            get { return template; }
        }
    }
}
