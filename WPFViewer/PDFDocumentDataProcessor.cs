using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.IO;

namespace WPFViewer
{
    class PDFDocumentDataProcessor : INotifyPropertyChanged
    {
        #region Fields
        private Logger.Logger log = Logger.Logger.GetLogger("WPFViewer", "PDFDocumentDataProcess");
        public FontFamily PDFFontFamily { get; set; }
        public FontStyle PDFFontStyle { get; set; }
        public FontWeight PDFFontWeight { get; set; }
        public FontStretch PDFFontStretch { get; set; }
        public ObservableCollection<PDFDocumentDataProcessorPage> Pages { get; set; } = new ObservableCollection<PDFDocumentDataProcessorPage>();
        private List<bool> isPagesLoaded = new List<bool>();

        private double _actualTextSize = 100.0;
        private int _alignmentMethodIndex = 0;
        private ScrollViewer _openedToolTipViewer = null;
        private PDFReader.Document _PDFReaderDocument = null;
        private int _visiblePagesStartIndex = 0;
        private int _visiblePagesEndIndex = 0;
        private int _visiblePageIndex = 0;
        private ViewModels.UserControls.PDFDocument.AltPlusMouseWheelHandler _moveOpenedToolTipHandler;
        private ViewModels.UserControls.PDFDocument.KeyboardEventHandler _documentKeyboardHandler;

        public int VisiblePageNumber
        {
            get { return _visiblePageIndex + 1; }
            set
            {
                _visiblePageIndex = value - 1;
                OnPropertyChanged("VisiblePageNumber");
            }
        }

        public ViewModels.UserControls.PDFDocument.AltPlusMouseWheelHandler MoveOpenedToolTipHandler
        {
            get { return _moveOpenedToolTipHandler; }
            set
            {
                _moveOpenedToolTipHandler = value;
                OnPropertyChanged("MoveOpenedToolTipHandler");
            }
        }

        public ViewModels.UserControls.PDFDocument.KeyboardEventHandler DocumentKeyboardHandler
        {
            get { return _documentKeyboardHandler; }
            set
            {
                _documentKeyboardHandler = value;
                OnPropertyChanged("DocumentKeyboardHandler");
            }
        }

        public int PagesNumber
        {
            get { return Pages.Count; }
        }

        public int AlignmentMethodIndex
        {
            get
            {
                return _alignmentMethodIndex;
            }
            set
            {
                _alignmentMethodIndex = value;
                OnPropertyChanged("AlignmentMethodIndex");
                CorrectText();
            }
        }

        public double TextSize
        {
            get
            {
                return ((int)(Properties.Settings.Default.TextSize * 100)) / 100.0;
            }
            set
            {
                Properties.Settings.Default.TextSize = value;
                OnPropertyChanged("TextSize");

                double coeff = Properties.Settings.Default.TextSize / _actualTextSize;
                _actualTextSize = Properties.Settings.Default.TextSize;
                ChangeTextSize(coeff);
            }
        }

        private double _actualPageSize = 100.0;
        public double PageSize
        {
            get
            {
                return ((int)(Properties.Settings.Default.PageSize * 100)) / 100.0;
            }
            set
            {
                Properties.Settings.Default.PageSize = value;
                OnPropertyChanged("PageSize");

                double coeff = Properties.Settings.Default.PageSize / _actualPageSize;
                _actualPageSize = Properties.Settings.Default.PageSize;
                ExternalInterface.Instance.DocumentData.LoadAllPageSizes();
            }
        }

        public int TranslationsFontSize
        {
            get { return Properties.Settings.Default.TranslationsFontSize; }
            set
            {
                Properties.Settings.Default.TranslationsFontSize = value;
                _translationsMade.Clear();
                OnPropertyChanged("TranslationsFontSize");
            }
        }

        public int OriginalLanguageNumber
        {
            get { return Properties.Settings.Default.OriginalLanguage; }
            set
            {
                Properties.Settings.Default.OriginalLanguage = value;
                _translationsMade.Clear();
                OnPropertyChanged("OriginalLanguageNumber");
            }
        }

