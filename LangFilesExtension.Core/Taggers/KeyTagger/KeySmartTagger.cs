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
        private readonly ITextView _textView;
        private ITextSnapshotLine _currentLine;
        private LanguageInfo _translationKeys;

        public KeySmartTagger(ITextBuffer buffer, ITextView textView, ITranslationKeysProvider keysProvider)
        {
            _buffer = buffer;
            _textView = textView;
            _keysProvider = keysProvider;
            _keysProvider.KeysUpdated += KeysUpdated;
            _textView.Caret.PositionChanged += OnPositionChanged;
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

                if (_translationKeys.Count() == 0)
                {
                    yield break;
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
                            AddActionsForKey(actionList, translationKey);
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

        private void AddActionsForKey(List<ISmartTagAction> actionList, TranslationKeyInfo translationKey)
        {
            var actionTemplates = new List<string>
                                      {
                                          "{0}",
                                          "Translate(\"{0}\")",
                                          "LanguageManager.Instance.Translate(\"{0}\")"
                                      };

            foreach (var actionTemplate in actionTemplates)
            {
                ISmartTagAction action = new CopyTranslationKeyAction(String.Format(actionTemplate, translationKey.Key));
                actionList.Add(action);
            }

            var actionTemplatesWithTransform = new List<string>
                                                   {
                                                       "{0}",
                                                       "<%$ Resources: EPiServer, {0} %>"
                                                   };
            foreach (var actionTemplate in actionTemplatesWithTransform)
            {
                ISmartTagAction action =
                    new CopyTranslationKeyAction(String.Format(actionTemplate,
                                                               translationKey.Key.Replace("/", ".").Trim('.')));
                actionList.Add(action);
            }
        }

        private void OnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            var lineFromPosition =
                e.NewPosition.BufferPosition.Snapshot.GetLineFromPosition(e.NewPosition.BufferPosition);
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

        private LanguageInfo ParseCurrentFile()
        {
            string text = _textView.TextSnapshot.GetText();

            var languageInfo = new LanguageInfo();
            languageInfo.AddKeys(LanguageFilesParser.Instance.ReadTranslationKeys(text, _filePath));
            return languageInfo;
        }

        private void OnTextChanged(object sender, TextContentChangedEventArgs e)
        {
            _translationKeys = null;
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            EventHandler<SnapshotSpanEventArgs> handler = TagsChanged;
            if (handler != null) handler(this, e);
        }
    }
}