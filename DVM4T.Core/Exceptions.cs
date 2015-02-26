using DVM4T.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVM4T.Exceptions
{
    public class ViewModelTypeNotFoundException : Exception
    {
        public ViewModelTypeNotFoundException(IContentViewModelData data)
            : base(String.Format("Could not find view model for schema '{0}' and Template '{1}' or default for schema '{0}' in loaded assemblies."
                    , data.Schema.Title, data.Template.Title)) 
        { }

        public ViewModelTypeNotFoundException(IViewModelData data)
            : base(String.Format("Could not find view model for item with Template '{0}' and Publication ID '{1}'", data.Template.Title, data.PublicationId))
        { }
    }

    public class PropertyTypeMismatchException : Exception
    {
        public PropertyTypeMismatchException(IModelProperty fieldProperty, IPropertyAttribute fieldAttribute, object fieldValue) : 
            base(String.Format("Type mismatch for property '{0}'. Expected type for '{1}' is {2}. Model Property is of type {3}. Field value is of type {4}."
            , fieldProperty.Name, fieldAttribute.GetType().Name, fieldAttribute.ExpectedReturnType.FullName, fieldProperty.PropertyType.FullName,
            fieldValue.GetType().FullName)) { }
    }
}
