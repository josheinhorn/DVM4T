
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
        private IDictionary<ModelType, IAttributeDef[]> modelAttributes = null;
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
                node => SafeParseEnum<FieldType>(node.Attribute("fieldType").Value),
                node => new FieldAttributeConfig
                        {
                            Name = node.Attribute("name").Value,
                            MultiExpectedReturnTypeName = node.Attribute("multiType").Value,
                            ReturnTypeName = node.Attribute("singleType").Value
                        } as IFieldAttributeDef);
            if (config.Elements("ModelAttributeTypes") != null)
            {
                modelAttributes = config.Elements("ModelAttributeTypes")
                    .Elements("ModelAttributes")
                    .ToDictionary(m => SafeParseEnum<ModelType>(m.Attribute("modelType").Value),
                    m => m.Elements("Attribute").Select(a => new AttributeConfig
                    {
                        Name = a.Attribute("name").Value,
                        ReturnTypeName = a.Attribute("returnType").Value,
                        DefaultPropertyName = a.Attribute("propertyName").Value
                    } as IAttributeDef).ToArray());
            }
            namespaces = config.Element("Namespaces")
                .Elements("Namespace")
                .Select(node => node.Attribute("value").Value).ToArray();
            embeddedSchemaTypeAttributeParameterName = config.Element("EmbeddedSchemaTypeAttributeParameterName").Attribute("value").Value;
            linkedComponentTypesAttributeParameterName = config.Element("LinkedComponentTypesAttributeParameterName").Attribute("value").Value;
        }

        private T SafeParseEnum<T>(string value) where T : struct
        {
            T result;
            Enum.TryParse<T>(value, out result);
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

        public IDictionary<ModelType, IAttributeDef[]> ModelAttributeTypes
        {
            get { return modelAttributes; }
        }
    }

    public class FieldAttributeConfig : IFieldAttributeDef
    {

        public string Name
        {
            get;
            set;
        }

        public string ReturnTypeName
        {
            get;
            set;
        }

        public string MultiExpectedReturnTypeName
        {
            get;
            set;
        }


        public string DefaultPropertyName
        {
            get;
            set;
        }
    }

    public class AttributeConfig : IAttributeDef
    {
        public string Name
        {
            get;
            set;
        }

        public string ReturnTypeName
        {
            get;
            set;
        }


        public string DefaultPropertyName
        {
            get;
            set;
        }
    }
}