        public int TranslationLanguageNumber
        {
            get { return Properties.Settings.Default.TranslationLanguage; }
            set
            {
                Properties.Settings.Default.TranslationLanguage = value;
                OnPropertyChanged("TranslationLanguageNumber");
            }
        }

        public string OriginalLanguage
        {
            get { return LanguagesDictionary[LanguagesList[OriginalLanguageNumber]]; }
        }
        public string TranslationLanguage
        {
            get { return LanguagesDictionary[LanguagesList[TranslationLanguageNumber]]; }
        }

        private int _fontFamilySelectedIndex = Properties.Settings.Default.FontFamilySelectedIndex;
        public int FontFamilySelectedIndex
        {
            get
            {
                return Properties.Settings.Default.FontFamilySelectedIndex;
            }
            set
            {
                FontFamily selectedFontFamily = null;
                int k = 0;
                foreach (var fontFamily in ExternalInterface.Instance.FontFamilies)
                {
                    if (k++ == value)
                    {
                        selectedFontFamily = fontFamily;
                        break;
                    }
                }

                if (selectedFontFamily != null)
                {
                    Properties.Settings.Default.FontFamilySelectedIndex = value;
                    SelectedFontFamily = selectedFontFamily;
                    ChangeVisiblePagesFontFamily();
                }

                OnPropertyChanged("SelectedFontFamily");
            }
        }

        private FontFamily _selectedFontFamily = null;
        public FontFamily SelectedFontFamily
        {
            get
            {
                if(_selectedFontFamily == null)
                {
                    _selectedFontFamily = ExternalInterface.Instance.FontFamilies.FirstOrDefault();
                }
                return _selectedFontFamily;
            }
            set
            {
                _selectedFontFamily = value;
                OnPropertyChanged("SelectedFontFamily");
            }
        }

        Dictionary<string, ToolTip> _translationsMade = new Dictionary<string, ToolTip>();
        private List<TextBlock> _currentTranslatingTextBlocks = new List<TextBlock>();
        private string _currentTranslatingPhrase;
        public string CurrentTranslatingPhrase
        {
            get { return _currentTranslatingPhrase; }
            set
            {
                _currentTranslatingPhrase = value;
                OnPropertyChanged("CurrentTranslatingPhrase");
            }
        }

        private ToolTip _currentPhraseTranslationToolTip = null;
        public ToolTip CurrentPhraseTranslationToolTip
        {
            get { return _currentPhraseTranslationToolTip; }
            set
            {
                _currentPhraseTranslationToolTip = value;
                OnPropertyChanged("CurrentPhraseTranslationToolTip");
            }
        }
        private bool isCtrlWasPressed = false;
        private bool isShiftWasPressed = false;

        Dictionary<string, string> _languagesDictionary = null;
        private List<String> _languagesList = null;

        public Dictionary<string, string> LanguagesDictionary
        {
            get
            {
                if (_languagesDictionary == null)
                    _languagesDictionary = Translator.Translator.GetLanguagesDictionary();
                return _languagesDictionary;
            }
            set
            {
                _languagesDictionary = value;
                OnPropertyChanged("LanguagesDictionary");
            }
        }

        public List<String> LanguagesList
        {
            get
            {
                if (_languagesList == null)
                    _languagesList = LanguagesDictionary.Keys.ToList();
                return _languagesList;
            }
            set
            {
                _languagesList = value;
                OnPropertyChanged("LanguagesList");
            }
        }

        #endregion

        #region Constructor
        public PDFDocumentDataProcessor()
        {
            log.Info($"PDFDocumentDataProcess -> () try to create object");
            TextBlock tb = new TextBlock();
            PDFFontFamily = tb.FontFamily;
            PDFFontStyle = tb.FontStyle;
            PDFFontWeight = tb.FontWeight;
            PDFFontStretch = tb.FontStretch;
            _moveOpenedToolTipHandler = MoveOpenedToolTip;
            _documentKeyboardHandler = OpenTranslationWindow;
            _languagesDictionary = Translator.Translator.GetLanguagesDictionary();
            _languagesList = _languagesDictionary.Keys.ToList();
            log.Info($"PDFDocumentDataProcess -> () object was created");
        }
        public PDFDocumentDataProcessor(PDFReader.Document document, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch)
        {
            log.Info($"PDFDocumentDataProcess -> (,,,,) try to create object");
            PDFFontFamily = fontFamily;
            PDFFontStyle = fontStyle;
            PDFFontWeight = fontWeight;
            PDFFontStretch = fontStretch;
            _moveOpenedToolTipHandler = MoveOpenedToolTip;
            _documentKeyboardHandler = OpenTranslationWindow;
            _languagesDictionary = Translator.Translator.GetLanguagesDictionary();
            _languagesList = _languagesDictionary.Keys.ToList();
            log.Info($"PDFDocumentDataProcess -> (,,,,) object was created");
        }

