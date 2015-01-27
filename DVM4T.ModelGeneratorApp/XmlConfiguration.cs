
using DVM4T.ModelGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DVM4T.ModelGeneratorApp
{
    public class XmlModelConfig : ICodeGenConfiguration
    {
        private IDictionary<FieldType, IFieldAttributeDef> fieldAttributeTypes = new Dictionary<FieldType, IFieldAttributeDef>();
        private string[] namespaces;
        private string embeddedSchemaTypeAttributeParameterName;
        private string linkedComponentTypesAttributeParameterName;
        public XmlModelConfig(string xmlFilePath)
        {
            var doc = XDocument.Load(xmlFilePath);
            var config = doc.Element("config");
            fieldAttributeTypes = config.Element("FieldAttributeTypes")
                .Elements("FieldAttribute")
                .ToDictionary(
                node => ParseFieldType(node.Attribute("fieldType").Value),
                node => new FieldAttributeConfig
                        {
                            Name = node.Attribute("name").Value,
                            MultiExpectedReturnTypeName = node.Attribute("multiType").Value,
                            SingleExpectedReturnTypeName = node.Attribute("singleType").Value
                        } as IFieldAttributeDef);
            namespaces = config.Element("Namespaces")
                .Elements("Namespace")
                .Select(node => node.Attribute("value").Value).ToArray();
            embeddedSchemaTypeAttributeParameterName = config.Element("EmbeddedSchemaTypeAttributeParameterName").Attribute("value").Value;
            linkedComponentTypesAttributeParameterName = config.Element("LinkedComponentTypesAttributeParameterName").Attribute("value").Value;
        }

        private FieldType ParseFieldType(string value)
        {
            FieldType result;
            Enum.TryParse<FieldType>(value, out result);
            return result;
        }


        public IDictionary<FieldType, IFieldAttributeDef> FieldAttributeTypes
        {
            get { return fieldAttributeTypes; }
        }

        public string[] Namespaces
        {
            get { return namespaces; }
        }

        public string EmbeddedSchemaTypeAttributeParameterName
        {
            get { return embeddedSchemaTypeAttributeParameterName; }
        }

        public string LinkedComponentTypesAttributeParameterName
        {
            get { return linkedComponentTypesAttributeParameterName; }
        }
    }

    public class FieldAttributeConfig : IFieldAttributeDef
    {

        public string Name
        {
            get;
            set;
        }

        public string SingleExpectedReturnTypeName
        {
            get;
            set;
        }

        public string MultiExpectedReturnTypeName
        {
            get;
            set;
        }
    }
}
