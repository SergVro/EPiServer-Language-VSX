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
using System.Collections.ObjectModel;
using System.Linq;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.SmartTagger
{
    public class TranslationSmartTagger : ITagger<TranslationSmartTag>, IDisposable
    {
        private readonly ITextBuffer _buffer;
        private readonly ICodeParser _parser;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITextView _textView;
        private SnapshotSpan _currentSpan;
        private LanguageToken _currentToken;
        private bool _isCurrentSpanActive;

        public TranslationSmartTagger(ICodeParser parser, ITextBuffer buffer, ITextView textView,
                                      IServiceProvider serviceProvider)
        {
            _parser = parser;
            _serviceProvider = serviceProvider;
            _buffer = buffer;
            _textView = textView;

            _parser.TokensChanged += TokensChangedEventHandler;
            _textView.Caret.PositionChanged += CaretPositionChanged;
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

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            // Clean smart-tag from previous position
            if (_isCurrentSpanActive)
            {
                _isCurrentSpanActive = false;
                var eventArgs = new SnapshotSpanEventArgs(_currentSpan);
                OnTagsChanged(eventArgs);
            }

            // Gettings smart-tags for the new position
            var positionSpan = new SnapshotSpan(e.NewPosition.BufferPosition, e.NewPosition.BufferPosition);
            _currentToken = _parser.GetTokens(positionSpan).FirstOrDefault();
            if (_currentToken != null)
            {
                _isCurrentSpanActive = true;
                _currentSpan = _currentToken.Span.GetSpan(_buffer.CurrentSnapshot);
                var eventArgs = new SnapshotSpanEventArgs(_currentSpan);
                OnTagsChanged(eventArgs);
            }
        }

        private TagSpan<TranslationSmartTag> CreateTag(IEnumerable<TranslationKeyInfo> tarnslations,
                                                       SnapshotSpan snapshotSpan)
        {
            var actionSetList = new List<SmartTagActionSet>();
            var actionList = new List<ISmartTagAction>();

            foreach (TranslationKeyInfo translationKeyInfo in tarnslations)
            {
                ISmartTagAction action = new OpenTranslationAction(translationKeyInfo, _serviceProvider);
                actionList.Add(action);
            }
            var actionSet = new SmartTagActionSet(actionList.AsReadOnly());
            actionSetList.Add(actionSet);
            var actionSets = new ReadOnlyCollection<SmartTagActionSet>(actionSetList);
            return new TagSpan<TranslationSmartTag>(snapshotSpan, new TranslationSmartTag(actionSets));
        }

        private void TokensChangedEventHandler(object sender, SnapshotSpanEventArgs e)
        {
            OnTagsChanged(e);
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            EventHandler<SnapshotSpanEventArgs> handler = TagsChanged;
            if (handler != null) handler(this, e);
        }

        #region Implementation of ITagger<TranslationSmartTag>

        public IEnumerable<ITagSpan<TranslationSmartTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            // Returns tags only if GetTags was called as a result of the caret postion change
            if (_isCurrentSpanActive && spans.FirstOrDefault() == _currentSpan)
            {
                yield return
                    CreateTag(_currentToken.TranslationKeys, _currentToken.Span.GetSpan(_buffer.CurrentSnapshot));
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion
    }
}