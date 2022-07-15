using System.ComponentModel;
using System.Runtime.CompilerServices;
#pragma warning disable CS0660
#pragma warning disable CS0661
namespace IndexerWpf.Classes
{
    public class Proper : INotifyPropertyChanged
    {
        public Proper() { }
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        protected bool SetProperty([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
