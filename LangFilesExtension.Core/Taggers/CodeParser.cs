using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using Microsoft.VisualStudio.Text;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers
{
    internal class CodeParser : ICodeParser
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

        public event EventHandler TokensChanged;

        public ITextSnapshot Snapshot
        {
            get { return _buffer.CurrentSnapshot; }
        }

        public IEnumerable<LanguageToken> GetTokens(SnapshotSpan span)
        {
            return GetTokensFromSnapshot(span.Snapshot).Where(t => t.Span.IntersectsWith(span));
        }

        public IEnumerable<LanguageToken> GetTokens(NormalizedSnapshotSpanCollection spans)
        {
            return spans.SelectMany(GetTokens);
        }

        #endregion

        private void KeysUpdated(object sender, EventArgs e)
        {
            _translationKeys = _keysProvider.GetKeys();
            UpdateTokens();
        }

        private void UpdateTokens()
        {
            _tokens = GetTokensInternal(_buffer.CurrentSnapshot);
            OnTokensChanged();
        }

        private void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            UpdateTokens();
        }

        private IEnumerable<LanguageToken> GetTokensFromSnapshot(ITextSnapshot snapshot)
        {
            if (_tokens == null)
            {
                _tokens = GetTokensInternal(_buffer.CurrentSnapshot);
            }
            if (_tokens.Any(t => t.Span.Snapshot != snapshot))
            {
                foreach (LanguageToken token in _tokens)
                {
                    token.Span = token.Span.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);
                }
            }
            return _tokens;
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
                    languageToken.Span = new SnapshotSpan(snapshot, startIndex + keyGroup.Index, keyGroup.Length);
                    languageToken.TarnslationsString = translations
                        .Aggregate("\n", (curr, tr) => string.Format("{0}{1}: {2}\n", curr,  tr.Language, tr.Value));
                    tokens.Add(languageToken);
                }
                match = match.NextMatch();
            }
            return tokens;
        }

        private void OnTokensChanged()
        {
            EventHandler handler = TokensChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }
}