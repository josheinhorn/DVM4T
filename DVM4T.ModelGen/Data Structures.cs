using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVM4T.ModelGen
{
    public class ViewModel
    {
        public ViewModel()
        {
            FieldProperties = new List<FieldProperty>();
        }
        public string Name { get; set; }
        public string SchemaName { get; set; }
        public IList<FieldProperty> FieldProperties { get; set; }
        public IList<PropertyAttribute> OtherProperties { get; set; }
        public Type BaseClass { get; set; }
        public string ContainingFolder { get; set; }
        public ModelType ModelType { get; set; }
    }
    public enum ModelType
    {
        ComponentPresentation, MultimediaComponent, EmbeddedSchemaFields
    }
    public class PropertyAttribute
    {
        public string Name { get; set; }
        public string PropertyName { get; set; }
    }
    public class FieldProperty
    {
        public FieldProperty()
        {
            LinkedComponentTypeNames = new List<string>();
        }
        public string PropertyName { get; set; }
        public string FieldName { get; set; }
        public FieldType FieldType { get; set; }
        public bool IsMultiValue { get; set; }
        public bool IsMetadata { get; set; }
        public IList<string> LinkedComponentTypeNames { get; set; }
        public string EmbeddedTypeName { get; set; }
    }

    public class Schema
    {
        public string Name { get; set; }
    }
    public enum FieldType
    {
        Text, MultilineText, RichText, Number, Date, Embedded, Linked, Multimedia, Keyword, ExternalLink
    }
}
