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
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using EPiServer.Labs.LangFilesExtension.Core.Parser;
using EPiServer.Labs.LangFilesExtension.Core.Taggers;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace EPiServer.Labs.LangFilesExtension.VSPackage
{
    ///<summary>
    /// This is the class that implements the package exposed by this assembly. 
    /// The minimum requirement for a class to be considered a valid package for Visual Studio is to implement 
    /// the IVsPackage interface and register itself with the shell. This package uses the helper classes 
    /// defined inside the Managed Package Framework (MPF) to do it: it derives from the Package class that 
    /// provides the implementation of the IVsPackage interface and uses the registration attributes defined 
    /// in the framework to register itself and its components with the shell.
    ///</summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    //[ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
    [Guid(GuidList.guidVSPackagePkgString)]
    public sealed class VSPackagePackage : Package, IVsRunningDocTableEvents3
    {
        private IVsRunningDocumentTable _docTable;
        private uint _docTableCookie;
        private DTE _dte;
        private SolutionEvents _solutionEvents;

        /// <summary>
        /// Default constructor of the package. Inside this method you can place any initialization code 
        /// that does not require any Visual Studio service because at this point the package object is 
        /// created but not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public VSPackagePackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, 
        /// so this is the place where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof (IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                var menuCommandID = new CommandID(GuidList.guidVSPackageCmdSet,
                                                  (int) PkgCmdIDList.EPiServerLanguageExtensionOptions);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
            }

            _docTable = (IVsRunningDocumentTable) GetService(typeof (SVsRunningDocumentTable));
            _docTable.AdviseRunningDocTableEvents(this, out _docTableCookie);

            // Your package is also a service container
            // The DTE object contains most of the goodies you'll want to play with
            _dte = ((IServiceContainer) this).GetService(typeof (SDTE)) as DTE;

            _solutionEvents = _dte.Events.SolutionEvents;
            _solutionEvents.Opened += SolutionOpened;
        }

        private void SolutionOpened()
        {
            CodeParserFactory.Instance.Reset();
            UpdateTranslationData();
        }

        /// <summary>
        ///   This function is the callback used to execute a command when the a menu item is clicked. 
        /// See the Initialize method to see how the menu item is associated to this function using 
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            UpdateTranslationData();

            // Show a Message Box to prove we were here
            var uiShell = (IVsUIShell) GetService(typeof (SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                0,
                ref clsid,
                "EPiServer Language Extension",
                string.Format(CultureInfo.CurrentCulture, "Translations updated. {0} translations keys found.",
                              LanguageFilesParser.Instance.Translations.Count()),
                string.Empty,
                0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_INFO,
                0, // false
                out result));
        }

        private void UpdateTranslationData()
        {
            var applicationObject = (DTE2) GetGlobalService(typeof (SDTE));
            LanguageFilesParser.Instance.UpdateData(applicationObject);
        }

        #region Implementation of IVsRunningDocTableEvents3

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
                                            uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
                                              uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            if (_dte == null || _dte.ActiveDocument == null)
            {
                return VSConstants.S_OK;
            }
            var doc = _dte.ActiveDocument.FullName;
            if (doc.EndsWith(LanguageFilesParser.LanguageFilesExtension, StringComparison.OrdinalIgnoreCase))
            {
                UpdateTranslationData();
            }
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld,
                                            string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew,
                                            string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}