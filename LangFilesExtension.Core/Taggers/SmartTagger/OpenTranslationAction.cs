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
using System.Collections.ObjectModel;
using System.Windows.Media;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.SmartTagger
{
    public class OpenTranslationAction : ISmartTagAction
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TranslationKeyInfo _translationKeyInfo;

        public OpenTranslationAction(TranslationKeyInfo translationKeyInfo, IServiceProvider serviceProvider)
        {
            _translationKeyInfo = translationKeyInfo;
            _serviceProvider = serviceProvider;
            DisplayText = string.Format("{0}: {1} line {2}", _translationKeyInfo.Language, _translationKeyInfo.FilePath, _translationKeyInfo.LineNumber);
            IsEnabled = true;
        }

        #region Implementation of ISmartTagAction

        public void Invoke()
        {
            var dte = _serviceProvider.GetService(typeof (SDTE)) as DTE;
            var itemOperations = dte.ItemOperations;
            var window = itemOperations.OpenFile(_translationKeyInfo.FilePath, "{7651A703-06E5-11D1-8EBD-00A0C90F26EA}");
            var selection = (TextSelection) window.Selection;
            selection.GotoLine(_translationKeyInfo.LineNumber);
        }

        public ReadOnlyCollection<SmartTagActionSet> ActionSets { get; set; }

        public ImageSource Icon { get; set; }

        public string DisplayText { get; set; }

        public bool IsEnabled { get; set; }

        #endregion
    }
}