        public PDFDocumentDataProcessor(PDFReader.Document document)
        {
            log.Info($"PDFDocumentDataProcess -> (_) try to create object");
            TextBlock tb = new TextBlock();
            PDFFontFamily = tb.FontFamily;
            PDFFontStyle = tb.FontStyle;
            PDFFontWeight = tb.FontWeight;
            PDFFontStretch = tb.FontStretch;
            _moveOpenedToolTipHandler = MoveOpenedToolTip;
            _documentKeyboardHandler = OpenTranslationWindow;
            _languagesDictionary = Translator.Translator.GetLanguagesDictionary();
            _languagesList = _languagesDictionary.Keys.ToList();
            log.Info($"PDFDocumentDataProcess -> (_) try to load pages");
            LoadPages(document);
            log.Info($"PDFDocumentDataProcess -> (_) object was created");
        }
        #endregion

        #region Methods
        public void Clear()
        {
            while(Pages.Count > 0)
            {
                Pages.RemoveAt(0);
            }

            _actualTextSize = 100;
            _actualPageSize = 100;
            TextSize = 100;
            PageSize = 100;
        }

        public void LoadPage(int pageIndex)
        {
            log.Info($"LoadPage -> try to load page {pageIndex}");
            if (!isPagesLoaded[pageIndex])
            {
                PDFReader.Page page = _PDFReaderDocument.ReadPDFPage(pageIndex);
                Pages[pageIndex] = ProcessPage(page, pageIndex);
                isPagesLoaded[pageIndex] = true;
            }

            if(Pages[pageIndex].PageSize != PageSize || Pages[pageIndex].TextSize != TextSize)
            {
                ChangePageSize(pageIndex);
                Pages[pageIndex].PageSize = PageSize;
                Pages[pageIndex].TextSize = TextSize;
            }
            log.Info($"LoadPage -> page {pageIndex} was loaded");
        }

        public PDFDocumentDataProcessorPage ProcessPage(PDFReader.Page readedPage, int pageIndex)
        {
            log.Info($"ProcessPage -> try to process page {pageIndex}");
            ObservableCollection<ObservableCollection<ObservableCollection<TextBlock>>> currentTextBlocks = new ObservableCollection<ObservableCollection<ObservableCollection<TextBlock>>>();

            foreach (var readedBlock in readedPage.WordBlocks)
            {
                ObservableCollection<ObservableCollection<TextBlock>> currentTextLines = new ObservableCollection<ObservableCollection<TextBlock>>();
                double[] linesSizes = new double[readedBlock.TextLines.Count];

                double blockFontSize = GetFontSizeForText(readedBlock.Text, PDFFontFamily, PDFFontStyle, PDFFontWeight, PDFFontStretch,
                    readedBlock.BoundingBox.Width, readedBlock.BoundingBox.Height);

                for (int i = 0; i < readedBlock.TextLines.Count; ++i)
                {
                    ObservableCollection<TextBlock> currentTextLine = new ObservableCollection<TextBlock>();
                    var readedTextLine = readedBlock.TextLines[i];
                    foreach (var readedWord in readedTextLine.Words)
                    {
                        var words = GetWords(readedWord);
                        foreach (var word in words)
                        {
                            TextBlock textBlock = new TextBlock();

                            textBlock.Text = word.Text;
                            try
                            {
                                double wordFontSize = GetFontSizeForText(word.Text, PDFFontFamily, PDFFontStyle, PDFFontWeight, PDFFontStretch,
                                    word.BoundingBox.Width, word.BoundingBox.Height);

                                if (blockFontSize / wordFontSize > 1.5)
                                    textBlock.FontSize = wordFontSize;
                                else
                                    textBlock.FontSize = blockFontSize;
                                textBlock.FontWeight = PDFFontWeight;
                                textBlock.FontStyle = PDFFontStyle;
                                textBlock.FontStretch = PDFFontStretch;
                                textBlock.FontFamily = PDFFontFamily;
                                textBlock.MouseLeftButtonDown += TextBlock_MouseLeftButtonDown;

                                Size wordSize = GetTextSize(textBlock);

                                textBlock.Margin = new Thickness(word.BoundingBox.Left, readedPage.Height - readedBlock.TextLines[i].BoundingBox.Top - wordSize.Height, 0, 0);

                                currentTextLine.Add(textBlock);
                            }
                            catch (Exception ex)
                            {
                                log.Error($"ProcessPage -> process page exception: {ex.Message}");
                            }
                        }
                    }
                    CorrectBlocksSize(currentTextLine);
                    currentTextLines.Add(currentTextLine);
                }
                currentTextBlocks.Add(currentTextLines);
            }

            ObservableCollection<Image> images = GetImages(readedPage.Images, readedPage.Height);

            log.Info($"ProcessPage -> page {pageIndex} was processed");

            return new PDFDocumentDataProcessorPage(currentTextBlocks, images, readedPage.Width, readedPage.Height, pageIndex);
        }

