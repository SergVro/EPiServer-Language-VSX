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

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger
{
    [Export(typeof (IViewTaggerProvider))]
    [ContentType("any")]
    [TagType(typeof (TranslationTag))]
    internal class TranslationTaggerProvider : IViewTaggerProvider
    {
        [Import]
        internal IClassifierAggregatorService ClassifierAggregatorService { get; set; }

        [Import]
        internal ITranslationKeysProvider KeysProvider { get; set; }

        #region IViewTaggerProvider Members

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            // If this view isn't editable, then there isn't a good reason to be showing these.
            if (!textView.Roles.Contains(PredefinedTextViewRoles.Editable) ||
                !textView.Roles.Contains(PredefinedTextViewRoles.PrimaryDocument))
                return null;

            // Make sure we only tagging top buffer
            if (buffer != textView.TextBuffer)
                return null;

            var parser = CodeParserFactory.Instance.GetCodeParser(buffer, KeysProvider);
            return new TranslationTagger(parser, buffer) as ITagger<T>;
        }

        #endregion
    }
}