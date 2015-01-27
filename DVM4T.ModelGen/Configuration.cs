using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVM4T.ModelGen
{
    public interface ICodeGenConfiguration
    {
        IDictionary<FieldType, IFieldAttributeDef> FieldAttributeTypes { get; }
        string[] Namespaces { get; }
        string EmbeddedSchemaTypeAttributeParameterName { get; }
        string LinkedComponentTypesAttributeParameterName { get; }
    }

    public interface IFieldAttributeDef
    {
        string Name { get; set; }
        string SingleExpectedReturnTypeName { get; set; }
        string MultiExpectedReturnTypeName { get; set; }
    }
}
