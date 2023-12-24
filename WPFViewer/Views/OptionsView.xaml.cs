using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFViewer.Views
{
    /// <summary>
    /// Логика взаимодействия для OptionsPage.xaml
    /// </summary>
    public partial class OptionsView : UserControl
    {
        public OptionsView()
        {
            InitializeComponent();
        }

        private void CurrentTranslatingWordTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ((ViewModels.OptionsViewModel)DataContext).CurrentTranslatingWordTextBox_MouseWheel(sender, e);
        }

        private void TranslatePhraseButton_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ((ViewModels.OptionsViewModel)DataContext).TranslatePhraseButton_MouseWheel(sender, e);
        }
    }
}
