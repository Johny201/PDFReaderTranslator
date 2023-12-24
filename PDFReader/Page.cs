using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

namespace PDFReader
{
    public class Page : INotifyPropertyChanged
    {
        public IReadOnlyList<TextBlock> WordBlocks;
        public List<Image> Images;
        public double Width;
        public double Height;

        public Page(IReadOnlyList<TextBlock> wordBlocks, List<Image> images, double width, double height)
        {
            WordBlocks = wordBlocks;
            Images = images;
            Width = width;
            Height = height;
        }

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
