using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using System.Windows.Threading;
using WPFViewer.Commands;

namespace WPFViewer.ViewModels.UserControls
{
    /// <summary>
    /// Логика взаимодействия для PDFDocument.xaml
    /// </summary>
    public partial class PDFDocument : Grid
    {
        #region Properties
        public static readonly DependencyProperty PagesProperty =
            DependencyProperty.Register("Pages", typeof(ObservableCollection<PDFDocumentDataProcessorPage>), typeof(PDFDocument));
        public static readonly DependencyProperty AltMouseWheelHandlerProperty =
            DependencyProperty.Register("AltMouseWheelHandler", typeof(AltPlusMouseWheelHandler), typeof(PDFDocument));
        public static readonly DependencyProperty KeyboardHandlerProperty =
            DependencyProperty.Register("KeyboardHandler", typeof(KeyboardEventHandler), typeof(PDFDocument));
        #endregion
        #region Public fields
        public ObservableCollection<PDFDocumentDataProcessorPage> Pages
        {
            get { return (ObservableCollection<PDFDocumentDataProcessorPage>)GetValue(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public delegate bool AltPlusMouseWheelHandler(int delta);
        public AltPlusMouseWheelHandler AltMouseWheelHandler
        {
            get { return (AltPlusMouseWheelHandler)GetValue(AltMouseWheelHandlerProperty); }
            set { SetValue(AltMouseWheelHandlerProperty, value); }
        }

        public delegate bool KeyboardEventHandler();
        public KeyboardEventHandler KeyboardHandler
        {
            get { return (KeyboardEventHandler)GetValue(KeyboardHandlerProperty); }
            set { SetValue(KeyboardHandlerProperty, value); }
        }
        #endregion
        #region Fields
        private ObservableCollection<PDFPage> _pdfpages;
        Dispatcher _PDFDocumentDispatchObject = null;
        object _PDFDocumentDispatcherLocker = new object();
        #endregion
        #region Constructor
        public PDFDocument()
        {
            InitializeComponent();

            _pdfpages = new ObservableCollection<PDFPage>();

            this.Loaded += PDFDocumentView_Loaded;
        }
        #endregion
        #region Methods
        public void PDFDocumentDispatch(Action action)
        {
            lock (_PDFDocumentDispatcherLocker)
            {
                if (_PDFDocumentDispatchObject != null)
                {
                    _PDFDocumentDispatchObject.Invoke(action);
                }
            }
        }
        public static Size MeasureTextSize(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            FormattedText ft = new FormattedText(text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                fontSize,
                Brushes.Black);

            return new Size(ft.Width, ft.Height);
        }

        private void ChangeSize(int delta)
        {
            //double coeff = 1.1;
            //if (delta < 0)
            //    coeff = 0.9;
            //PDFDocumentDispatch(() =>
            //{
            //    if(ColumnDefinitions.Count > 0 && Pages.Count > 0)
            //    {
            //        for(int i = 0; i < PageSizes.Count; ++i)
            //        {
            //            PageSizes[i] = new Size(PageSizes[0].Width * coeff, PageSizes[0].Height * coeff);
            //        }
            //        ColumnDefinitions[0].Width = new GridLength(PageSizes[0].Width);
            //    }
            //});
            //for(int i = 0; i < Pages.Count; ++i)
            //{
            //    for(int j = 0; j < Pages[i].Count; ++j)
            //    {
            //        PDFDocumentDispatch(() =>
            //        {
            //            Pages[i][j].Margin = new Thickness(Pages[i][j].Margin.Left * coeff, Pages[i][j].Margin.Top * coeff, 0, 0);
            //            Pages[i][j].FontSize *= coeff;

            //            //if(PageSizes.Count > j && RowDefinitions.Count > j)
            //            RowDefinitions[j].Height = new GridLength(RowDefinitions[j].Height.Value * coeff);

            //            for (int jx = 0; jx < Pages[i].Count; ++jx)
            //                Pages[i][jx].FontSize *= coeff;
            //        });
            //    }
            //}
        }
        #endregion
        #region ChangedEvents
        private void PDFDocumentView_Loaded(object sender, RoutedEventArgs e)
        {
            if (GetValue(PagesProperty) != null)
            {
                Pages.CollectionChanged += Pages_CollectionChanged;
                for (int i = 0; i < Pages.Count; ++i)
                {
                    RowDefinition rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(Pages[i].Height);
                    RowDefinitions.Add(rowDef);

                    _pdfpages.Add(new PDFPage(Pages[i]));

                    Grid.SetColumn(_pdfpages[i], 0);
                    Grid.SetRow(_pdfpages[i], i);

                    Children.Add(_pdfpages[i]);

                    Pages[i].PropertyChanged += PDFDocumentDataProcessorPage_PropertyChanged;
                }
                if (Pages.Count > 0)
                {
                    ColumnDefinitions[0].Width = new GridLength(Pages.Max(ps => ps.Width));
                }
            }
        }

        private void PDFDocumentDataProcessorPage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(sender is PDFDocumentDataProcessorPage page)
            {
                if (Pages.Contains(page))
                {
                    int pageIndex = Pages.IndexOf(page);

                    _pdfpages[pageIndex].Width = page.Width;
                    _pdfpages[pageIndex].Height = page.Height;

                    //RowDefinitions[pageIndex].Height = new GridLength(page.Height);

                    ColumnDefinitions[0].Width = new GridLength(Pages.Select(p => p.Width).Max());

                    RowDefinitions[pageIndex].MinHeight = page.Height + page.PageSpace;
                }
            }
        }

        private void PageSizes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Pages != null && Pages.Count > 0)
            {
                for (int i = 0; i < RowDefinitions.Count && i < Pages.Count; ++i)
                {
                    RowDefinitions[i].Height = new GridLength(Pages[i].Height);
                }

                ColumnDefinitions[0].Width = new GridLength(Pages.Max(ps => ps.Width));
            }
        }

