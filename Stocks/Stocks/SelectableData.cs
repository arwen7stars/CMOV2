using System;
using System.Collections.Generic;
using System.Text;

namespace Stocks
{
    class SelectableData<T>
    {
        public T Data { get; set; }
        public bool Selected { get; set; }
    }
}
