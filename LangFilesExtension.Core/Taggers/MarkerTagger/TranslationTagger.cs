using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger
{
    public class TranslationTagger: ITagger<TranslationTag> 
    {
        private readonly ITextBuffer _buffer;
        private readonly ITranslationKeysProvider _keysProvider;
        private readonly LanguageInfo _translationKeys;
        private readonly Regex _resourcesRegex = new Regex(@"(\<\%\$\s*Resources\s*:\s*EPiServer\s*,\s*(?<key>(\w+\W?)+)\%\>)|(""(?<key>(/\w+)+)"")",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private IList<ITagSpan<TranslationTag>> _tags;

        private IList<ITagSpan<TranslationTag>> Tags
        {
            get
            {
                if (_tags == null || _tags.Any(t => t.Span.Snapshot != _buffer.CurrentSnapshot))
                {
                    _tags = GetUpdatedTags();
                }
                return _tags;
            }
        }

        public TranslationTagger(ITextBuffer buffer, ITranslationKeysProvider keysProvider)
        {
            _buffer = buffer;
            _keysProvider = keysProvider;

            _keysProvider.KeysUpdated += OnKeysUpdated;
            _buffer.ChangedLowPriority += OnTextChanged;

            _translationKeys = keysProvider.GetKeys();

            _tags = GetUpdatedTags();
        }

        private List<ITagSpan<TranslationTag>> GetUpdatedTags()
        {
            return GetTagsInternal(new NormalizedSnapshotSpanCollection(_buffer.CurrentSnapshot,
                                                             new[] {new Span(0, _buffer.CurrentSnapshot.Length)})).ToList();
        }

        private void OnKeysUpdated(object sender, EventArgs e)
        {
            _tags = null;
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            var span = new SnapshotSpan(snapshot, new Span(0, snapshot.Length));
            OnTagsChanged(new SnapshotSpanEventArgs(span));
        }

        private void OnTextChanged(object sender, TextContentChangedEventArgs e)
        {
            _tags = null;
        }

        public IEnumerable<ITagSpan<TranslationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            //if (!spans.IntersectsWith(new NormalizedSnapshotSpanCollection(Tags.Select(t=>t.Span.TranslateTo(_buffer.CurrentSnapshot, SpanTrackingMode.EdgeExclusive)))))
            //{
            //    yield break;
            //}

            foreach (var span in spans)
            {
                SnapshotSpan snapshotSpan = span;
                foreach (var tag in Tags.Where(t => t.Span.IntersectsWith(snapshotSpan)))
                {
                    yield return tag;
                }
            }
        }

        private IEnumerable<ITagSpan<TranslationTag>> GetTagsInternal(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var snapshotSpan in spans)
            {
                Debug.Assert(snapshotSpan.Snapshot.TextBuffer == _buffer);

                var containingLine = snapshotSpan.Start.GetContainingLine();

                string spanText;
                int startIndex = 0;
                if (snapshotSpan.Length > 0)
                {
                    spanText = snapshotSpan.GetText();
                    startIndex = snapshotSpan.Start.Position;
                }
                else
                {
                    spanText = containingLine.GetText();
                    startIndex = containingLine.Start.Position;
                }
                var match = _resourcesRegex.Match(spanText);
                while (match.Success)
                {
                    var keyGroup = match.Groups["key"];
                    var key = keyGroup.Value;
                    var translations = _translationKeys.GetTranslationsForKey(key).ToList();
                    if (translations.Any())
                    {
                        yield return
                            CreateTag(translations,
                                      new SnapshotSpan(snapshotSpan.Snapshot, startIndex + keyGroup.Index, keyGroup.Length));
                    }

                    match = match.NextMatch();
                }
            }
        }

        private TagSpan<TranslationTag> CreateTag(IEnumerable<TranslationKeyInfo> tarnslations, SnapshotSpan snapshotSpan)
        {
            return new TagSpan<TranslationTag>(snapshotSpan, new TranslationTag(tarnslations));
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            EventHandler<SnapshotSpanEventArgs> handler = TagsChanged;
            if (handler != null) handler(this, e);
        }
    }
}
