using System.Collections.Generic;
using System.Linq;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using Microsoft.VisualStudio.Text.Tagging;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger
{
    public class TranslationTag : TextMarkerTag // ITag 
    {
        public IList<TranslationKeyInfo> Tarnslations { get; set; }

        public TranslationTag(IEnumerable<TranslationKeyInfo> translations)
            : base("HighlightTranslation")
        {
            Tarnslations = translations.ToList();

        }
    }
}
