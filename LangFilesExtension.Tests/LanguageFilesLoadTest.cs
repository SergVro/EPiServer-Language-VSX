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

using System.Diagnostics;
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

                Debug.WriteLine("Language " + languageId);

                var nodes = language.Descendants().Where(el => el.Descendants().Count() == 0);
                foreach (var xNode in nodes)
                {
                    var ancestors = xNode.Ancestors().ToList();
                    var ancestorsString = ancestors.Take(ancestors.Count - 2).Aggregate("",
                                                                                        (curr, el) =>
                                                                                        el.Name + "/" + curr);
                    var key = "/" + ancestorsString + xNode.Name;
                    var value = xNode.Value;
                    Debug.WriteLine(key + " =  " + value);
                }
            }
        }
    }
}