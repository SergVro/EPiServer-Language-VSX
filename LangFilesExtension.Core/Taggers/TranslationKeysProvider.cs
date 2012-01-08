using System;
using System.ComponentModel.Composition;
using EPiServer.Labs.LangFilesExtension.Core.Parser;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers
{
    [Export(typeof(ITranslationKeysProvider))]
    public class TranslationKeysProvider : ITranslationKeysProvider, IDisposable
    {
        private bool _disposed;

        [ImportingConstructor]
        public TranslationKeysProvider()
        {
            LanguageFilesParser.Instance.DataUpdated += DataUpdated;
        }

        void DataUpdated(object sender, EventArgs e)
        {
            var handler = KeysUpdated;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public LanguageInfo GetKeys()
        {
            return LanguageFilesParser.Instance.Translations;
        }

        public event EventHandler KeysUpdated;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    LanguageFilesParser.Instance.DataUpdated -= DataUpdated;
                }
                _disposed = true;
            }
        }
    }


}