using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace EPiServer.Labs.LangFilesExtension.Core.QuickInfo
{
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("ToolTip QuickInfo Controller")]
    [ContentType("CSharp")]
    [ContentType("HTML")]
    internal class LanguageQuickInfoControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        internal IQuickInfoBroker QuickInfoBroker { get; set; }
        /// <summary>
        /// Attempts to create an IntelliSense controller for a specific text view opened in a specific context.
        /// </summary>
        /// <param name="textView">The text view for which a controller should be created.</param><param name="subjectBuffers">The set of text buffers with matching content types that are potentially visible in the view.</param>
        /// <returns>
        /// A valid IntelliSense controller, or null if none could be created.
        /// </returns>
        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            return new LanguageQuickInfoController(textView, subjectBuffers, this);
        }
    }
}