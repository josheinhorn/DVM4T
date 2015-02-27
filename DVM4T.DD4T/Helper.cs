using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic = DD4T.ContentModel;
namespace DVM4T.DD4T
{
    internal static class Helper
    {
        internal static Dynamic.ComponentPresentation BuildComponentPresentation(
            Dynamic.IComponent component, Dynamic.IComponentTemplate template)
        {
            return new Dynamic.ComponentPresentation
                {
                    ComponentTemplate = template as Dynamic.ComponentTemplate,
                    Component = component as Dynamic.Component
                };
        }
    }
}
