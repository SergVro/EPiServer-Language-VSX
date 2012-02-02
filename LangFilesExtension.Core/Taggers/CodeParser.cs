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
using System.Text.RegularExpressions;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using Microsoft.VisualStudio.Text;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers
{
    internal class CodeParser : ICodeParser, IDisposable
    {
        private readonly ITextBuffer _buffer;
        private readonly ITranslationKeysProvider _keysProvider;

        private readonly Regex _resourcesRegex =
            new Regex(@"(\<\%\$\s*Resources\s*:\s*EPiServer\s*,\s*(?<key>(\w+\W?)+)\%\>)|(""(?<key>(/\w+)+)"")",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private IList<LanguageToken> _tokens;
        private LanguageInfo _translationKeys;

        public CodeParser(ITextBuffer buffer, ITranslationKeysProvider keysProvider)
        {
            _buffer = buffer;
            _buffer.ChangedLowPriority += BufferChanged;

            _keysProvider = keysProvider;
            _keysProvider.KeysUpdated += KeysUpdated;

            _translationKeys = _keysProvider.GetKeys();
        }

        #region ICodeParser Members

        public event EventHandler<SnapshotSpanEventArgs> TokensChanged;

        public ITextSnapshot Snapshot
        {
            get { return _buffer.CurrentSnapshot; }
        }

        public IEnumerable<LanguageToken> GetTokens(SnapshotSpan span)
        {
            return GetTokens().Where(t => t.Span.GetSpan(span.Snapshot).IntersectsWith(span));
        }

        public IEnumerable<LanguageToken> GetTokens(NormalizedSnapshotSpanCollection spans)
        {
            return spans.SelectMany(GetTokens);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_buffer != null)
            {
                _buffer.ChangedLowPriority -= BufferChanged;
            }
            if (_keysProvider != null)
            {
                _keysProvider.KeysUpdated -= KeysUpdated;
            }
        }

        #endregion

        private IEnumerable<LanguageToken> GetTokens()
        {
            return _tokens ?? (_tokens = GetTokensInternal(_buffer.CurrentSnapshot));
        }

        private void KeysUpdated(object sender, EventArgs e)
        {
            _translationKeys = _keysProvider.GetKeys();
            UpdateTokens();

            var span = new SnapshotSpan(Snapshot, new Span(0, Snapshot.Length));
            OnTokensChanged(span);
        }

        private void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            if (!e.Changes.Any())
            {
                return;
            }
            ITextSnapshot textSnapshot = e.After;

            if (e.Changes.Any(c => _tokens.Any(t => t.Span.GetSpan(e.After).IntersectsWith(c.OldSpan)
                                                    || t.Span.GetSpan(e.After).IntersectsWith(c.NewSpan))))
            {
                UpdateTokens();
            }
            else
            {
                var tokensCount = _tokens.Count;
                foreach (var change in e.Changes)
                {
                    var lineStart = textSnapshot.GetLineFromPosition(change.NewSpan.Start);
                    var lineEnd = textSnapshot.GetLineFromPosition(change.NewSpan.End);
                    var changedSpan = new SnapshotSpan(lineStart.Start, lineEnd.End);
                    AddTokensFromText(textSnapshot, _tokens, lineStart.Start.Position, changedSpan.GetText());
                }
                if (tokensCount != _tokens.Count)
                {
                    _tokens = GetTokens().ToList();
                    RemoveDupplicates(_tokens, textSnapshot);
                }
            }
            var changeslineStart = textSnapshot.GetLineFromPosition(e.Changes.First().NewSpan.Start);
            var changeslineEnd = textSnapshot.GetLineFromPosition(e.Changes.Last().NewSpan.End);
            var changesSpan = new SnapshotSpan(changeslineStart.Start, changeslineEnd.End);
            OnTokensChanged(changesSpan);
        }

        private void RemoveDupplicates(IList<LanguageToken> tokens, ITextSnapshot textSnapshot)
        {
            var tokensList =
                tokens.Select(
                    t => new {Token = t, Index = tokens.IndexOf(t), SnapShotSpan = t.Span.GetSpan(textSnapshot)}).ToList
                    ();
            var noDuplicate = tokensList.Where(t =>
                                               !tokensList.Any(
                                                   ti =>
                                                   ti.SnapShotSpan.IntersectsWith(t.SnapShotSpan) && ti.Index > t.Index));
            _tokens = noDuplicate.Select(t => t.Token).ToList();
        }

        private void UpdateTokens()
        {
            _tokens = GetTokensInternal(_buffer.CurrentSnapshot);
        }

        private IList<LanguageToken> GetTokensInternal(ITextSnapshot snapshot)
        {
            var tokens = new List<LanguageToken>();
            if (_translationKeys.Count() == 0) // do not parse anything if we don't have any translation keys
            {
                return tokens;
            }
            string spanText = snapshot.GetText();
            const int startIndex = 0;

            AddTokensFromText(snapshot, tokens, startIndex, spanText);

            return tokens;
        }

        private void AddTokensFromText(ITextSnapshot snapshot, IList<LanguageToken> tokens, int startIndex,
                                       string spanText)
        {
            Match match = _resourcesRegex.Match(spanText);
            while (match.Success)
            {
                Group keyGroup = match.Groups["key"];
                string key = keyGroup.Value;
                List<TranslationKeyInfo> translations = _translationKeys.GetTranslationsForKey(key).ToList();
                if (translations.Any())
                {
                    var languageToken = new LanguageToken();
                    languageToken.TranslationKeys = translations;
                    languageToken.Span = snapshot.CreateTrackingSpan(startIndex + keyGroup.Index, keyGroup.Length,
                                                                     SpanTrackingMode.EdgeExclusive);
                    languageToken.TarnslationsString = translations
                        .Aggregate("\n", (curr, tr) => string.Format("{0}{1}: {2}\n", curr, tr.Language, tr.Value));
                    tokens.Add(languageToken);
                }
                match = match.NextMatch();
            }
        }

        private void OnTokensChanged(SnapshotSpan span)
        {
            var eventArgs = new SnapshotSpanEventArgs(span);
            var handler = TokensChanged;
            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }
    }
}