﻿using System;
using System.ComponentModel.Composition;
using EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.SmartTagger
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("any")]
    [TagType(typeof(SmartTag))]
    public class TranslationSmartTaggerProvider : IViewTaggerProvider
    {
        [Import]
        internal IClassifierAggregatorService ClassifierAggregatorService { get; set; }

        [Import]
        internal IBufferTagAggregatorFactoryService AggService { get; set; }

        [Import]
        internal ITranslationKeysProvider KeysProvider { get; set; }

        [Import(typeof(SVsServiceProvider))]
        internal IServiceProvider ServiceProvider { get; set; }

        #region Implementation of IViewTaggerProvider

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            // If this view isn't editable, then there isn't a good reason to be showing these.
            if (!textView.Roles.Contains(PredefinedTextViewRoles.Editable) || !textView.Roles.Contains(PredefinedTextViewRoles.PrimaryDocument))
                return null;

            // Make sure we only tagging top buffer
            if (buffer != textView.TextBuffer)
                return null;

            var tagAggregator = AggService.CreateTagAggregator<TranslationTag>(buffer);
            return new TranslationSmartTagger(buffer, textView, KeysProvider, ServiceProvider, tagAggregator) as ITagger<T>;
        }

        #endregion
    }
}