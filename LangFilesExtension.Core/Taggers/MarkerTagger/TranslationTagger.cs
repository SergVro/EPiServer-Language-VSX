using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger
{
    public class TranslationTagger : ITagger<TranslationTag>
    {
        private readonly ICodeParser _parser;

        public TranslationTagger(ICodeParser parser)
        {
            _parser = parser;
            _parser.TokensChanged += TokensChangedEventHandler;
        }

        #region ITagger<TranslationTag> Members

        public IEnumerable<ITagSpan<TranslationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            IEnumerable<LanguageToken> tokens = _parser.GetTokens(spans);
            return tokens.Select(t => new TagSpan<TranslationTag>(t.Span, new TranslationTag(t.TranslationKeys)));
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion

        private void TokensChangedEventHandler(object sender, EventArgs e)
        {
            ITextSnapshot snapshot = _parser.Snapshot;
            var span = new SnapshotSpan(snapshot, new Span(0, snapshot.Length));
            OnTagsChanged(new SnapshotSpanEventArgs(span));
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            EventHandler<SnapshotSpanEventArgs> handler = TagsChanged;
            if (handler != null) handler(this, e);
        }
    }
}