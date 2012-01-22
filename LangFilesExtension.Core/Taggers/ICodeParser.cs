using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers
{
    public interface ICodeParser
    {
        event EventHandler TokensChanged;
        ITextSnapshot Snapshot { get; }
        IEnumerable<LanguageToken> GetTokens(SnapshotSpan span);
        IEnumerable<LanguageToken> GetTokens(NormalizedSnapshotSpanCollection spans);
    }
}
