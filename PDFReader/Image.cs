using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader
{
    public class Image
    {
        public string Filename;
        public double Left;
        public double Top;
        public double Width;
        public double Height;

        public Image() { }

        public Image(string filename, double left, double top, double width, double height)
        {
            Filename = filename;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
    }
}
