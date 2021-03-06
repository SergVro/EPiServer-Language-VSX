﻿#region copyright

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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using EPiServer.Labs.LangFilesExtension.Core.Taggers;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace EPiServer.Labs.LangFilesExtension.Core.QuickInfo
{
    internal class LanguageQuickInfoSource : IQuickInfoSource
    {
        private readonly ITextBuffer _buffer;
        private readonly ICodeParser _parser;
        private readonly LanguageQuickInfoProvider _provider;

        private bool _isDisposed;

        public LanguageQuickInfoSource(LanguageQuickInfoProvider provider, ITextBuffer buffer, ICodeParser parser)
        {
            _provider = provider;
            _buffer = buffer;
            _parser = parser;
        }

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistryService { get; set; }

        #region IQuickInfoSource Members

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
        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent,
                                            out ITrackingSpan applicableToSpan)
        {
            // Map the trigger point down to our buffer.
            SnapshotPoint? triggerPoint = session.GetTriggerPoint(_buffer.CurrentSnapshot);
            applicableToSpan = null;
            if (!triggerPoint.HasValue)
            {
                return;
            }

            foreach (var token in _parser.GetTokens(new SnapshotSpan(triggerPoint.Value, triggerPoint.Value)))
            {
                applicableToSpan = token.Span;
                if (!quickInfoContent.Contains(token.TarnslationsString))
                {
                    quickInfoContent.Insert(0, token.TarnslationsString);
                }
            }
        }

        #endregion
    }
}