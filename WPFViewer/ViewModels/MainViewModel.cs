using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using WPFViewer.Commands;

namespace WPFViewer.ViewModels
{
    public class MainViewModel
    {
        private double _scrollableHeight = 0.0;
        private double _viewerHeight = 0.0;
        private double _verticalOffset = 0.0;
        private int? _documentPageCount = null;

        public void MouseWriteButtonDownCommand()
        {
        }

        public MainViewModel()
        {
            _documentPageCount = ExternalInterface.Instance?.DocumentData?.Pages?.Count;
        }

        public void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer viewer)
            {
                if(ExternalInterface.Instance.IsNewDocumentLoaded)
                {
                    _scrollableHeight = 0.0;
                    _verticalOffset = viewer.VerticalOffset;
                    viewer.ScrollToVerticalOffset(0);
                    ExternalInterface.Instance.IsNewDocumentLoaded = false;

                    ExternalInterface.Instance.DocumentData.CalculateVisiblePages(viewer.VerticalOffset, viewer.Height);
                    ExternalInterface.Instance.DocumentData.LoadVisiblePages();

                    return;
                }
                if (_scrollableHeight == 0.0)
                {
                    _scrollableHeight = viewer.ScrollableHeight;
                    _viewerHeight = viewer.Height;
                    _verticalOffset = viewer.VerticalOffset;
                }

                ExternalInterface.Instance.DocumentData.CalculateVisiblePages(viewer.VerticalOffset, viewer.ViewportHeight);
                ExternalInterface.Instance.DocumentData.LoadVisiblePages();
                ExternalInterface.Instance.DocumentData.ChangeVisiblePagesFontFamily();

                if (_scrollableHeight != viewer.ScrollableHeight)
                {
                    if(_documentPageCount == null || _documentPageCount == ExternalInterface.Instance.DocumentData.Pages.Count)
                    {
                        //viewer.ScrollToVerticalOffset((viewer.VerticalOffset + viewer.Height / 2.0) / (_scrollableHeight + viewer.Height) * (viewer.ScrollableHeight + viewer.Height) - viewer.Height / 2.0);
                        if(_scrollableHeight + _viewerHeight != 0)
                        {
                            double verticalOffset = (_verticalOffset + viewer.Height / 2.0) / (_scrollableHeight + viewer.Height) * (viewer.ScrollableHeight + viewer.Height) - viewer.Height / 2.0;
                            //double verticalOffset = (viewer.ScrollableHeight + viewer.Height) * (_verticalOffset + viewer.Height) / (_scrollableHeight - _verticalOffset + viewer.Height) / (1.0 + (_verticalOffset + viewer.Height) / (_scrollableHeight - _verticalOffset + viewer.Height));
                            //double verticalOffset = (_verticalOffset * viewer.ScrollableHeight) / (_scrollableHeight - _verticalOffset) / (1.0 + _verticalOffset / (_scrollableHeight - _verticalOffset));
                            //double relativePosition = (_verticalOffset) / (_scrollableHeight);
                            //double newPosition = relativePosition * (viewer.ScrollableHeight);
                            //double verticalOffset = newPosition;
                            viewer.ScrollToVerticalOffset(verticalOffset);
                        }
                        //viewer.ScrollToVerticalOffset()
                    }
                    else
                        _documentPageCount = ExternalInterface.Instance?.DocumentData?.Pages?.Count;
                }

                _scrollableHeight = viewer.ScrollableHeight;
                _viewerHeight = viewer.Height;
                _verticalOffset = viewer.VerticalOffset;
            }
        }
    }
}
