using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.KeyTagger
{
    public class KeySmartTag : SmartTag
    {
        public KeySmartTag(ReadOnlyCollection<SmartTagActionSet> actionSets) : base(SmartTagType.Factoid, actionSets)
        {

        }
    }
}
