using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPiServer.Labs.LangFilesExtension.Test
{
    [TestClass]
    public class LanguageFilesLoadTest
    {
        [TestMethod]
        [DeploymentItem("AlloyTech_EN.xml")]
        public void ReadXmlTest()
        {
            XDocument doc = XDocument.Load("AlloyTech_EN.xml");
            var languages = doc.Element("languages");
            
            Assert.IsNotNull(languages);

            foreach (var language in languages.Elements("language"))
            {
                var languageId = language.Attribute("id");
            
                Assert.IsNotNull(languageId);

                System.Diagnostics.Debug.WriteLine("Language "+languageId);

                var nodes = language.Descendants().Where(el => el.Descendants().Count() == 0);
                foreach (var xNode in nodes)
                {
                    var ancestors = xNode.Ancestors().ToList();
                    var ancestorsString = ancestors.Take(ancestors.Count-2).Aggregate("", (curr, el) => el.Name + "/" + curr);
                    var key = "/" + ancestorsString + xNode.Name;
                    var value = xNode.Value;
                    System.Diagnostics.Debug.WriteLine(key + " =  " + value);
                }
                
            }
        }
    }
}
