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
using System.Text;

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

                //////////

                menuCommandID = new CommandID(GuidList.guidVSPackage1CmdSet, (int)PkgCmdIDList.cmdidMove5LinesUpSelect);
                menuItem = new MenuCommand(MenuItem_MoveLinesUpWithSelect, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidVSPackage1CmdSet, (int)PkgCmdIDList.cmdidMove5LinesDownSelect);
                menuItem = new MenuCommand(MenuItem_MoveLinesDownWithSelect, menuCommandID);
                mcs.AddCommand(menuItem);

                ///////////

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

        private void MenuItem_MoveLinesUpWithSelect(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE;
            if (dte != null)
            {
                var doc = dte.ActiveDocument;
                if (doc != null)
                {
                    var txt = doc.Object() as TextDocument;

                    txt.Selection.LineUp(true, 5);
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

        private void MenuItem_MoveLinesDownWithSelect(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE;
            if (dte != null)
            {
                var doc = dte.ActiveDocument;
                if (doc != null)
                {
                    var txt = doc.Object() as TextDocument;

                    txt.Selection.LineDown(true, 5);
                }
            }
        }

        private void AddEverything(string project_path, Regex regex, IList<FileListObject> obj, ProjectItem item, int level = 0)
        {
            if (project_path == null ||
                item == null ||
                item.Name == null ||
                regex == null ||
                obj == null)
                return;

            try
            {
                string pad = "".PadLeft(level * 4);

                if (regex.IsMatch(item.Name))
                {
                    string fullpath = "";
                    string fullpath_unchanged = "";
                    try
                    {
                        fullpath = Path.GetDirectoryName(fullpath_unchanged = item.Properties.Item("FullPath").Value.ToString());
                    }
                    catch
                    {
                        fullpath = "";
                        fullpath_unchanged = "";
                    }

                    // safety check
                    if (fullpath == null ||
                        fullpath_unchanged == null)
                    {
                        MessageBox.Show("[FATAL ERROR] fullpath or fullpath_unchanged are null and this should never happen");
                        return;
                    }

                    Logger.Log(LogType.Debug, "{0}=> {1}", pad, fullpath_unchanged);

                    if (Path.IsPathRooted(fullpath) &&
                        fullpath.Length > 0 &&
                        !string.IsNullOrEmpty(project_path))
                    {
                        var temp = project_path;
                        int backtrace_level = 0;

                        while (temp.Length > 3)
                        {
                            if (fullpath.StartsWith(temp, StringComparison.OrdinalIgnoreCase)) // incase in same directory as the *.vcxproj)
                            {
                                fullpath = fullpath.Remove(0, temp.Length);
                                while (fullpath.StartsWith("\\"))
                                    fullpath = fullpath.Remove(0, 1);

                                for (int i = 0; i < backtrace_level; ++i)
                                {
                                    fullpath = Path.Combine("..", fullpath);
                                }

                                break;
                            }

                            try
                            {
                                temp = Path.GetDirectoryName(temp); // strip away another directory
                            }
                            catch
                            {
                                break;
                            }

                            ++backtrace_level;
                        }
                    }

                    if (fullpath.Length == 0)
                        fullpath = ".";

                    obj.Add(new FileListObject(item.Name, fullpath, item));
                }

                foreach (var piobj in item.ProjectItems)
                {
                    var sub_item = piobj as ProjectItem;
                    if (sub_item != null)
                        AddEverything(project_path, regex, obj, sub_item, level + 1);
                }
            }
            catch (Exception ex)
            {
                string param = string.Format(
                    "project_path: '{0}', regex: '{1}', obj: '{2}', item: '{3}', level: '{4}'",
                    project_path,
                    regex,
                    obj,
                    item,
                    level);

                Logger.Log(
                    LogType.Error,
                    "VSPackage1Package.AddEverything -- Exception: {0}\r\nParams: {1}\r\n{2}",
                    ex.Message,
                    param,
                    ex.StackTrace);
            }
        }

        private void MenuItem_OpenSolutionFile(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE;
            if (dte != null)
            {
                var files = new List<FileListObject>();

                var regex = new Regex(@"^(\w|\s|\.)+\.\w+$", RegexOptions.IgnoreCase);

                try
                {
                    foreach (var prjobj in dte.Solution.Projects)
                    {
                        var project = prjobj as Project;
                        if (project != null)
                        {
                            string fullpath = "";

                            try
                            {
                                fullpath = Path.GetDirectoryName(project.FullName);
                                Logger.Log(LogType.Debug, "Project: {0}", project.FullName);
                            }
                            catch
                            {
                                Logger.Log(LogType.Debug, "Project FullName empty: {0}", project.Name);
                            }

                            foreach (var piobj in project.ProjectItems)
                            {
                                var item = piobj as ProjectItem;
                                if (item != null)
                                    AddEverything(fullpath, regex, files, item);
                            }
                        }
                    }

                    files.Sort((a, b) => a.Path.CompareTo(b.Path));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Exception: " + ex.Message + "\n" + ex.StackTrace,
                        "Exception",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

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
                        //dlg.SelectedListObject.Item.ExpandView(); // expands the shit inside the file (function names etc) in the solution explorer
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
