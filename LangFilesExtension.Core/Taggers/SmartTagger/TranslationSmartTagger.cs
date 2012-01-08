using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.SmartTagger
{
    public class TranslationSmartTagger : ITagger<TranslationSmartTag>
    {
        private readonly ITranslationKeysProvider _keysProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITagAggregator<TranslationTag> _tagAggregator;
        private readonly ITextBuffer _buffer;
        private readonly ITextView _textView;

        public TranslationSmartTagger(ITextBuffer buffer, ITextView textView, ITranslationKeysProvider keysProvider, 
            IServiceProvider serviceProvider, ITagAggregator<TranslationTag> tagAggregator)
        {
            _buffer = buffer;
            _textView = textView;
            _keysProvider = keysProvider;
            _serviceProvider = serviceProvider;
            _tagAggregator = tagAggregator;

            _keysProvider.KeysUpdated += OnKeysUpdated;
            _buffer.ChangedLowPriority += OnTextChanged;
        }

        private void OnKeysUpdated(object sender, EventArgs e)
        {
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            var span = new SnapshotSpan(snapshot, new Span(0, snapshot.Length));
            OnTagsChanged(new SnapshotSpanEventArgs(span));
        }

        private void OnTextChanged(object sender, TextContentChangedEventArgs e)
        {
            foreach (ITextChange textChange in e.Changes)
            {
                var start = e.After.GetLineFromPosition(textChange.NewSpan.Start).Start;
                var end = e.After.GetLineFromPosition(textChange.NewSpan.End).End;
                var span = new SnapshotSpan(start, end);

                OnTagsChanged(new SnapshotSpanEventArgs(span));
            }
        }

        private TagSpan<TranslationSmartTag> CreateTag(IEnumerable<TranslationKeyInfo> tarnslations,
                                                                  SnapshotSpan snapshotSpan)
        {
            var actionSetList = new List<SmartTagActionSet>();
            var actionList = new List<ISmartTagAction>();

            foreach (var translationKeyInfo in tarnslations)
            {
                ISmartTagAction action = new OpenTranslationAction(translationKeyInfo, _serviceProvider);
                actionList.Add(action);
            }
            var actionSet = new SmartTagActionSet(actionList.AsReadOnly());
            actionSetList.Add(actionSet);
            var actionSets = new ReadOnlyCollection<SmartTagActionSet>(actionSetList);
            return new TagSpan<TranslationSmartTag>(snapshotSpan, new TranslationSmartTag(actionSets));
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            EventHandler<SnapshotSpanEventArgs> handler = TagsChanged;
            if (handler != null) handler(this, e);
        }
    
        
        #region Implementation of ITagger<out TranslationSmartTag>

        public IEnumerable<ITagSpan<TranslationSmartTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var snapshotSpan in spans)
            {
                var translationTags = _tagAggregator.GetTags(snapshotSpan);
                foreach (var tagSpan in translationTags)
                {
                    foreach (var span in tagSpan.Span.GetSpans(snapshotSpan.Snapshot))
                    {
                        yield return CreateTag(tagSpan.Tag.Tarnslations, span);
                    }
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion


    }
}