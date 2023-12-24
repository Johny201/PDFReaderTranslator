using System;
using WPFViewer.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WPFViewer.ViewModels
{
    class TranslationWindowViewModel
    {
        public TranslationWindowViewModel()
        {
            _translateCurrentPhraseCommand = new Command(TranslateCurrentPhraseMethod);
        }
        private Command _translateCurrentPhraseCommand;
        public ICommand TranslateCurrentPhraseCommand
        {
            get { return _translateCurrentPhraseCommand; }
        }

        private void TranslateCurrentPhraseMethod()
        {
            ExternalInterface.Instance.DocumentData.GetTranslate();
        }

        public void TranslationWindowView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ExternalInterface.Instance.DocumentData.MoveOpenedToolTip(e.Delta))
                e.Handled = true;
        }
    }
}
