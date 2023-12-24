using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;

namespace PDFReader
{
    public class Document
    {
        public List<Page> Pages;
        public string Filename;
        private UglyToad.PdfPig.Content.Page[] _loadedPages;
        private Logger.Logger log = Logger.Logger.GetLogger("PDFReader", "Document");

        public Document(string filename)
        {
            Filename = filename;
            Pages = new List<Page>();
        }

        public bool LoadPDFDocument()
        {
            try
            {
                log.Info($"LoadPDFDocument -> Try to read {Filename}");
                using (var pdf = PdfDocument.Open(Filename))
                {
                    var enumerableLoadedPages = pdf.GetPages();
                    _loadedPages = new UglyToad.PdfPig.Content.Page[enumerableLoadedPages.Count()];
                    int pageIndex = 0;
                    foreach (var loadedPage in enumerableLoadedPages)
                    {
                        Pages.Add(new Page(null, null, loadedPage.Width, loadedPage.Height));
                        _loadedPages[pageIndex++] = loadedPage;
                    }
                }

                log.Info($"LoadPDFDocument -> Successfully read {Filename}");

                return true;
            } catch(Exception ex)
            {
                log.Error("LoadPDFDocument -> exception: " + ex.Message);
            }

            return false;
        }

        public Page ReadPDFPage(int pageIndex)
        {
            try
            {
                log.Info($"ReadPDFPage -> Try to read page {pageIndex} from {Filename}");
                var readingPage = _loadedPages[pageIndex];
                var wordBlocks = RecursiveXYCut.Instance.GetBlocks(readingPage.GetWords());
                var readingImages = readingPage.GetImages();
                string[] aFname = Filename.Split('\\');
                string documentName = aFname[aFname.Length - 1];

                int imgNumber = 1;
                var images = ProcessImages(documentName, readingImages, pageIndex + 1, ref imgNumber);

                log.Info($"ReadPDFPage -> Successfully read page {pageIndex} from {Filename}");

                return new Page(wordBlocks, images, readingPage.Width, readingPage.Height);
            } catch(Exception ex)
            {
                log.Info($"ReadPDFPage -> Read page {pageIndex} from {Filename} exception: " + ex.Message);
            }

            return new Page(new List<UglyToad.PdfPig.DocumentLayoutAnalysis.TextBlock>(), new List<Image>(), 0, 0);
        }

        public bool ReadPDFDocument()
        {
            try
            {
                log.Info($"ReadPDFDocuemnt -> try to read {Filename}");
                Pages = new List<Page>();
                int imgNumber = 1;
                using (var pdf = PdfDocument.Open(Filename))
                {
                    int pageIndex = 0;
                    foreach (var readingPage in pdf.GetPages())
                    {
                        var wordBlocks = RecursiveXYCut.Instance.GetBlocks(readingPage.GetWords());
                        var readingImages = readingPage.GetImages();
                        string[] aFname = Filename.Split('\\');
                        string documentName = aFname[aFname.Length - 1];

                        var images = ProcessImages(documentName, readingImages, pageIndex + 1, ref imgNumber);
                        Pages.Add(new Page(wordBlocks, images, readingPage.Width, readingPage.Height));
                    }
                }
                log.Info($"ReadPDFDocument -> successfully read {Filename}");
            }
            catch (Exception ex)
            {
                log.Error($"ReadPDFDocument -> exception: " + ex.Message);
            }

            return false;
        }

        private List<Image> ProcessImages(string documentName, IEnumerable<IPdfImage> pdfImages, int pageNumber, ref int imgNumber)
        {
            string storeDirectory = $"DocumentsData\\{documentName}";
            log.Info($"ProcessImage -> try to process images from {storeDirectory}");
            if (!Directory.Exists(storeDirectory))
            {
                try
                {
                    if (!Directory.Exists("DocumentsData"))
                    {
                        Directory.CreateDirectory("DocumentsData");
                    }
                    Directory.CreateDirectory(storeDirectory);
                }
                catch (Exception ex)
                {
                    log.Error($"ProcessImages -> 1st block from {storeDirectory} exception: " + ex.Message);
                }
            }
            List<Image> result = new List<Image>();
            foreach (var pdfImage in pdfImages)
            {
                try
                {
                    byte[] imageBytes = null;
                    pdfImage.TryGetPng(out imageBytes);
                    if (pdfImage.IsInlineImage)
                    {
                        var aImg = pdfImage;
                    }
                    if (imageBytes == null && pdfImage.IsInlineImage)
                        continue;
                    else if(imageBytes == null)
                    {
                        imageBytes = new byte[pdfImage.RawBytes.Count];
                        for (int i = 0; i < pdfImage.RawBytes.Count; ++i)
                            imageBytes[i] = pdfImage.RawBytes[i];
                    }

                    string imageFilename = $"{storeDirectory}\\{pageNumber}_{imgNumber++}.png";
                    result.Add(new Image(imageFilename, pdfImage.Bounds.Left, pdfImage.Bounds.Top, pdfImage.Bounds.Width, pdfImage.Bounds.Height));
                    File.WriteAllBytes(imageFilename, imageBytes);
                }
                catch (Exception ex)
                {
                    log.Error($"ProcessImages -> 2nd block from {storeDirectory} exception: " + ex.Message);
                }
            }

            log.Info($"ProcessImages -> from {storeDirectory} was processed");

            return result;
        }
    }
}
