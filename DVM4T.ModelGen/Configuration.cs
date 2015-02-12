using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVM4T.ModelGen
{
    public interface ICodeGenConfiguration
    {
        IDictionary<FieldType, IFieldAttributeDef> FieldAttributeTypes { get; }
        IDictionary<ModelType, IAttributeDef[]> ModelAttributeTypes { get; }
        string[] Namespaces { get; }
        string EmbeddedSchemaTypeAttributeParameterName { get; }
        string LinkedComponentTypesAttributeParameterName { get; }
    }
    public interface IAttributeDef 
    {
        string Name { get; set; }
        string ReturnTypeName { get; set; }
        string DefaultPropertyName { get; set; }
    }
    public interface IFieldAttributeDef : IAttributeDef
    {
        string MultiExpectedReturnTypeName { get; set; }
    }
}
