using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace EPiServer.Labs.LangFilesExtension.Core.Taggers.MarkerTagger
{
    [Export(typeof (EditorFormatDefinition))]
    [Name("HighlightTranslation")]
    [UserVisible(true)]
    internal class TranslationFormatDefinition : MarkerFormatDefinition
    {
        public TranslationFormatDefinition()
        {
            Border = new Pen(new SolidColorBrush(Colors.Orange), 0.5);
            Fill = new SolidColorBrush(Colors.White) {Opacity = 0};
            DisplayName = "Highlight Translation";
            ZOrder = 5;
        }
    }
}