        public void ChangeVisiblePagesFontFamily()
        {
            log.Info($"ChangeVisiblePagesFontFamily -> try to change from {_visiblePagesStartIndex} to {_visiblePagesEndIndex}");
            try
            {
                for (int i = _visiblePagesStartIndex; i < _visiblePagesEndIndex; ++i)
                {
                    for (int j = 0; j < Pages[i].DataBlocks.Count; ++j)
                    {
                        for (int jx = 0; jx < Pages[i].DataBlocks[j].Count; ++jx)
                        {
                            for (int l = 0; l < Pages[i].DataBlocks[j][jx].Count; ++l)
                            {
                                Pages[i].DataBlocks[j][jx][l].FontFamily = SelectedFontFamily;
                            }
                        }
                    }
                }
                log.Info($"ChangeVisibilePagesFontFamily -> succesfully change from {_visiblePagesStartIndex} to {_visiblePagesEndIndex}");
            } catch(Exception ex)
            {
                log.Info($"ChangeVisibilePagesFontFamily -> succesfully change from {_visiblePagesStartIndex} to {_visiblePagesEndIndex} exception: {ex.Message}");
            }
        }

        public void CorrectBlocksSize(ObservableCollection<TextBlock> textLine)
        {
            try
            {
                double minCoeff = 100;
                for (int i = 1; i < textLine.Count; ++i)
                {
                    double width = GetTextSize(textLine[i - 1]).Width;
                    if (textLine[i - 1].Margin.Left + width >
                        textLine[i].Margin.Left)
                    {
                        double curCoeff = (textLine[i].Margin.Left - textLine[i - 1].Margin.Left) /
                            (width);
                        double spaceCoeff = (textLine[i - 1].Text.Length - 1.0) / (textLine[i - 1].Text.Length);
                        curCoeff *= spaceCoeff;
                        if (curCoeff < minCoeff)
                            minCoeff = curCoeff;
                    }
                }

                if (minCoeff > 1.0 || minCoeff < 0.5)
                    return;

                for (int i = 0; i < textLine.Count; ++i)
                    textLine[i].FontSize *= minCoeff;
            } catch(Exception ex)
            {
                log.Error($"CorrectBlockSize -> exception: {ex.Message}");
            }
        }

