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

using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers
{
    public class CodeParserFactory : ICodeParserFactory
    {
        private static readonly object _lockObject = new object();
        private static volatile ICodeParserFactory _instance;
        private readonly IDictionary<ITextBuffer, ICodeParser> _parsers;

        private CodeParserFactory()
        {
            _parsers = new Dictionary<ITextBuffer, ICodeParser>();
        }

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

        #region ICodeParserFactory Members

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

        #endregion
    }
}