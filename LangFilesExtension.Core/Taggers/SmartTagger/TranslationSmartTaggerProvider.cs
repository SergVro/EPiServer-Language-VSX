using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.SmartTagger
{
    [Export(typeof (IViewTaggerProvider))]
    [ContentType("any")]
    [TagType(typeof (TranslationSmartTag))]
    public class TranslationSmartTaggerProvider : IViewTaggerProvider
    {
        [Import]
        internal ITranslationKeysProvider KeysProvider { get; set; }

        [Import(typeof (SVsServiceProvider))]
        internal IServiceProvider ServiceProvider { get; set; }

        #region Implementation of IViewTaggerProvider

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (!textView.Roles.Contains(PredefinedTextViewRoles.Editable) ||
                !textView.Roles.Contains(PredefinedTextViewRoles.PrimaryDocument))
                return null;

            if (buffer != textView.TextBuffer)
                return null;

            ICodeParser parser = CodeParserFactory.Instance.GetCodeParser(buffer, KeysProvider);
            return new TranslationSmartTagger(parser, buffer, ServiceProvider) as ITagger<T>;
        }

        #endregion
    }
}