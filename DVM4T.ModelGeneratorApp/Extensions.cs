using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVM4T.ModelGeneratorApp
{
    public static class Extensions
    {
        public static string ResolveModelName(this string schemaName)
        {
            return schemaName.Replace(" ", "").Replace("Schema", "Model");
        }

        public static string ResolvePropertyName(this string fieldName)
        {
            if (!String.IsNullOrEmpty(fieldName))
                return fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1); //capitalize the first letter
            else return null;
        }
    }
}
