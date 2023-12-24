using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Collections.ObjectModel;
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

namespace WPFViewer.ViewModels.UserControls
{
    /// <summary>
    /// Логика взаимодействия для DocumentText.xaml
    /// </summary>
    public partial class PDFPage : Canvas
    {
        #region Properties
        public static readonly DependencyProperty PDFDocumentDataPageProperty =
            DependencyProperty.Register("PDFDocumentDataPage", typeof(PDFDocumentDataProcessorPage), typeof(PDFPage));
        #endregion

        #region Public fields
        public PDFDocumentDataProcessorPage PDFDocumentDataPage
        {
            get { return (PDFDocumentDataProcessorPage)GetValue(PDFDocumentDataPageProperty); }
            set { SetValue(PDFDocumentDataPageProperty, value); }
        }
        #endregion
        #region Constructor
        public PDFPage()
        {
            InitializeComponent();

            this.Loaded += DocumentData_Loaded;
        }

        public PDFPage(PDFDocumentDataProcessorPage page)
        {
            InitializeComponent();

            PDFDocumentDataPage = page;

            this.Loaded += DocumentData_Loaded;
        }
        #endregion

        #region Methods
        private void LocateData()
        {
            LocateText();
            LocateImages();
        }

        private void LocateText()
        {
            if (PDFDocumentDataPage == null)
                return;
            foreach(ObservableCollection<ObservableCollection<TextBlock>> textBlock in PDFDocumentDataPage.DataBlocks)
            {
                foreach(ObservableCollection<TextBlock> textLine in textBlock)
                {
                    foreach(TextBlock tb in textLine)
                    {
                        Children.Add(tb);
                    }
                }
            }
        }

        private void LocateImages()
        {
            if (PDFDocumentDataPage == null)
                return;
            foreach (Image image in PDFDocumentDataPage.Images)
            {
                Children.Add(image);
            }
        }
        #endregion

        #region Operators
        public static bool operator ==(PDFPage page1, PDFPage page2)
        {
            try
            {
                if (page1.PDFDocumentDataPage.DataBlocks.Count != page2.PDFDocumentDataPage.DataBlocks.Count)
                    return false;

                for (int i = 0; i < page1.PDFDocumentDataPage.DataBlocks.Count; ++i)
                {
                    for (int ix = 0; ix < page1.PDFDocumentDataPage.DataBlocks[i].Count; ++ix)
                    {
                        if (ix >= page2.PDFDocumentDataPage.DataBlocks[i].Count)
                            return false;
                        for (int j = 0; j < page1.PDFDocumentDataPage.DataBlocks[i][ix].Count; ++j)
                        {
                            if (j >= page2.PDFDocumentDataPage.DataBlocks[i][ix].Count)
                                return false;
                            if (page1.PDFDocumentDataPage.DataBlocks[i][ix][j].Text != page2.PDFDocumentDataPage.DataBlocks[i][ix][j].Text)
                                return false;
                        }
                    }
                }
            }
            catch { }

            return true;
        }

        public static bool operator !=(PDFPage page1, PDFPage page2)
        {
            return !(page1 == page2);
        }
        #endregion

        #region ChangedEvents
        private void DocumentData_Loaded(object sender, RoutedEventArgs e)
        {
            LocateData();
        }

        private void TextBlocks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                for (int i = 0; i < e.NewItems.Count; ++i)
                {
                    Children.Add((TextBlock)e.NewItems[i]);
                }
            }
        }
        #endregion
    }
}
