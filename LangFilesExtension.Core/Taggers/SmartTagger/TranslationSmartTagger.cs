using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.SmartTagger
{
    public class TranslationSmartTagger : ITagger<TranslationSmartTag>
    {
        private readonly ICodeParser _parser;
        private readonly IServiceProvider _serviceProvider;

        public TranslationSmartTagger(ICodeParser parser, IServiceProvider serviceProvider)
        {
            _parser = parser;
            _serviceProvider = serviceProvider;

            _parser.TokensChanged += TokensChangedEventHandler;
        }

        #region Implementation of ITagger<TranslationSmartTag>

        public IEnumerable<ITagSpan<TranslationSmartTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            IEnumerable<LanguageToken> tokens = _parser.GetTokens(spans);
            return tokens.Select(t => CreateTag(t.TranslationKeys, t.Span));
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion

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