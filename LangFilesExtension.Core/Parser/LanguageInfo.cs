﻿#region copyright

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
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Labs.LangFilesExtension.Core.Parser

{
    public class LanguageInfo
    {
        private readonly List<TranslationKeyInfo> _translations;
        private ILookup<String, TranslationKeyInfo> _languageFilesLookup;
        private ILookup<String, TranslationKeyInfo> _translationKeysLookup;

        public LanguageInfo()
        {
            _translations = new List<TranslationKeyInfo>();
        }

        private ILookup<String, TranslationKeyInfo> KeysLookup
        {
            get
            {
                return _translationKeysLookup ??
                       (_translationKeysLookup = _translations.ToLookup(t => t.Key, StringComparer.OrdinalIgnoreCase));
            }
        }

        private ILookup<String, TranslationKeyInfo> LanguageFilesLookup
        {
            get
            {
                return _languageFilesLookup ??
                       (_languageFilesLookup =
                        _translations.ToLookup(CreateFileKey, StringComparer.OrdinalIgnoreCase));
            }
        }

        public IEnumerable<TranslationKeyInfo> GetTranslationsForKey(string key)
        {
            var searchKey = key.Trim().TrimStart('/');
            searchKey = searchKey.Replace(".", "/");
            searchKey = string.Format("/{0}", searchKey);
            var translationKeyInfos = KeysLookup[searchKey];
            return translationKeyInfos;
        }

        public IEnumerable<TranslationKeyInfo> GetKeysForPosition(string filePath, int lineNumber, string keyEnding)
        {
            var filePositionKey = GetFilePositionKey(filePath, lineNumber, keyEnding);
            return LanguageFilesLookup[filePositionKey];
        }

        public int Count()
        {
            return KeysLookup.Count;
        }

        public void Clear()
        {
            _translations.Clear();
            ResetLookups();
        }

        public void AddKey(string key, string language, string value, string filePath, int lineNumber)
        {
            var translationInfo = new TranslationKeyInfo
                                      {
                                          Key = key,
                                          Value = value,
                                          FilePath = filePath,
                                          LineNumber = lineNumber,
                                          Language = language
                                      };

            _translations.Add(translationInfo);

            ResetLookups();
        }

        public void AddKey(TranslationKeyInfo keyInfo)
        {
            _translations.Add(keyInfo);
            ResetLookups();
        }

        public void AddKeys(IEnumerable<TranslationKeyInfo> keys)
        {
            _translations.AddRange(keys);
            ResetLookups();
        }

        private void ResetLookups()
        {
            _translationKeysLookup = null;
            _languageFilesLookup = null;
        }

        private string CreateFileKey(TranslationKeyInfo keyInfo)
        {
            var keyEnding = keyInfo.Key.Split('/').LastOrDefault();
            return GetFilePositionKey(keyInfo.FilePath, keyInfo.LineNumber, keyEnding);
        }

        private static string GetFilePositionKey(string filePath, int lineNumber, string keyEnding)
        {
            return String.Format("{0}#{1}#{2}", filePath.ToLower(), lineNumber, (keyEnding ?? String.Empty).ToLower());
        }
    }
}