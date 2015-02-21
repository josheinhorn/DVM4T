using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Contracts;

namespace DVM4T.Base
{

    /// <summary>
    /// Base class for all View Models
    /// </summary>
    public abstract class ViewModelBase : IViewModel
    {
        public IViewModelData ModelData
        {
            get;
            set;
        }
    }

    [Obsolete("Use ViewModelBase instead")]
    /// <summary>
    /// Base class for all Component Presentation View Models
    /// </summary>
    public abstract class ComponentPresentationViewModelBase : ViewModelBase
    {

    }

    [Obsolete("Use ViewModelBase instead")]
    /// <summary>
    /// Base class for all Embedded Schema View Models
    /// </summary>
    public abstract class EmbeddedSchemaViewModelBase : ViewModelBase
    {
    }


}
