using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using Microsoft.VisualStudio.Language.Intellisense;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.KeyTagger
{
    public class CopyTranslationKeyAction : ISmartTagAction
    {
        private readonly string _key;
        private readonly IServiceProvider _serviceProvider;

        public CopyTranslationKeyAction(string key, IServiceProvider serviceProvider)
        {
            _key = key;
            _serviceProvider = serviceProvider;

            DisplayText = String.Format("Copy key {0}", _key);
            IsEnabled = true;
        }

        #region Implementation of ISmartTagAction

        public void Invoke()
        {
            System.Windows.Forms.Clipboard.SetText(_key);
        }

        public ReadOnlyCollection<SmartTagActionSet> ActionSets { get; private set; }
        public ImageSource Icon { get; private set; }
        public string DisplayText { get; private set; }
        public bool IsEnabled { get; private set; }

        #endregion
    }
}
