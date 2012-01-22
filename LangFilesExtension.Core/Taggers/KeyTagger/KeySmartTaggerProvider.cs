using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using EPiServer.Labs.LangFilesExtension.Core.Taggers.SmartTagger;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.KeyTagger
{
    [Export(typeof (IViewTaggerProvider))]
    [ContentType("xml")]
    [TagType(typeof (KeySmartTag))]
    public class KeySmartTaggerProvider : IViewTaggerProvider
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

            return new KeySmartTagger(buffer, textView, KeysProvider, ServiceProvider) as ITagger<T>;
        }

        #endregion
    }
}
