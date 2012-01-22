using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers
{
    internal interface ICodeParserFactory
    {
        ICodeParser GetCodeParser(ITextBuffer buffer, ITranslationKeysProvider keysProvider);
    }
}
