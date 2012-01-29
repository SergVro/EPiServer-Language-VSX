using System.Collections.Generic;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using Microsoft.VisualStudio.Text;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers
{
    public class LanguageToken
    {
        public IList<TranslationKeyInfo> TranslationKeys { get; set; }
        public ITrackingSpan Span { get; set; }
        public string TarnslationsString { get; set; }
    }

}