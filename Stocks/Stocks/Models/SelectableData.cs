using System.ComponentModel;

namespace Stocks
{
    public class SelectableData<T> : INotifyPropertyChanged
    {
        public T Data { get; set; }

        private bool _Selected;
        public bool Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
                OnPropertyChanged(nameof(Selected));
            }
        }

        public SelectableData(T data)
        {
            Data = data;
            Selected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler == null)
                return;

            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}