        public void CalculateVisiblePages(double verticalOffset, double viewportHeight)
        {
            try
            {
                int startIndex = 0;
                int endIndex = Pages.Count - 1;
                double currentHeight = 0.0;
                for (int i = 0; i < Pages.Count; ++i)
                {
                    currentHeight += Pages[i].Height;
                    if (currentHeight > verticalOffset)
                    {
                        startIndex = i;
                        break;
                    }
                }

                VisiblePageNumber = startIndex + 1;

                for (int i = startIndex + 1; i < Pages.Count; ++i)
                {
                    currentHeight += Pages[i].Height;
                    if (currentHeight > verticalOffset + viewportHeight)
                    {
                        endIndex = i;
                        break;
                    }
                }

                if (startIndex - 3 < 0)
                    startIndex = 0;
                else
                    startIndex -= 3;

                if (endIndex + 3 > Pages.Count)
                    endIndex = Pages.Count;
                else
                    endIndex += 3;

                _visiblePagesStartIndex = startIndex;
                _visiblePagesEndIndex = endIndex;
            } catch(Exception ex)
            {
                log.Error($"CalculateVisiblePages -> exception: {ex.Message}");
            }
        }

        public void LoadAllPageSizes()
        {
            try
            {
                for (int i = 0; i < Pages.Count; ++i)
                {
                    if (Pages[i].PageSize != i)
                    {
                        double coeff = PageSize / Pages[i].PageSize;
                        ChangePageSize(i);
                        Pages[i].PageSize = PageSize;
                        Pages[i].TextSize = TextSize;
                    }
                }
            } catch(Exception ex)
            {
                log.Error($"LoadAllPageSizes -> exception: {ex.Message}");
            }
        }

        public void LoadVisiblePages()
        {
            for (int i = _visiblePagesStartIndex; i < _visiblePagesEndIndex; ++i)
                LoadPage(i);
        }

        public void LoadPages(PDFReader.Document document)
        {
            log.Info($"LoadPages -> start");
            _PDFReaderDocument = document;
            int pageIndex = 0;
            foreach (var readedPage in document.Pages)
            {
                Pages.Add(new PDFDocumentDataProcessorPage(new ObservableCollection<ObservableCollection<ObservableCollection<TextBlock>>>(),
                    new ObservableCollection<Image>(), readedPage.Width, readedPage.Height, pageIndex++));
            }

            isPagesLoaded = new List<bool>();
            for (int i = 0; i < Pages.Count; ++i)
                isPagesLoaded.Add(false);
            log.Info($"LoadPages -> end");
        }

        private Dictionary<Translator.Tag, List<Translator.Tag>> GetTranslation(string text)
        {
            var result = Translator.Translator.Translate(text, OriginalLanguage, TranslationLanguage);
            return result;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                ToolTip toolTip = new ToolTip();

                if (sender is TextBlock textBlock)
                {
                    textBlock.Foreground = Brushes.Blue;

                    if (isCtrlWasPressed)
                    {
                        CurrentTranslatingPhrase += " " + textBlock.Text;
                    }
                    else if (isShiftWasPressed)
                    {
                        CurrentTranslatingPhrase += textBlock.Text;
                        CurrentTranslatingPhrase = CurrentTranslatingPhrase.Replace("-", "");
                    }
                    else
                    {
                        CurrentTranslatingPhrase = textBlock.Text;
                        _currentTranslatingTextBlocks.Clear();
                    }

                    _currentTranslatingTextBlocks.Add(textBlock);

                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        isCtrlWasPressed = true;
                        isShiftWasPressed = false;
                        return;
                    }
                    else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        isShiftWasPressed = true;
                        isCtrlWasPressed = false;
                        return;
                    }
                    else
                    {
                        isCtrlWasPressed = false;
                        isShiftWasPressed = false;
                    }
                    if (!_translationsMade.ContainsKey(CurrentTranslatingPhrase))
                    {
                        if (textBlock.ToolTip != null)
                        {
                            if (textBlock.ToolTip is ScrollViewer openedViewer)
                            {
                                _openedToolTipViewer = openedViewer;
                            }
                            return;
                        }
                        toolTip = GetTranslate();
                    }
                    else
                    {
                        toolTip = _translationsMade[CurrentTranslatingPhrase];
                        if (toolTip.Content is ScrollViewer viewer)
                        {
                            if (viewer.Content is Grid grid)
                            {
                                try
                                {
                                    foreach (var child in grid.Children)
                                    {
                                        if (child is TextBlock tb)
                                        {
                                            if(tb.FontSize != TranslationsFontSize)
                                            {
                                                toolTip = GetTranslate();
                                                break;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error($"TextBlock_MouseLeftButton -> exception while correct translations font size: {ex.Message}");
                                }
                            }
                        }
                        if (toolTip.Content is ScrollViewer currentOpenedViewer)
                        {
                            _openedToolTipViewer = currentOpenedViewer;
                        }
                    }

                    toolTip.IsOpen = true;
                    textBlock.ToolTip = toolTip;
                    CurrentPhraseTranslationToolTip = toolTip;

                    textBlock.MouseLeave += TextBlock_MouseLeave;
                }
            } catch(Exception ex)
            {
                log.Error($"TextBlock_MouseLeftButtonDown -> exception: {ex.Message}");
            }
        }

