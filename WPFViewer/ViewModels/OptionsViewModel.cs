using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WPFViewer.Commands;

namespace WPFViewer.ViewModels
{
    public class OptionsViewModel : INotifyPropertyChanged
    {
        #region Private fields
        private Command _hideOptionsCommand;
        private Command _openFileCommand;
        private Command _textSizeMinusCommand;
        private Command _textSizePlusCommand;
        private Command _pageSizeMinusCommand;
        private Command _pageSizePlusCommand;
        private Command _textAlignmentCommand;
        private Command _translateCurrentPhraseCommand;
        private Command _scrollToPageCommand;
        private Command _translationsFontSizePlusCommand;
        private Command _translationsFontSizeMinusCommand;

        private Visibility _isOptionsVisible;

        private string _readingFilePath;

        #endregion
        #region Public fields
        public Visibility IsOptionsVisible
        {
            get { return _isOptionsVisible; }
            set
            {
                _isOptionsVisible = value;
                OnPropertyChanged("IsOptionsVisible");
            }
        }

        public string ReadingFilePath
        {
            get { return _readingFilePath; }
            set
            {
                _readingFilePath = value;
            }
        }
        
        #endregion
        #region Public commands
        public ICommand HideOptionsCommand
        {
            get { return _hideOptionsCommand; }
        }

        public ICommand OpenFileCommand
        {
            get { return _openFileCommand; }
        }

        public ICommand TextSizeMinusCommand
        {
            get { return _textSizeMinusCommand; }
        }

        public ICommand TextSizePlusCommand
        {
            get { return _textSizePlusCommand; }
        }

        public ICommand PageSizeMinusCommand
        {
            get { return _pageSizeMinusCommand; }
        }

        public ICommand PageSizePlusCommand
        {
            get { return _pageSizePlusCommand; }
        }

        public ICommand TextAlignmentCommand
        {
            get { return _textAlignmentCommand; }
        }

        public ICommand TranslateCurrentPhraseCommand
        {
            get { return _translateCurrentPhraseCommand; }
        }

        public ICommand ScrollToPageCommand
        {
            get { return _scrollToPageCommand; }
        }

        public ICommand TranslationsFontSizePlusCommand
        {
            get { return _translationsFontSizePlusCommand; }
        }

        public ICommand TranslationsFontSizeMinusCommand
        {
            get { return _translationsFontSizeMinusCommand; }
        }
        #endregion
        #region Command methods
        private void HideOptionsMethod()
        {
            IsOptionsVisible = Visibility.Collapsed;
        }

        private void OpenFileMethod()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                ReadingFilePath = fileDialog.FileName;
                ExternalInterface.Instance.OpenFile(ReadingFilePath);
            }
        }

        private void TextSizeMinusMethod()
        {
            ExternalInterface.Instance.DocumentData.TextSize -= 10;
        }

        private void TextSizePlusMethod()
        {
            ExternalInterface.Instance.DocumentData.TextSize += 10;
        }

        private void PageSizeMinusMethod()
        {
            ExternalInterface.Instance.DocumentData.PageSize -= 10;
        }

        private void PageSizePlusMethod()
        {
            ExternalInterface.Instance.DocumentData.PageSize += 10;
        }

        private void TextAlignmentMethod()
        {
            
        }

        private void TranslateCurrentPhraseMethod()
        {
            ExternalInterface.Instance.DocumentData.GetTranslate();
        }

        private void ScrollToPageMethod()
        {

        }

        private void TranslationsFontSizePlusMethod()
        {
            ExternalInterface.Instance.DocumentData.TranslationsFontSize += 1;
        }

        private void TranslationsFontSizeMinusMethod()
        {
            ExternalInterface.Instance.DocumentData.TranslationsFontSize -= 1;
        }
        #endregion

        #region Constructor
        public OptionsViewModel()
        {
            _isOptionsVisible = Visibility.Visible;
            _hideOptionsCommand = new Command(HideOptionsMethod);
            _openFileCommand = new Command(OpenFileMethod);
            _textSizeMinusCommand = new Command(TextSizeMinusMethod);
            _textSizePlusCommand = new Command(TextSizePlusMethod);
            _pageSizeMinusCommand = new Command(PageSizeMinusMethod);
            _pageSizePlusCommand = new Command(PageSizePlusMethod);
            _textAlignmentCommand = new Command(TextAlignmentMethod);
            _translateCurrentPhraseCommand = new Command(TranslateCurrentPhraseMethod);
            _scrollToPageCommand = new Command(ScrollToPageMethod);
            _translationsFontSizePlusCommand = new Command(TranslationsFontSizePlusMethod);
            _translationsFontSizeMinusCommand = new Command(TranslationsFontSizeMinusMethod);
        }
        #endregion

        #region Methods
        public void CurrentTranslatingWordTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ExternalInterface.Instance.DocumentData.MoveOpenedToolTip(e.Delta))
                e.Handled = true;
        }

        public void TranslatePhraseButton_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ExternalInterface.Instance.DocumentData.MoveOpenedToolTip(e.Delta))
                e.Handled = true;
        }
        #endregion

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
