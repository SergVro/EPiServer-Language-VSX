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
        private readonly TranslationKeyInfo _translationKeyInfo;
        private readonly IServiceProvider _serviceProvider;

        public OpenTranslationAction(TranslationKeyInfo translationKeyInfo, IServiceProvider serviceProvider)
        {
            _translationKeyInfo = translationKeyInfo;
            _serviceProvider = serviceProvider;
            DisplayText = _translationKeyInfo.FilePath + " line " + _translationKeyInfo.LineNumber;
            IsEnabled = true;
        }

        #region Implementation of ISmartTagAction

        public void Invoke()
        {
            var dte = _serviceProvider.GetService(typeof(SDTE)) as DTE;
            var itemOperations = dte.ItemOperations;
            var window = itemOperations.OpenFile(_translationKeyInfo.FilePath, "{7651A703-06E5-11D1-8EBD-00A0C90F26EA}");
            var selection = (TextSelection)window.Selection;
            selection.GotoLine(_translationKeyInfo.LineNumber);
        }

        public ReadOnlyCollection<SmartTagActionSet> ActionSets { get; set; }

        public ImageSource Icon { get; set; }

        public string DisplayText { get; set; }

        public bool IsEnabled { get; set; }

        #endregion
    }
}