        public ToolTip GetTranslate()
        {
            ToolTip toolTip = new ToolTip();
            toolTip.Opened += ToolTip_Opened;
            toolTip.Closed += ToolTip_Closed;
            try
            {
                Dictionary<Translator.Tag, List<Translator.Tag>> dict = GetTranslation(CurrentTranslatingPhrase);
                Grid grid = new Grid();
                ColumnDefinition categoryColumn = new ColumnDefinition();
                ColumnDefinition translationsColumn = new ColumnDefinition();

                grid.ColumnDefinitions.Add(categoryColumn);
                grid.ColumnDefinitions.Add(translationsColumn);

                int rowIndex = 0;
                foreach (var pair in dict)
                {
                    RowDefinition row = new RowDefinition();
                    grid.RowDefinitions.Add(row);

                    TextBlock category = new TextBlock();
                    category.Text = pair.Key.text + "  ";
                    category.Foreground = Brushes.Blue;

                    category.FontSize = TranslationsFontSize;

                    Grid.SetRow(category, rowIndex);
                    Grid.SetColumn(category, 0);
                    grid.Children.Add(category);

                    WrapPanel translationsPanel = new WrapPanel();
                    translationsPanel.Orientation = Orientation.Horizontal;

                    foreach (var el in pair.Value)
                    {
                        TextBlock text = new TextBlock();
                        text.Text = el.text;
                        if (el.type == Translator.Tag.TagType.a)
                            text.Foreground = Brushes.Blue;
                        if (el.type == Translator.Tag.TagType.span)
                            text.Foreground = Brushes.Gray;
                        text.FontSize = TranslationsFontSize;

                        translationsPanel.Children.Add(text);
                    }

                    Grid.SetRow(translationsPanel, rowIndex);
                    Grid.SetColumn(translationsPanel, 1);
                    grid.Children.Add(translationsPanel);

                    ++rowIndex;
                }

                ScrollViewer viewer = new ScrollViewer();
                viewer.Content = grid;
                toolTip.Content = viewer;
                _openedToolTipViewer = viewer;

                _translationsMade.Add(CurrentTranslatingPhrase, toolTip);
                CurrentPhraseTranslationToolTip = toolTip;
            } catch(Exception ex)
            {
                log.Error($"GetTranslate -> exception: {ex.Message}");
            }
            return toolTip;
        }

        private void ToolTip_Closed(object sender, RoutedEventArgs e)
        {
            if (sender is ToolTip toolTip)
                if (toolTip.Content is ScrollViewer viewer)
                    _openedToolTipViewer = null;
        }

