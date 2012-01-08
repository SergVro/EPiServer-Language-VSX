using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.KeyTagger
{
    public class KeySmartTagger : ITagger<KeySmartTag>
    {
        private readonly ITextBuffer _buffer;
        private readonly string _filePath;

        private readonly Regex _keyRegex = new Regex(@"\<(?<key>\w+)\>",
                                                     RegexOptions.Compiled | RegexOptions.IgnoreCase |
                                                     RegexOptions.Multiline);

        private readonly ITranslationKeysProvider _keysProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITextView _textView;
        private ITextSnapshotLine _currentLine;
        private LanguageInfo _translationKeys;

        public KeySmartTagger(ITextBuffer buffer, ITextView textView, ITranslationKeysProvider keysProvider,
                              IServiceProvider serviceProvider)
        {
            _buffer = buffer;
            _textView = textView;
            _keysProvider = keysProvider;
            _serviceProvider = serviceProvider;
            _keysProvider.KeysUpdated += KeysUpdated;
            _textView.Selection.SelectionChanged += OnSelectionChanged;
            _buffer.Changed += OnTextChanged;
            ITextDocument textDocument;
            if (buffer.Properties.TryGetProperty(typeof (ITextDocument), out textDocument))
            {
                _filePath = textDocument.FilePath;
            }
            _translationKeys = _keysProvider.GetKeys();
        }

        #region ITagger<KeySmartTag> Members

        public IEnumerable<ITagSpan<KeySmartTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan snapshotSpan in spans)
            {
                if (_currentLine == null || !snapshotSpan.Snapshot.Equals(_currentLine.Snapshot) ||
                    !snapshotSpan.OverlapsWith(_currentLine.Extent))
                {
                    continue;
                }
                if (_translationKeys == null)
                {
                    _translationKeys = ParseCurrentFile();
                }
                SnapshotSpan currentSpan = _currentLine.Extent;
                string text = currentSpan.GetText();

                Match match = _keyRegex.Match(text);
                while (match.Success)
                {
                    Group keyGroup = match.Groups["key"];
                    string key = keyGroup.Value;
                    int lineNumber =
                        currentSpan.Snapshot.GetLineNumberFromPosition(currentSpan.Start.Position + keyGroup.Index) + 1;
                    List<TranslationKeyInfo> translationKeys =
                        _translationKeys.GetKeysForPosition(_filePath, lineNumber, key).ToList();
                    if (translationKeys.Any())
                    {
                        var actionSetList = new List<SmartTagActionSet>();
                        var actionList = new List<ISmartTagAction>();

                        foreach (TranslationKeyInfo translationKey in translationKeys)
                        {
                            ISmartTagAction action = new CopyTranslationKeyAction(translationKey.Key, _serviceProvider);
                            actionList.Add(action);
                        }

                        if (actionList.Any())
                        {
                            var actionSet = new SmartTagActionSet(actionList.AsReadOnly());
                            actionSetList.Add(actionSet);
                            var actionSets = new ReadOnlyCollection<SmartTagActionSet>(actionSetList);

                            var tagSpan = new SnapshotSpan(currentSpan.Snapshot,
                                                           currentSpan.Start.Position + keyGroup.Index, keyGroup.Length);
                            yield return new TagSpan<KeySmartTag>(tagSpan, new KeySmartTag(actionSets));
                        }
                    }
                    match = match.NextMatch();
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            var selection = (ITextSelection) sender;
            ITextSnapshotLine lineFromPosition =
                selection.ActivePoint.Position.Snapshot.GetLineFromPosition(selection.ActivePoint.Position);
            TriggerTagsChangedForNewPosition(lineFromPosition);
        }

        private void TriggerTagsChangedForNewPosition(ITextSnapshotLine lineFromPosition)
        {
            if (_currentLine != null && lineFromPosition.Extent == _currentLine.Extent)
            {
                return;
            }
            ITextSnapshotLine oldLine = _currentLine;
            _currentLine = lineFromPosition;

            if (oldLine != null)
            {
                TriggerTagsChangedForLine(oldLine);
            }
            TriggerTagsChangedForLine(_currentLine);
        }

        private void TriggerTagsChangedForLine(ITextSnapshotLine lineFromPosition)
        {
            if (lineFromPosition == null)
            {
                throw new ArgumentNullException("lineFromPosition");
            }
            SnapshotPoint start = lineFromPosition.Start;
            SnapshotPoint end = lineFromPosition.End;
            var span = new SnapshotSpan(start, end);
            OnTagsChanged(new SnapshotSpanEventArgs(span));
        }

        private void KeysUpdated(object sender, EventArgs e)
        {
            _translationKeys = _keysProvider.GetKeys();
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            var span = new SnapshotSpan(snapshot, new Span(0, snapshot.Length));
            OnTagsChanged(new SnapshotSpanEventArgs(span));
        }

        private void OnTextChanged(object sender, TextContentChangedEventArgs e)
        {
            _translationKeys = null;
        }

        private LanguageInfo ParseCurrentFile()
        {
            string text = _textView.TextSnapshot.GetText();

            var languageInfo = new LanguageInfo();
            languageInfo.AddKeys(LanguageFilesParser.Instance.ReadTranslationKeys(text, _filePath));
            return languageInfo;
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            EventHandler<SnapshotSpanEventArgs> handler = TagsChanged;
            if (handler != null) handler(this, e);
        }
    }
}