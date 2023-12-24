using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    public class Tag
    {
        public enum TagType
        {
            a,
            span,
            another
        }
        public TagType type;
        public string text;
        public Tag(TagType type, string text)
        {
            this.type = type;
            this.text = text;
        }
    }
}
