using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Translator
{
    public class Translator
    {
        private static HttpClient httpClient = new HttpClient();
        private static HtmlParser parser = new HtmlParser();
        private static Logger.Logger log = Logger.Logger.GetLogger("Translator", "Translator");
        private static Dictionary<string, int> LanguageNumberCorrespondence = GetLanguageNumberCorrespondence();

        public static Dictionary<Tag, List<Tag>> Translate(string word, string originalLanguage, string translationLanguage)
        {
            Dictionary<Tag, List<Tag>> dict = new Dictionary<Tag, List<Tag>>();
            try
            {
                int originalLanguageNumber = LanguageNumberCorrespondence[originalLanguage];
                int translationLanguageNumber = LanguageNumberCorrespondence[translationLanguage];
                dict = TranslateUnsafe(word, originalLanguageNumber, translationLanguageNumber);
                log.Info($"Translate -> {word} was translated");
            } catch(Exception ex)
            {
                log.Error($"Translate -> exception while process response for request with word {word}: " + ex.Message);
            }

            return dict;
        }

        public static Dictionary<Tag, List<Tag>> TranslateUnsafe(string word, int originalLanguage, int translationLanguage)
        {
            log.Info($"TranslateUnsafe -> Try to translate text: {word}");
            word = word.Replace("+", "%2B").Replace("@", "%40").Replace("#", "%23").Replace(";", "%3B").Replace("%", "%25").Replace(":", "%3A").Replace("?", "%3F")
                .Replace("&", "%26").Replace(",", "%2C").Replace("/", "%2F").Replace("=", "%3D").Replace("+", "%2B").Replace(" ", "+");
            Dictionary<Tag, List<Tag>> dict = new Dictionary<Tag, List<Tag>>();
            char[] chars = word.ToCharArray();

            List<string> result = new List<string>();
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.multitran.com/m.exe?ll1={originalLanguage}&ll2={translationLanguage}&CL={originalLanguage}&s={word}");

            log.Info($"TranslateUnsafe -> Generate request: {request}");

            var taskResponse = httpClient.SendAsync(request);
            while (!taskResponse.IsCompleted)
                System.Threading.Thread.Sleep(20);
            var response = taskResponse.Result;
            var taskResponseText = response.Content.ReadAsStringAsync();
            while (!taskResponseText.IsCompleted)
                System.Threading.Thread.Sleep(20);
            string responseText = taskResponseText.Result;

            var document = parser.ParseDocument(responseText);
            IElement td_gray = document.QuerySelector("td.gray");

            log.Info($"TranslateUnsafe -> Starting parse response for request {request}");

            if (td_gray == null || td_gray.ParentElement == null || td_gray.ParentElement.ParentElement == null)
                return dict;

            IElement translationsTable = document.QuerySelector("td.gray").ParentElement.ParentElement;

            log.Info($"TranslateUnsafe -> Continue parse response for request {request}");

            foreach (var tr in translationsTable.Children)
            {
                Tag category = new Tag(Tag.TagType.a, "");
                List<Tag> translations = new List<Tag>(0);
                bool isFirstTranslationInCategory = true;
                foreach(var td in tr.Children)
                {
                    if(td.ClassName == "subj")
                    {
                        if (td.Children != null && td.Children.Length > 0)
                            //result.Add(td.Children[0].TextContent + " ");
                            category.text = td.Children[0].TextContent;
                    }
                    else if(td.ClassName == "trans" && td.ChildNodes != null)
                    {
                        List<string> blocks = td.TextContent.Split(';').ToList();
                        try
                        {
                            foreach (var el in td.ChildNodes)
                            {
                                if(el != null)
                                {
                                    if (el.TextContent == null)
                                        continue;
                                    if(el.NodeName == null)
                                    {
                                        translations.Add(new Tag(Tag.TagType.another, el.TextContent));
                                    }
                                    if (el.NodeName.ToLower() == "a")
                                    {
                                        translations.Add(new Tag(Tag.TagType.a, el.TextContent));
                                    }
                                    else if (el.NodeName.ToLower() == "span")
                                        translations.Add(new Tag(Tag.TagType.span, el.TextContent));
                                    else
                                        translations.Add(new Tag(Tag.TagType.another, el.TextContent));
                                }
                            }
                        } catch(Exception ex)
                        {
                            log.Error($"TranslateUnsafe -> exception while process response for request {request}: " + ex.Message);
                        }

                    }
                }

                if(translations.Count != 0)
                    dict.Add(category, translations);
            }

            return dict;
        }

        private static Dictionary<string, int> GetLanguageNumberCorrespondence()
        {
            Newtonsoft.Json.Linq.JObject correspondence = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("LanguageNumberCorrespondence.json"));
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach(var child in correspondence.Children())
            {
                if (child.HasValues)
                {
                    string name = child.Path;
                    int value = Convert.ToInt32(child.First.ToObject<string>());
                    result.Add(name, value);
                }
            }
            return result;
        }

        public static Dictionary<string, string> GetLanguagesDictionary()
        {
            Newtonsoft.Json.Linq.JObject correspondence = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("LanguagesList.json"));
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var child in correspondence.Children())
            {
                if (child.HasValues)
                {
                    string languageIdentifier = child.Path;
                    string language = child.First.ToObject<string>();
                    result.Add(language, languageIdentifier);
                }
            }
            return result;
        }
    }
}
