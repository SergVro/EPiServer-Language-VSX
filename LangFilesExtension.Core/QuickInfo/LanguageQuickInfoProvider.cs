using System.ComponentModel.Composition;
using EPiServer.Labs.LangFilesExtension.Core.Taggers;
using EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace EPiServer.Labs.LangFilesExtension.Core.QuickInfo
{
    [Export(typeof(IQuickInfoSourceProvider))]
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

        /// <summary>
        /// Creates a Quick Info provider for the specified context.
        /// </summary>
        /// <param name="textBuffer">The text buffer for which to create a provider.</param>
        /// <returns>
        /// A valid <see cref="T:Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSource"/> instance, or null if none could be created.
        /// </returns>
        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new LanguageQuickInfoSource(this, textBuffer, AggService.CreateTagAggregator<TranslationTag>(textBuffer));
        }
    }
}