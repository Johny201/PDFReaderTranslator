using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WPFViewer
{
    public class PDFDocumentDataProcessorPage : INotifyPropertyChanged
    {
        private double _width;
        private double _height;
        private double _pageSpace = 10.0;
        private int _pageIndex;
        private double _pageSize = 100.0;
        private double _textSize = 100.0;
        private System.Windows.Media.FontFamily _fontFamily = null;

        public ObservableCollection<ObservableCollection<ObservableCollection<TextBlock>>> DataBlocks;
        public ObservableCollection<Image> Images;
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged("Width");
            }
        }
        public double Height
        {
            get { return _height; }
            set
            {
                if(_height != 0.0)
                    _pageSpace *= (value / _height);
                _height = value;
                OnPropertyChanged("Height");
            }
        }

        public double PageSpace
        {
            get { return _pageSpace; }
            set
            {
                _pageSpace = value;
                OnPropertyChanged("PageSpace");
            }
        }

        public double TextSize
        {
            get { return _textSize; }
            set
            {
                _textSize = value;
                OnPropertyChanged("TextSize");
            }
        }

        public int PageIndex
        {
            get { return _pageIndex; }
            set
            {
                _pageIndex = value;
                OnPropertyChanged("PageIndex");
            }
        }

        public double PageSize
        {
            get { return _pageSize; }
            set
            {
                _pageSize = value;
                OnPropertyChanged("PageSize");
            }
        }

        public PDFDocumentDataProcessorPage(ObservableCollection<ObservableCollection<ObservableCollection<TextBlock>>> dataBlocks, ObservableCollection<Image> images, double width, double height, int pageIndex = -1)
        {
            DataBlocks = dataBlocks;
            Images = images;
            Width = width;
            Height = height;
            _pageIndex = pageIndex;

            DataBlocks.CollectionChanged += DataBlocks_CollectionChanged;
        }

        private void DataBlocks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
                OnPropertyChanged(sender, "DataBlocks");
        }

        #region Events
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void OnPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
