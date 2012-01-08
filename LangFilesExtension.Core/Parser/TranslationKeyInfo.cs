namespace EPiServer.Labs.LangFilesExtension.Core.Parser
{
    public class TranslationKeyInfo
    {
        public string Value { get; set; }
        public string Key { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string Language { get; set; }
    }
}