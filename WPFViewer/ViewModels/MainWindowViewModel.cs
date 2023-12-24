using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WPFViewer.Commands;
using System.Windows;
using System.ComponentModel;

namespace WPFViewer.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public IReadOnlyList<string> Arguments { get; }
        public MainWindowViewModel()
        {
            _showHideOptionsCommand = new Command(ShowHideOptionsMethod);
        }
        public MainWindowViewModel(IReadOnlyList<string> arguments) : this()
        {
            Arguments = arguments;
        }

        private Visibility _isOptionsVisible = Visibility.Visible;

        public Visibility IsOptionsVisible
        {
            get { return _isOptionsVisible; }
            set
            {
                _isOptionsVisible = value;
                OnPropertyChanged("IsOptionsVisible");
            }
        }

        private ICommand _showHideOptionsCommand;

        public ICommand ShowHideOptionsCommand
        {
            get { return _showHideOptionsCommand; }
        }

        private void ShowHideOptionsMethod()
        {
            if (IsOptionsVisible == Visibility.Visible)
            {
                IsOptionsVisible = Visibility.Collapsed;
            }
            else
            {
                IsOptionsVisible = Visibility.Visible;
            }
        }
        #region Events
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
