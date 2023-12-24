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
using System.Windows.Shapes;

namespace WPFViewer.Views
{
    /// <summary>
    /// Логика взаимодействия для TranslationWindow.xaml
    /// </summary>
    public partial class TranslationWindowView : Window
    {
        public TranslationWindowView()
        {
            InitializeComponent();
        }

        private void TranslationWindowView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ((ViewModels.TranslationWindowViewModel)DataContext).TranslationWindowView_MouseWheel(sender, e);
        }
    }
}
