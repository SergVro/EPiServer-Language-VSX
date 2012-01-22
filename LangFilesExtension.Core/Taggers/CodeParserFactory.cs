using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers
{
    [Export(typeof(ICodeParserFactory))]
    internal class CodeParserFactory : ICodeParserFactory
    {
        private static readonly IDictionary<ITextBuffer, ICodeParser> _parsers =  new Dictionary<ITextBuffer, ICodeParser>();
        private static readonly object _lockObject = new object();

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
    }
}