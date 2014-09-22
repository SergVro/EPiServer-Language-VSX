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