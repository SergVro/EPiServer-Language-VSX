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

using System.ComponentModel.Composition;
using EPiServer.Labs.LangFilesExtension.Core.Taggers;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace EPiServer.Labs.LangFilesExtension.Core.QuickInfo
{
    [Export(typeof (IQuickInfoSourceProvider))]
    [Name("ToolTip QuickInfo Source")]
    [Order(Before = "Default Quick Info Presenter")]
    [ContentType("text")]
    internal class LanguageQuickInfoProvider : IQuickInfoSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        [Import]
        internal ITranslationKeysProvider KeysProvider { get; set; }

        [Import]
        internal IBufferTagAggregatorFactoryService AggService { get; set; }

        #region IQuickInfoSourceProvider Members

        /// <summary>
        /// Creates a Quick Info provider for the specified context.
        /// </summary>
        /// <param name="buffer">The text buffer for which to create a provider.</param>
        /// <returns>
        /// A valid <see cref="T:Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSource"/> instance, or null if none could be created.
        /// </returns>
        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer buffer)
        {
            var parser = CodeParserFactory.Instance.GetCodeParser(buffer, KeysProvider);
            return new LanguageQuickInfoSource(this, buffer, parser);
        }

        #endregion
    }
}