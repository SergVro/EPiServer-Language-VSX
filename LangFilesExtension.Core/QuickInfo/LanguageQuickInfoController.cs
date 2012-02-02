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
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace EPiServer.Labs.LangFilesExtension.Core.QuickInfo
{
    internal class LanguageQuickInfoController : IIntellisenseController
    {
        private readonly LanguageQuickInfoControllerProvider _provider;
        private readonly IList<ITextBuffer> _subjectBuffers;
        private IQuickInfoSession _session;
        private ITextView _textView;

        public LanguageQuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers,
                                           LanguageQuickInfoControllerProvider provider)
        {
            _textView = textView;
            _subjectBuffers = subjectBuffers;
            _provider = provider;

            _textView.MouseHover += OnTextViewMouseHover;
        }

        #region IIntellisenseController Members

        /// <summary>
        /// Detaches the controller from the specified <see cref="T:Microsoft.VisualStudio.Text.Editor.ITextView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="T:Microsoft.VisualStudio.Text.Editor.ITextView"/> from which the controller should detach.</param>
        public void Detach(ITextView textView)
        {
            if (_textView == textView)
            {
                _textView.MouseHover -= OnTextViewMouseHover;
                _textView = null;
            }
        }

        /// <summary>
        /// Called when a new subject <see cref="T:Microsoft.VisualStudio.Text.ITextBuffer"/> appears in the graph of buffers associated with
        ///             the <see cref="T:Microsoft.VisualStudio.Text.Editor.ITextView"/>, due to a change in projection or content type.
        /// </summary>
        /// <param name="subjectBuffer">The newly-connected text buffer.</param>
        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        /// <summary>
        /// Called when a subject <see cref="T:Microsoft.VisualStudio.Text.ITextBuffer"/> is removed from the graph of buffers associated with
        ///             the <see cref="T:Microsoft.VisualStudio.Text.Editor.ITextView"/>, due to a change in projection or content type. 
        /// </summary>
        /// <param name="subjectBuffer">The disconnected text buffer.</param>
        /// <remarks>
        /// It is not guaranteed that
        ///             the subject buffer was previously connected to this controller.
        /// </remarks>
        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        #endregion

        private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            //find the mouse position by mapping down to the subject buffer
            SnapshotPoint? point = _textView.BufferGraph.MapDownToFirstMatch
                (new SnapshotPoint(_textView.TextSnapshot, e.Position),
                 PointTrackingMode.Positive,
                 snapshot => _subjectBuffers.Contains(snapshot.TextBuffer),
                 PositionAffinity.Predecessor);

            if (point != null)
            {
                ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
                                                                                       PointTrackingMode.Positive);

                if (!_provider.QuickInfoBroker.IsQuickInfoActive(_textView))
                {
                    _session = _provider.QuickInfoBroker.TriggerQuickInfo(_textView, triggerPoint, true);
                }
            }
        }
    }
}