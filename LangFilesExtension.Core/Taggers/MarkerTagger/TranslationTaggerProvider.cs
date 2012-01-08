using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger
{
    [Export(typeof (ITaggerProvider))]
    [ContentType("any")]
    [TagType(typeof (TranslationTag))]
    internal class TranslationTaggerProvider : ITaggerProvider
    {
        [Import]
        internal IClassifierAggregatorService ClassifierAggregatorService { get; set; }

        [Import]
        internal ITranslationKeysProvider KeysProvider { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            return new TranslationTagger(buffer, KeysProvider) as ITagger<T>;
        }
    }
}
