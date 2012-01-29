using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger
{
    public class TranslationTagger : ITagger<TranslationTag>, IDisposable
    {
        private readonly ICodeParser _parser;
        private readonly ITextBuffer _buffer;

        public TranslationTagger(ICodeParser parser, ITextBuffer buffer)
        {
            _parser = parser;
            _buffer = buffer;
            _parser.TokensChanged += TokensChangedEventHandler;
        }

        #region ITagger<TranslationTag> Members

        public IEnumerable<ITagSpan<TranslationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var tokens = _parser.GetTokens(spans);
            return tokens.Select(t => new TagSpan<TranslationTag>(t.Span.GetSpan(_buffer.CurrentSnapshot), new TranslationTag(t.TranslationKeys)));
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion

        private void TokensChangedEventHandler(object sender, SnapshotSpanEventArgs e)
        {
            OnTagsChanged(e);
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            EventHandler<SnapshotSpanEventArgs> handler = TagsChanged;
            if (handler != null) handler(this, e);
        }

        public void Dispose()
        {
            if (_parser != null)
            {
                _parser.TokensChanged -= TokensChangedEventHandler;
            }
        }
    }
}