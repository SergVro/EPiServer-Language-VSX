using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("any")]
    [TagType(typeof(TranslationTag))]
    internal class TranslationTaggerProvider : IViewTaggerProvider
    {
        [Import]
        internal IClassifierAggregatorService ClassifierAggregatorService { get; set; }

        [Import]
        internal ITranslationKeysProvider KeysProvider { get; set; }

        [Import]
        internal ICodeParserFactory ParserFactory { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            // If this view isn't editable, then there isn't a good reason to be showing these.
            if (!textView.Roles.Contains(PredefinedTextViewRoles.Editable) ||
                !textView.Roles.Contains(PredefinedTextViewRoles.PrimaryDocument))
                return null;

            // Make sure we only tagging top buffer
            if (buffer != textView.TextBuffer)
                return null;

            var parser = ParserFactory.GetCodeParser(buffer, KeysProvider);
            return new TranslationTagger(parser) as ITagger<T>;
        }
    }
}
