using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Contracts;
using DD4T.ContentModel;
using System.Collections;

namespace DVM4T.DD4T
{
    //TODO: Consider creating Factory or Provider to create these, especially ComponentPresentation
    public abstract class TridionItemBase : ITridionItemData
    {
        protected abstract IItem Item { get; }
        private int? pubId = null;

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
                if (pubId == null)
                {
                    try
                    {
                        pubId =  new TcmUri(Item.Id).PublicationId;
                    }
                    catch (Exception)
                    {
                        pubId = 0;
                    }
                }
                return (int)pubId;
            }
        }

        public abstract object BaseData { get; }
    }

    public class Keyword : TridionItemBase, IKeywordData
    {
        private readonly IKeyword keyword;
        private readonly IFieldsData metadata;
        private readonly ISchemaData metadataSchema;
        private readonly ICategoryData category;

        public Keyword(IKeyword keyword, string categoryName = null)
        {
            this.keyword = keyword;
            this.metadata = new FieldSet(keyword.MetadataFields);
            //DD4T provides no way to retrieve Category
            this.category = new CategoryData(keyword.TaxonomyId, categoryName);
        }

        protected override IItem Item
        {
            get { return keyword; }
        }

        public override object BaseData
        {
            get { return keyword; }
        }

        public string Key
        {
            get { return keyword.Key; }
        }

        public IFieldsData Metadata
        {
            get { return metadata; }
        }

        public ISchemaData MetadataSchema
        {
            get { return metadataSchema; } //No DD4T Metadata Schema
        }

        public ICategoryData Category
        {
            get { return category; } //Only limited DD4T Category Data - TcmUri only!
        }
    }

    public class CategoryData : TridionItemBase, ICategoryData
    {
        private ICategory category;

        public CategoryData(string taxonomyId, string categoryTitle)
        {
            category = new Category
            {
                Id = taxonomyId,
                Title = categoryTitle
            };
        }
        public CategoryData(ICategory category)
        {
            this.category = category;
        }
        public string XmlName
        {
            get { return null; } //No XmlName available in DD4T
        }

        protected override IItem Item
        {
            get { return category; }
        }

        public override object BaseData
        {
            get { return category; }
        }
    }

    public class Schema : TridionItemBase, ISchemaData
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

        public override object BaseData
        {
            get { return schema; }
        }
    }
    public class Field : IFieldData
    {
        private IField field;
        private ISchemaData embeddedSchema;
        private IList<IKeywordData> keywords;
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
            this.keywords = field.Keywords != null ? field.Keywords.Select(x => new Keyword(x, field.CategoryName) as IKeywordData).ToList()
                : null;
        }
        public IEnumerable Values
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

        public ISchemaData EmbeddedSchema
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

        public object BaseData
        {
            get { return field; }
        }


        public IList<IKeywordData> Keywords
        {
            get { return keywords; }
        }
    }

    public class FieldSet : IFieldsData
    {
        private IDictionary<string, IFieldData> fields;
        private IFieldSet fieldset;
        private IFieldData CreateField(IField field)
        {
            return new Field(field);
        }
        public FieldSet(IFieldSet fieldset)
        {
            this.fieldset = fieldset;
            if (fieldset == null)
                fields = new Dictionary<string, IFieldData>();
            else fields = fieldset.ToDictionary(x => x.Key, y => CreateField(y.Value));
        }
        public IFieldData this[string key]
        {
            get { return fields[key]; }
        }

        public bool ContainsKey(string key)
        {
            return fields.ContainsKey(key);
        }

        public object BaseData
        {
            get { return fieldset; }
        }
    }

    public class ContentPresentationData : IContentPresentationData
    {
        private readonly IFieldsData content;
        private readonly ISchemaData schema;
        private readonly ITemplateData template;
        private readonly int publicationId;
        public ContentPresentationData(IFieldsData content, ISchemaData schema, ITemplateData template)
        {
            this.content = content;
            this.schema = schema;
            this.template = template;
            publicationId = template.PublicationId;
        }

        public IFieldsData Content
        {
            get { return content; }
        }

        public ISchemaData Schema
        {
            get { return schema; }
        }

        public ITemplateData Template
        {
            get { return template; }
        }

        public IFieldsData Metadata
        {
            get { return null; }
        }

        public int PublicationId
        {
            get { return publicationId; }
        }

        public object BaseData
        {
            get { return content; }
        }
    }

    public class MultimediaData : IMultimediaData
    {
        private IMultimedia multimedia;
        public MultimediaData(IMultimedia multimedia)
        {
            //This can and will often be null
            this.multimedia = multimedia;
        }
        public string Url
        {
            get { return multimedia.Url; }
        }

        public string FileName
        {
            get { return multimedia.FileName; }
        }

        public string MimeType
        {
            get { return multimedia.MimeType; }
        }

        public IMultimedia DD4TMultimedia { get { return multimedia; } }

        public object BaseData
        {
            get { return multimedia; }
        }
    }
    
    public class Component : TridionItemBase, IComponentData
    {
        private IFieldsData fields;
        private IFieldsData metadataFields;
        protected IComponent dd4t;
        private ISchemaData schema;
        private IMultimediaData multimedia;
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
            this.multimedia = new MultimediaData(dd4t.Multimedia);
        }

        public IFieldsData Content
        {
            get { return fields; }
        }

        public IFieldsData Metadata
        {
            get
            {
                return metadataFields;   
            }
        }

        public ISchemaData Schema
        {
            get
            {
                return schema;
            }
        }

        public IMultimediaData MultimediaData
        {
            get { return multimedia; }
        }

        public override object BaseData
        {
            get { return dd4t; }
        }
    }

    public class ComponentTemplate : TridionItemBase, ITemplateData
    {
        private readonly IComponentTemplate template;
        private readonly IFieldsData metadataFields;
        protected override IItem Item
        {
            get { return template; }
        }
        public ComponentTemplate(IComponentTemplate template)
        {
            this.template = template;
            this.metadataFields = new FieldSet(template.MetadataFields);
        }
        public IFieldsData Metadata
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

        public override object BaseData
        {
            get { return template; }
        }
    }

    public class ComponentPresentation : IComponentPresentationData
    {
        private readonly IComponentData component;
        private readonly ITemplateData template;
        private readonly IComponentPresentation cp;

        public ComponentPresentation(IComponentPresentation cp)
        {
            this.cp = cp;
            component = new Component(cp.Component);
            template = new ComponentTemplate(cp.ComponentTemplate);
        }
        public ComponentPresentation(IComponent component, IComponentTemplate template)
        {
            this.cp = Helper.BuildComponentPresentation(component, template); //Helps get around naming conflicts
            this.component = new Component(component);
            this.template = new ComponentTemplate(template);
        }
        public ComponentPresentation(IComponentData component, ITemplateData template)
        {
            this.component = component;
            this.template = template;
            this.cp = Helper.BuildComponentPresentation(component.BaseData as IComponent, template.BaseData as IComponentTemplate);
        }
        
        public IComponentData Component
        {
            get { return component; }
        }
        [Obsolete("Use Template instead")]
        public ITemplateData ComponentTemplate
        {
            get { return template; }
        }

        public object BaseData
        {
            get { return cp; }
        }

        //public IMultimediaData MultimediaData
        //{
        //    get { return component == null ? null : component.MultimediaData; }
        //}

        //public string TcmUri
        //{
        //    get { return component == null ? null : component.TcmUri; }
        //}

        //public string Title
        //{
        //    get { return component == null ? null : component.Title; }
        //}

        public IFieldsData Content
        {
            get { return component == null ? null : component.Content; }
        }

        public ISchemaData Schema
        {
            get { return component == null ? null : component.Schema; }
        }

        public ITemplateData Template
        {
            get { return template; }
        }

        public IFieldsData Metadata
        {
            get { return component == null ? null : component.Metadata; }
        }

        public int PublicationId
        {
            get { return component == null ? 0 : component.PublicationId; }
        }
    }
    public class PageTemplate : TridionItemBase, ITemplateData
    {
        private readonly IPageTemplate template;
        private readonly IFieldsData metadataFields;
        protected override IItem Item
        {
            get { return template; }
        }
        public PageTemplate(IPageTemplate template)
        {
            this.template = template;
            this.metadataFields = new FieldSet(template.MetadataFields);
        }
        public IFieldsData Metadata
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

        public override object BaseData
        {
            get { return template; }
        }
    }
    public class Page : TridionItemBase, IPageData
    {
        IPage page = null;

        protected override IItem Item
        {
            get { return page; }
        }

        public override object BaseData
        {
            get { return Item; }
        }

        public Page(IPage page)
        {
            this.page = page;
            ComponentPresentations = page.ComponentPresentations.Select(x => new ComponentPresentation(x) as IComponentPresentationData).ToList();
            Metadata = new FieldSet(page.MetadataFields);
            FileName = page.Filename;
            Template = new PageTemplate(page.PageTemplate);
            PageTemplate = Template;
        }

        public IList<IComponentPresentationData> ComponentPresentations
        {
            get;
            private set;
        }
        [Obsolete("Use Template instead")]
        public ITemplateData PageTemplate
        {
            get;
            private set;
        }

        public ITemplateData Template
        {
            get;
            private set;
        }

        public IFieldsData Metadata
        {
            get;
            private set;
        }

        public string FileName
        {
            get;
            private set;
        }
    }

}
