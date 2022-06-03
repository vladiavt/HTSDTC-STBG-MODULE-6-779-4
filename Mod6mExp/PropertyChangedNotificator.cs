using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Machine
{
    /// <summary>
    /// Use to include the PropertyChangedEventHandler, and OnPropertyChanged event within a class.
    /// </summary>
    public abstract class PropertyChangedNotificator : INotifyPropertyChanged
    {
        public void OnPropertyChanged([CallerMemberName]string callerName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(callerName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
