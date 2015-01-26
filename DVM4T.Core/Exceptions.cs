using DVM4T.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVM4T.Exceptions
{
    public class ViewModelTypeNotFoundExpception : Exception
    {
        public ViewModelTypeNotFoundExpception(string schemaName, string viewModelKey)
            : base(String.Format("Could not find view model for schema '{0}' and key '{1}' or default for schema '{0}' in loaded assemblies."
                    , schemaName, viewModelKey)) 
        { }
    }

    public class FieldTypeMismatchException : Exception
    {
        public FieldTypeMismatchException(FieldAttributeProperty fieldProperty, IFieldAttribute fieldAttribute, object fieldValue) : 
            base(String.Format("Type mismatch for property {0}. Expected type for {1} is {2}. Model Property is of type {3}. Field value is of type {4}."
            , fieldProperty.Name, fieldAttribute.GetType().Name, fieldAttribute.ExpectedReturnType.FullName, fieldProperty.PropertyType.FullName,
            fieldValue.GetType().FullName)) { }
    }
}
