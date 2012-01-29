using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers
{
    public class CodeParserFactory : ICodeParserFactory
    {
        private readonly IDictionary<ITextBuffer, ICodeParser> _parsers;
        private static readonly object _lockObject = new object();
        private static volatile ICodeParserFactory _instance;

        public static ICodeParserFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new CodeParserFactory();
                        }
                    }
                }
                return _instance;
            }
        }

        private CodeParserFactory()
        {
            _parsers = new Dictionary<ITextBuffer, ICodeParser>();
        }

        public ICodeParser GetCodeParser(ITextBuffer buffer, ITranslationKeysProvider keysProvider)
        {
            IDictionary<ITextBuffer, ICodeParser> parsers;
            lock (_lockObject)
            {
                if (!_parsers.ContainsKey(buffer))
                {
                    var newParser = new CodeParser(buffer, keysProvider);
                    _parsers.Add(buffer, newParser);
                }
                parsers = _parsers;
            }
            return parsers[buffer];
        }

        public void Reset()
        {
            lock (_lockObject)
            {
                _parsers.Clear(); 
            }
        }
    }
}