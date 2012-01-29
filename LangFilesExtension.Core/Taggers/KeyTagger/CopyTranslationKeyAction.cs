using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.KeyTagger
{
    public class CopyTranslationKeyAction : ISmartTagAction
    {
        private readonly string _key;

        public CopyTranslationKeyAction(string key)
        {
            _key = key;

            DisplayText = String.Format("Copy {0}", _key);
            IsEnabled = true;
        }

        #region Implementation of ISmartTagAction

        public void Invoke()
        {
            Clipboard.SetText(_key);
        }

        public ReadOnlyCollection<SmartTagActionSet> ActionSets { get; private set; }
        public ImageSource Icon { get; private set; }
        public string DisplayText { get; private set; }
        public bool IsEnabled { get; private set; }

        #endregion
    }
}