using DVM4T.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVM4T.Core
{
    public class ViewModelList<T> : List<IViewModel>, IEnumerable<T> where T : IViewModel
    {
        public new IEnumerator<T> GetEnumerator()
        {
            return this.ToArray().Cast<T>().GetEnumerator(); //Assuming all the objects added to this implement T
        }
    }
}
