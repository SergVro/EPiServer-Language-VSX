using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;

namespace EPiServer.Labs.LangFilesExtension.Core.Parser
{
    public class LanguageFilesParser
    {
        private const string KeysSeparator = "/";
        public const string LanguageFilesExtension = ".xml";
        private readonly LanguageInfo _translations;
        private EventHandler _dataUpdated;

        private static volatile LanguageFilesParser _instance;
        private static readonly object _lockObject = new object();

        public static LanguageFilesParser Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new LanguageFilesParser();
                        }
                    }
                }
                return _instance;
            }
        }

        public LanguageInfo Translations
        {
            get { return _translations; }
        }

        public event EventHandler DataUpdated
        {
            add { _dataUpdated += value; }
            remove { _dataUpdated -= value; }
        }

        private LanguageFilesParser()
        {
            _translations = new LanguageInfo();
        }

        public void UpdateData(DTE2 applicationObject)
        {
            var fileNames = GetLanguageFilesInSolution(applicationObject);

            var xmlFiles = fileNames.Where(f => f.EndsWith(LanguageFilesExtension)).ToList();

            _translations.Clear();

            foreach (var xmlFile in xmlFiles)
            {
                AddKeysFromFile(xmlFile);
            }

            RiseDataUpdatedEvent();
        }

        private IEnumerable<string> GetLanguageFilesInSolution(DTE2 applicationObject)
        {
            var fileNames = new List<String>();

            if (applicationObject == null)
            {
                Trace.TraceError("applicationObject (DTE) is null");
                return fileNames;
            }

            var solution = applicationObject.Solution;
            if (solution == null)
            {
                Trace.TraceError("Solution is null");
                return fileNames;
            }

            Projects projects = solution.Projects;
            if (projects == null)
            {
                Trace.TraceError("Solution.Projects is null");
                return fileNames;
            }
            Trace.TraceInformation("There is {0} projects in solution {1}", projects.Count, solution.FullName);

            foreach (Project project in projects)
            {
                GetFileNamesFromProject(fileNames, project);
            }
            return fileNames;
        }

        private void GetFileNamesFromProject(List<string> fileNames, Project project)
        {
            if (project == null)
            {
                return;
            }
            var projectName = project.Name;
            Trace.TraceInformation("Processing project {0}", projectName);
            if (project.ProjectItems == null)
            {
                return;
            }
            foreach (ProjectItem projectItem in project.ProjectItems)
            {
                GetFileNamesFromProjectItem(fileNames, projectItem);
            }
        }

        private void RiseDataUpdatedEvent()
        {
            var handler = _dataUpdated;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void AddKeysFromFile(string xmlFile)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Load(xmlFile, LoadOptions.SetLineInfo);
            }
            catch (FileNotFoundException e)
            {
                Trace.TraceError("File {0}, doesn't exists. Error: {1}", xmlFile, e);
                return;
            }
            catch (IOException e)
            {
                Trace.TraceError("Can't open file {0}. Error: {1}", xmlFile, e);
                return;
            }
            catch (XmlException e)
            {
                Trace.TraceError("Can't open file {0}. Error: {1}", xmlFile, e);
                return;
            }
            
            var keys = ReadTranslationKeys(doc, xmlFile);
            _translations.AddKeys(keys);
        }

        public IEnumerable<TranslationKeyInfo> ReadTranslationKeys(string text, string xmlFile)
        {
            var reader = new StringReader(text);
            XDocument doc;
            try
            {
                doc = XDocument.Load(reader, LoadOptions.SetLineInfo);

            }
            catch (XmlException e)
            {
                Trace.TraceError("Can't open file {0}. Error: {1}", xmlFile, e);
                yield break;
            }

            foreach (TranslationKeyInfo key in ReadTranslationKeys(doc, xmlFile))
            {
                yield return key;
            }
        }

        public IEnumerable<TranslationKeyInfo> ReadTranslationKeys(XDocument doc, string xmlFile)
        {
            var languages = doc.Element("languages");
            if (languages == null)
            {
                yield break;
            }

            foreach (var language in languages.Elements("language"))
            {
                var languageIdAttribute = language.Attribute("id");
                if (languageIdAttribute == null)
                {
                    continue;
                }
                var languageId = languageIdAttribute.Value;

                var nodes = language.Descendants().Where(el => !el.Descendants().Any());
                foreach (var xNode in nodes)
                {
                    var ancestors = xNode.Ancestors().ToList();
                    var ancestorsString = ancestors.Take(ancestors.Count - 2)
                        // Skip two last ancestors - languages and language nodes
                        .Aggregate("", (curr, el) => el.Name + KeysSeparator + curr);
                    var key = KeysSeparator + ancestorsString + xNode.Name;
                    var value = xNode.Value;

                    int lineNumber = ((IXmlLineInfo) xNode).HasLineInfo() ? ((IXmlLineInfo) xNode).LineNumber : -1;

                    var translationInfo = new TranslationKeyInfo
                                              {
                                                  Key = key,
                                                  Value = value,
                                                  FilePath = xmlFile,
                                                  LineNumber = lineNumber,
                                                  Language = languageId
                                              };

                    yield return translationInfo;
                 }
            }
        }

        private void GetFileNamesFromProjectItem(List<string> fileNames, ProjectItem projectItem)
        {
            if (projectItem == null)
            {
                return;
            }
            var count = projectItem.FileCount;
            for (short i = 1; i <= count; i++)
            {
                string fileName = projectItem.FileNames[i];
                if (fileName != null)
                {
                    fileNames.Add(fileName);
                }
            }
            
            if (projectItem.ProjectItems != null)
            {
                var itemsCount = projectItem.ProjectItems.Count;
                if (itemsCount > 0)
                {
                    foreach (ProjectItem item in projectItem.ProjectItems)
                    {
                        GetFileNamesFromProjectItem(fileNames, item);
                    }
                }
            }

            if (projectItem.SubProject != null)
            {
                var subProject = projectItem.SubProject;
                GetFileNamesFromProject(fileNames, subProject);
            }

        }

 
    }
}