using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using Company.VSPackage1.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using Company.VSPackage1.Classes;
using System.Windows;

namespace Company.VSPackage1
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidVSPackage1PkgString)]
    public sealed class VSPackage1Package : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public VSPackage1Package()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidVSPackage1CmdSet, (int)PkgCmdIDList.cmdidMyCommand);
                MenuCommand menuItem = new MenuCommand(MenuItem_MoveLinesUp, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidVSPackage1CmdSet, (int)PkgCmdIDList.cmdidMyCommand2);
                menuItem = new MenuCommand(MenuItem_MoveLinesDown, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidVSPackage1CmdSet, (int)PkgCmdIDList.cmdidOpenSolutionFile);
                menuItem = new MenuCommand(MenuItem_OpenSolutionFile, menuCommandID);
                mcs.AddCommand(menuItem);

                mcs.AddCommand(new MenuCommand(null, new CommandID(GuidList.guidVSPackage1CmdSet, (int)PkgCmdIDList.TopLevelMenu)));
            }
        }
        #endregion

        private void MenuItem_MoveLinesUp(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE;
            if (dte != null)
            {
                var doc = dte.ActiveDocument;
                if (doc != null)
                {
                    var txt = doc.Object() as TextDocument;

                    txt.Selection.LineUp(false, 5);
                }
            }
        }

        private void MenuItem_MoveLinesDown(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE;
            if (dte != null)
            {
                var doc = dte.ActiveDocument;
                if (doc != null)
                {
                    var txt = doc.Object() as TextDocument;

                    txt.Selection.LineDown(false, 5);
                }
            }
        }

        private void AddEverything(Regex regex, string path, IList<FileListObject> obj, string project_name, ProjectItem item, int level = 0)
        {
            string pad = "".PadLeft(level * 4);

            if (regex.IsMatch(item.Name))
                obj.Add(new FileListObject(path, item));

            foreach (var piobj in item.ProjectItems)
            {
                var sub_item = piobj as ProjectItem;
                var sub_path = Path.Combine(path, sub_item.Name);
                AddEverything(regex, sub_path, obj, project_name, sub_item, level + 1);
            }
        }

        private void MenuItem_OpenSolutionFile(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE;
            if (dte != null)
            {
                var files = new List<FileListObject>();

                var regex = new Regex(@"^(\w|\s|\.)+\.\w+$", RegexOptions.IgnoreCase);

                foreach (var prjobj in (Array)dte.ActiveSolutionProjects)
                {
                    var project = prjobj as Project;
                    if (project != null)
                    {
                        foreach (var piobj in project.ProjectItems)
                        {
                            var item = piobj as ProjectItem;

                            AddEverything(regex, "", files, project.Name, item);
                        }
                    }
                }

                files.Sort((a, b) => a.Path.CompareTo(b.Path));

                ///////////////////////

                var dlg = new OpenProjectFileForm(files);

                dlg.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

                // set the window in the center of Visual Studio
                //dlg.Top = dte.MainWindow.Top + dte.MainWindow.Height / 2 - dlg.Height / 2;
                //dlg.Left = dte.MainWindow.Left + dte.MainWindow.Width / 2 - dlg.Width / 2;
#if _DISABLED
                if (dte.ActiveWindow != null)
                {
                    // center it inside of the active source editor
                    dlg.Top = dte.ActiveWindow.Top + dte.ActiveWindow.Height / 2 - dlg.Height / 2;
                    dlg.Left = dte.ActiveWindow.Left + dte.ActiveWindow.Width / 2 - dlg.Width / 2;
                }
                else
                    dlg.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
#endif

                dlg.ShowDialog();

                if (dlg.SelectedListObject != null)
                {
                    try
                    {
                        dlg.SelectedListObject.Item.ExpandView();
                        var window = dlg.SelectedListObject.Item.Open();
                        window.Activate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Failed to open selected file.\n" + ex.Message,
                            "Open solution file",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
            }
        }
    }
}
