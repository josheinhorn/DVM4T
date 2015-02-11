using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Contracts;

namespace DVM4T.Base
{
    /// <summary>
    /// Base class for all Component Presentation View Models
    /// </summary>
    public abstract class ComponentPresentationViewModelBase : IComponentPresentationViewModel
    {
        public IComponentPresentationData ComponentPresentation
        {
            get;
            set;
        }
        public IViewModelData ModelData
        {
            get;
            set;
        }
    }
    /// <summary>
    /// Base class for all Embedded Schema View Models
    /// </summary>
    public abstract class EmbeddedSchemaViewModelBase : IEmbeddedSchemaViewModel
    {
        public IViewModelData ModelData
        {
            get;
            set;
        }
    }
}
