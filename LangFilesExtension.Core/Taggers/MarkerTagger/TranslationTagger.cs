#region copyright

// COPYRIGHT (C) 2012 EPISERVER AB
// 
// THIS FILE IS PART OF Language files Visual Studio Extension for EPiServer.
// 
// Language files Visual Studio Extension for EPiServer IS FREE SOFTWARE: YOU CAN REDISTRIBUTE IT AND/OR MODIFY IT
// UNDER THE TERMS OF THE GNU LESSER GENERAL PUBLIC LICENSE VERSION v2.1 AS PUBLISHED BY THE FREE SOFTWARE
// FOUNDATION.
// 
// Language files Visual Studio Extension for EPiServer IS DISTRIBUTED IN THE HOPE THAT IT WILL BE USEFUL, BUT WITHOUT
// ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR FITNESS FOR A PARTICULAR
// PURPOSE. SEE THE GNU LESSER GENERAL PUBLIC LICENSE FOR MORE DETAILS.
// 
// YOU SHOULD HAVE RECEIVED A COPY OF THE GNU LESSER GENERAL PUBLIC LICENSE ALONG WITH 
// Language files Visual Studio Extension for EPiServer. IF NOT, SEE <HTTP://WWW.GNU.ORG/LICENSES/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger
{
    public class TranslationTagger : ITagger<TranslationTag>, IDisposable
    {
        private readonly ITextBuffer _buffer;
        private readonly ICodeParser _parser;

        public TranslationTagger(ICodeParser parser, ITextBuffer buffer)
        {
            _parser = parser;
            _buffer = buffer;
            _parser.TokensChanged += TokensChangedEventHandler;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_parser != null)
            {
                _parser.TokensChanged -= TokensChangedEventHandler;
            }
        }

        #endregion

        #region ITagger<TranslationTag> Members

        public IEnumerable<ITagSpan<TranslationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var tokens = _parser.GetTokens(spans);
            return
                tokens.Select(
                    t =>
                    new TagSpan<TranslationTag>(t.Span.GetSpan(_buffer.CurrentSnapshot),
                                                new TranslationTag(t.TranslationKeys)));
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
    }
}