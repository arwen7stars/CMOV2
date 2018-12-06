using System;
using System.Collections.Generic;
using System.Text;

namespace Stocks
{
    public class SelectableData<T>
    {
        public T Data { get; set; }
        public bool Selected { get; set; }

        public SelectableData(T data) {
            this.Data = data;
            this.Selected = false;
        }
    }
}
