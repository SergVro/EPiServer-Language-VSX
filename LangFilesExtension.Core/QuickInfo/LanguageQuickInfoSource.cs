using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace EPiServer.Labs.LangFilesExtension.Core.QuickInfo
{
    class LanguageQuickInfoSource : IQuickInfoSource
    {
        private readonly LanguageQuickInfoProvider _provider;
        private readonly ITextBuffer _buffer;
        private readonly ITagAggregator<TranslationTag> _aggregator;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistryService { get; set; }


        public LanguageQuickInfoSource(LanguageQuickInfoProvider provider, ITextBuffer buffer, ITagAggregator<TranslationTag> aggregator)
        {
            _provider = provider;
            _buffer = buffer;
            _aggregator = aggregator;
        }

        private bool _isDisposed;
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Determines which pieces of QuickInfo content should be part of the specified <see cref="T:Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSession"/>.
        /// </summary>
        /// <param name="session">The session for which completions are to be computed.</param>
        /// <param name="quickInfoContent">The QuickInfo content to be added to the session.</param>
        /// <param name="applicableToSpan">The <see cref="T:Microsoft.VisualStudio.Text.ITrackingSpan"/> to which this session applies.</param>
        /// <remarks>
        /// Each applicable <see cref="M:Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSource.AugmentQuickInfoSession(Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSession,System.Collections.Generic.IList{System.Object},Microsoft.VisualStudio.Text.ITrackingSpan@)"/> instance will be called in-order to (re)calculate a
        ///             <see cref="T:Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSession"/>. Objects can be added to the session by adding them to the quickInfoContent collection
        ///             passed-in as a parameter.  In addition, by removing items from the collection, a source may filter content provided by
        ///             <see cref="T:Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSource"/>s earlier in the calculation chain.
        /// </remarks>
        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {

            // Map the trigger point down to our buffer.
            SnapshotPoint? triggerPoint = session.GetTriggerPoint(_buffer.CurrentSnapshot);
            applicableToSpan = null;
            if (!triggerPoint.HasValue)
            {
                return;
            }

            foreach (IMappingTagSpan<TranslationTag> curTag in _aggregator.GetTags(new SnapshotSpan(triggerPoint.Value, triggerPoint.Value)))
            {
                var tagSpan = curTag.Span.GetSpans(_buffer).First();
                applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(tagSpan, SpanTrackingMode.EdgeExclusive);
                if (!quickInfoContent.Contains(curTag.Tag.TarnslationsString))
                {
                    quickInfoContent.Insert(0, curTag.Tag.TarnslationsString);
                }
            }
        }
    }
}
