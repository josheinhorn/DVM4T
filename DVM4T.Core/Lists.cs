using DVM4T.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVM4T.Core
{
    /// <summary>
    /// An adapter to allow for enumeration of more strongly typed views
    /// </summary>
    /// <typeparam name="T">Type of view model or common base class/interface of view model types to use</typeparam>
    [Obsolete("Use ObjectListAdapter<T> instead")]
    public class ViewModelList<T> : List<IViewModel>, IEnumerable<T> where T : IViewModel
    {
        public new IEnumerator<T> GetEnumerator()
        {
            //If ToArray() is not used, a Stack Overflow occurs because it is recursively calling this very method!
            return this.ToArray().Cast<T>().GetEnumerator(); //Assuming all the objects added to this implement T
        }
    }

    /// <summary>
    /// An adapter to allow for enumeration of strongly typed objects while implementing IList&lt;object&gt;
    /// </summary>
    /// <typeparam name="T">Type of model or common base class/interface of types to use</typeparam>
    public class ObjectListAdapter<T> : List<object>, IEnumerable<T>
    {
        public new IEnumerator<T> GetEnumerator()
        {
            //If ToArray() is not used, a Stack Overflow occurs because it is recursively calling this very method!
            return this.ToArray().Cast<T>().GetEnumerator(); //Assuming all the objects added to this implement T - definitely NOT guaranteed!
        }
    }
}
