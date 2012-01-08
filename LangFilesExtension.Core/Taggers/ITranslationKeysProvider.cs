using System;
using EPiServer.Labs.LangFilesExtension.Core.Parser;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers
{
    public interface ITranslationKeysProvider
    {
        LanguageInfo GetKeys();
        event EventHandler KeysUpdated;
    }
}