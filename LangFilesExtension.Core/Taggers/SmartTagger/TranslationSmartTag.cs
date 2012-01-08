using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Language.Intellisense;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.SmartTagger
{
    public class TranslationSmartTag : SmartTag
    {
        public TranslationSmartTag(ReadOnlyCollection<SmartTagActionSet> actionSets) : base(SmartTagType.Factoid, actionSets)
        {
        }
    }
}