        private void ToolTip_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ToolTip toolTip)
                if(toolTip.Content is ScrollViewer viewer)
                    _openedToolTipViewer = viewer;
        }

        private void TextBlock_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (sender is TextBlock textBlock)
                {
                    for (int i = 0; i < _currentTranslatingTextBlocks.Count; ++i)
                        _currentTranslatingTextBlocks[i].Foreground = Brushes.Black;
                    if (textBlock.ToolTip is ToolTip toolTip)
                    {
                        toolTip.IsOpen = false;
                    }
                    textBlock.ToolTip = null;
                    textBlock.MouseLeave -= TextBlock_MouseLeave;
                }
            } catch(Exception ex)
            {
                log.Error($"TextBlock_MouseLeave -> exception: {ex.Message}");
            }
        }

        public bool MoveOpenedToolTip(int delta)
        {
            if(_openedToolTipViewer != null)
            {
                try
                {
                    double offset = _openedToolTipViewer.VerticalOffset - delta;
                    if (offset < 0.0)
                        offset = 0.0;
                    if (offset > _openedToolTipViewer.ScrollableHeight)
                        offset = _openedToolTipViewer.ScrollableHeight;
                    _openedToolTipViewer.ScrollToVerticalOffset(offset);

                    return true;
                }
                catch { }
            }

            return false;
        }

        public bool OpenTranslationWindow()
        {
            Views.TranslationWindowView translationWindow = new Views.TranslationWindowView();
            translationWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            translationWindow.Show();
            return true;
        }

        private ObservableCollection<Image> GetImages(List<PDFReader.Image> images, double height)
        {
            ObservableCollection<Image> result = new ObservableCollection<Image>();
            try
            {
                foreach (var image in images)
                {
                    try
                    {
                        Image img = new Image();
                        img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(image.Filename));
                        img.Margin = new Thickness(image.Left, height - image.Top, 0, 0);
                        img.Width = image.Width;
                        img.Height = image.Height;
                        img.HorizontalAlignment = HorizontalAlignment.Left;
                        img.VerticalAlignment = VerticalAlignment.Top;
                        result.Add(img);
                    }
                    catch (Exception ex)
                    {
                        string s = ex.Message;
                    }
                }
            } catch(Exception ex)
            {
                log.Error($"GetImages exception: {ex.Message}");
            }

            return result;
        }

        //Выравнивание текста
        private void CorrectText()
        {
            try
            {
                foreach (var page in Pages)
                {
                    foreach (ObservableCollection<ObservableCollection<TextBlock>> textLines in page.DataBlocks)
                    {
                        if (textLines.Count > 1)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                double maxRight = 0;
                                foreach (ObservableCollection<TextBlock> textLine in textLines)
                                {
                                    double currentRight = textLine[textLine.Count - 1].Margin.Left + textLine[textLine.Count - 1].ActualWidth;
                                    if (maxRight < currentRight)
                                        maxRight = currentRight;
                                }
                                foreach (ObservableCollection<TextBlock> textLine in textLines)
                                {
                                    TextBlock lastWord = textLine[textLine.Count - 1];
                                    double freeSpace = maxRight - (lastWord.Margin.Left + lastWord.ActualWidth);
                                    if (freeSpace > 0 && freeSpace < (lastWord.Margin.Left + lastWord.ActualWidth - textLine[0].Margin.Left) / 5)
                                    {
                                        for (int i = 0; i < textLine.Count; ++i)
                                        {
                                            double wordShift = freeSpace / (textLine.Count - 1);
                                            textLine[i].Margin = new Thickness(textLine[i].Margin.Left + wordShift * i, textLine[i].Margin.Top,
                                                    textLine[i].Margin.Right, textLine[i].Margin.Bottom);
                                        }
                                    }
                                }
                            });
                        }
                    }
                }
            } catch(Exception ex)
            {
                log.Error($"CorrectText -> exception: {ex.Message}");
            }
        }
        //Некоторые слова сегментированы неверно, и в одно слова определяются символы из разных. Для этого и производится эта обработка
        private List<UglyToad.PdfPig.Content.Word> GetWords(UglyToad.PdfPig.Content.Word word)
        {
            List<UglyToad.PdfPig.Content.Word> words = new List<UglyToad.PdfPig.Content.Word>();
            List<UglyToad.PdfPig.Content.Letter> letters = new List<UglyToad.PdfPig.Content.Letter>();

            foreach (var letter in word.Letters)
            {
                if (letters.Count == 0)
                    letters.Add(letter);
                else
                {
                    if (letters[0].StartBaseLine.Y == letter.StartBaseLine.Y)
                        letters.Add(letter);
                    else
                    {
                        UglyToad.PdfPig.Content.Word w = new UglyToad.PdfPig.Content.Word(letters);
                        words.Add(w);
                        letters = new List<UglyToad.PdfPig.Content.Letter>();
                        letters.Add(letter);
                    }
                }
            }

            if (letters.Count > 0)
            {
                UglyToad.PdfPig.Content.Word w = new UglyToad.PdfPig.Content.Word(letters);
                words.Add(w);
            }

            return words;
        }

        private double GetFontSizeForText(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double width, double height)
        {
            Size size = MeasureTextSize(text, fontFamily, fontStyle, fontWeight, fontStretch, 10);
            double widthCoeff = size.Width / width;
            double heightCoeff = size.Height / height;
            return widthCoeff > heightCoeff ? 10 / heightCoeff : 10 / widthCoeff;
        }

        private Size MeasureTextSize(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            FormattedText ft = new FormattedText(text,
                                                 CultureInfo.CurrentCulture,
                                                 FlowDirection.LeftToRight,
                                                 new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                                                 fontSize,
                                                 Brushes.Black);
            return new Size(ft.Width, ft.Height);
        }

        private Size GetTextSize(TextBlock textBlock)
        {
            FormattedText ft = new FormattedText(textBlock.Text, CultureInfo.CurrentUICulture, textBlock.FlowDirection,
                                                 new Typeface(textBlock.FontFamily, textBlock.FontStyle,
                                                              textBlock.FontWeight, textBlock.FontStretch),
                                                 textBlock.FontSize, textBlock.Foreground
                );

            return new Size(ft.Width, ft.Height);
        }

        public void ChangeTextSize(double coeff)
        {
            ChangeVisiblePageSizes();
            return;
            for (int i = 0; i < Pages.Count; ++i)
            {
                for (int j = 0; j < Pages[i].DataBlocks.Count; ++j)
                {
                    for (int jx = 0; jx < Pages[i].DataBlocks[j].Count; ++jx)
                    {
                        for (int l = 0; l < Pages[i].DataBlocks[j][jx].Count; ++l)
                        {
                            Pages[i].DataBlocks[j][jx][l].FontSize *= coeff;
                        }
                    }
                }
            }
        }

        public void ChangePageSize(int pageIndex)
        {
            try
            {
                double pageCoeff = PageSize / Pages[pageIndex].PageSize;
                double fontCoeff = pageCoeff * TextSize / Pages[pageIndex].TextSize;

                Pages[pageIndex].Width *= pageCoeff;
                Pages[pageIndex].Height *= pageCoeff;

                for (int j = 0; j < Pages[pageIndex].DataBlocks.Count; ++j)
                {
                    for (int jx = 0; jx < Pages[pageIndex].DataBlocks[j].Count; ++jx)
                    {
                        for (int l = 0; l < Pages[pageIndex].DataBlocks[j][jx].Count; ++l)
                        {
                            Pages[pageIndex].DataBlocks[j][jx][l].FontSize *= fontCoeff;
                            Thickness th = Pages[pageIndex].DataBlocks[j][jx][l].Margin;
                            Pages[pageIndex].DataBlocks[j][jx][l].Margin = new Thickness(th.Left * pageCoeff, th.Top * pageCoeff, 0, 0);
                        }
                    }
                }

                for (int j = 0; j < Pages[pageIndex].Images.Count; ++j)
                {
                    Thickness th = Pages[pageIndex].Images[j].Margin;
                    Pages[pageIndex].Images[j].Margin = new Thickness(th.Left * pageCoeff, th.Top * pageCoeff, 0, 0);
                    Pages[pageIndex].Images[j].Width *= pageCoeff;
                    Pages[pageIndex].Images[j].Height *= pageCoeff;
                }
            } catch(Exception ex)
            {
                log.Error($"ChangePageSize -> exception: {ex.Message}");
            }
        }

        public void ChangeVisiblePageSizes()
        {
            try
            {
                for (int i = _visiblePagesStartIndex; i < _visiblePagesEndIndex; ++i)
                {
                    if (Pages[i].PageSize != PageSize || Pages[i].TextSize != TextSize)
                    {
                        ChangePageSize(i);
                        Pages[i].PageSize = PageSize;
                        Pages[i].TextSize = TextSize;
                    }
                }
            } catch(Exception ex)
            {
                log.Error($"ChangeVisiblePageSizes -> exception: {ex.Message}");
            }
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