        private void Pages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //if(e.OldItems != null)
            //{
            //    _pdfpages = new ObservableCollection<PDFPage>();
            //    while (Children.Count > 0)
            //        Children.RemoveAt(0);
            //    while (RowDefinitions.Count > 0)
            //        RowDefinitions.RemoveAt(0);
            //}
            if (e.OldItems != null)
            {
                for (int i = 0; i < e.OldItems.Count; ++i)
                {
                    PDFDocumentDataProcessorPage page = (PDFDocumentDataProcessorPage)e.OldItems[i];

                    PDFPage pdfpage = new PDFPage(page);

                    int index = -1;
                    for (int j = 0; j < _pdfpages.Count; ++j)
                        if (_pdfpages[j] == pdfpage)
                        {
                            index = j;
                            break;
                        }
                    if (index != -1)
                    {
                        Children.Remove(_pdfpages[index]);

                        _pdfpages.RemoveAt(index);
                        RowDefinitions.RemoveAt(index);
                    }

                    page.PropertyChanged -= PDFDocumentDataProcessorPage_PropertyChanged;
                }

                //for (int i = 0; i < _pdfpages.Count; ++i)
                //{
                //    Grid.SetRow(_pdfpages[i], i);
                //}
            }
            //while (RowDefinitions.Count < Pages.Count)
            //{
            //    RowDefinition rowDef = new RowDefinition();
            //    RowDefinitions.Add(rowDef);
            //}
            //while (RowDefinitions.Count > Pages.Count)
            //{
            //    RowDefinitions.RemoveAt(RowDefinitions.Count - 1);
            //}
            if (e.NewItems != null)
            {
                for (int i = 0; i < e.NewItems.Count; ++i)
                {
                    PDFDocumentDataProcessorPage page = (PDFDocumentDataProcessorPage)e.NewItems[i];
                    PDFPage pdfpage = new PDFPage(page);

                    page.PropertyChanged += PDFDocumentDataProcessorPage_PropertyChanged;

                    Grid.SetColumn(pdfpage, 0);
                    Grid.SetRow(pdfpage, page.PageIndex);

                    RowDefinitions.Insert(page.PageIndex, new RowDefinition { Height = new GridLength(page.Height) });
                    //Children.Remove(_pdfpages[page.PageIndex]);
                    Children.Add(pdfpage);

                    _pdfpages.Insert(page.PageIndex, pdfpage);
                    //_pdfpages.Insert(page.PageIndex, pdfpage);

                    page.PropertyChanged += PDFDocumentDataProcessorPage_PropertyChanged;
                }
            }

            if (Pages != null && Pages.Count > 0)
            {
                for (int i = 0; i < RowDefinitions.Count && i < Pages.Count; ++i)
                {
                    RowDefinitions[i].Height = new GridLength(Pages[i].Height);
                }

                ColumnDefinitions[0].Width = new GridLength(Pages.Max(ps => ps.Width));
            }
        }
        #endregion
        #region Events
        private static int movingCount = 3;
        private void PDFDocument_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                if(AltMouseWheelHandler != null)
                {
                    try
                    {
                        if(AltMouseWheelHandler.Invoke(e.Delta))
                            e.Handled = true;
                    }
                    catch { }
                }
            }
        }

        private void PDFDocument_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            KeyboardHandler.Invoke();
        }
        #endregion
    }
}
