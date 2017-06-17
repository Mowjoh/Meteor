using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Meteor.database;
using System.Xml;
using Meteor.content;

namespace Meteor.sections
{
    public partial class Workspace
    {
        // ReSharper disable once AssignNullToNotNullAttribute
        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;
        private readonly db_handler _dbHandler;

        private int ActiveWorkspaceId { get; set; }
        private int ActiveWorkspaceSlot { get; set; }
        private int SelectedWorkspaceId { get; set; }

        private readonly builder _builder;

        public Workspace()
        {
            InitializeComponent();

            _dbHandler = new db_handler();

            ReloadWorkspacesList();

            if (WorkspacesListBox.Items.Count > 0)
                try
                {
                    ActiveWorkspaceSlot =
                        _dbHandler.get_workspace_slot(int.Parse(_dbHandler.get_property("workspace")));
                    WorkspacesListBox.SelectedIndex = ActiveWorkspaceSlot - 2;
                }
                catch
                {
                    WorkspacesListBox.SelectedIndex = 0;
                }

            LoadWorkspaceStats();
            
            //Instanciancing builder
            uichar_handler uichar = new uichar_handler();
            if (!File.Exists(uichar.filepath)) return;

            var region = int.Parse(_dbHandler.get_property("region"));
            var language = int.Parse(_dbHandler.get_property("language"));
            var workspace = int.Parse(_dbHandler.get_property("workspace"));

            _builder = new builder(workspace, region, language, _dbHandler, uichar);
        }

        //Listbox Actions
        private void workspace_selected(object sender, SelectionChangedEventArgs e)
        {
            var li = (ListBoxItem)WorkspacesListBox.SelectedItem;
            if (li != null)
                WorkspaceNameTextBox.Text = li.Content.ToString();

            if (_dbHandler.workspace_default(WorkspacesListBox.SelectedIndex + 2))
            {
                ClearWorkspaceButton.IsEnabled = false;
                DeleteWorkspaceButton.IsEnabled = false;
                WorkspaceNameTextBox.IsEnabled = false;
                WorkspaceActivateButton.Visibility = Visibility.Hidden;
                S4ELaunchButton.IsEnabled = false;
                WorkspaceBuildButton.IsEnabled = false;
            }
            else
            {
                ClearWorkspaceButton.IsEnabled = true;
                DeleteWorkspaceButton.IsEnabled = true;
                WorkspaceNameTextBox.IsEnabled = true;
                SelectedWorkspaceId = _dbHandler.get_workspace_id(WorkspacesListBox.SelectedIndex + 2);

                if (_dbHandler.get_property("workspace") == _dbHandler.get_workspace_id(WorkspacesListBox.SelectedIndex + 2).ToString())
                {
                    WorkspaceActivateButton.Visibility = Visibility.Hidden;
                    S4ELaunchButton.IsEnabled = true;
                    WorkspaceBuildButton.IsEnabled = true;
                }
                else
                {
                    WorkspaceActivateButton.Visibility = Visibility.Visible;
                    S4ELaunchButton.IsEnabled = false;
                    WorkspaceBuildButton.IsEnabled = false;
                }
            }


            LoadWorkspaceStats();
        }

        private void AddWorkspace(object sender, RoutedEventArgs e)
        {
            if (WorkspacesListBox.Items.Count < 9)
            {
                ((MainWindow)Application.Current.MainWindow).AddWorkspaceWorker.Launch();

                            MeteorCode.WriteToConsole("Added a new workspace", 0);
            }
            else
            {
                            MeteorCode.WriteToConsole("You cannot have more than 9 workspaces", 1);
            }
        }

        private void SyncSkins()
        {
            var selected = WorkspacesListBox.SelectedIndex + 2;
            if (selected == ActiveWorkspaceSlot)
            {
                            MeteorCode.WriteToConsole("It's the same workspace, you dummy", 1);
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).CopyWorkspaceWorker.Launch(SelectedWorkspaceId);
            }
        }

        //Information
        private void SetWorkspaceName()
        {
            var name = WorkspaceNameTextBox.Text;

            _dbHandler.set_workspace_name(name, SelectedWorkspaceId);

            if (WorkspacesListBox.SelectedIndex + 2 == ActiveWorkspaceSlot)
                ((MainWindow)Application.Current.MainWindow).ActiveWorkspaceTextBox.Text = name;

            var lbi = (ListBoxItem)WorkspacesListBox.SelectedItem;
            lbi.Content = name;

            ReloadWorkspacesList();

                        MeteorCode.WriteToConsole("Changed workspace name to " + name, 0);
        }

        private void SetWorkspaceNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetWorkspaceName();
            }
        }

        private void ActivateWorkspace(object sender, RoutedEventArgs e)
        {
            //Getting new ID and Slot
            ActiveWorkspaceId = _dbHandler.get_workspace_id(WorkspacesListBox.SelectedIndex + 2);
            ActiveWorkspaceSlot = _dbHandler.get_workspace_slot(WorkspacesListBox.SelectedIndex + 2);

            //Assigning values
            _dbHandler.set_property_value(ActiveWorkspaceId.ToString(), "workspace");
            WorkspaceNameTextBox.Text = _dbHandler.GetWorkspaceName(ActiveWorkspaceId);
            ((MainWindow)Application.Current.MainWindow).ActiveWorkspaceTextBox.Text = _dbHandler.GetWorkspaceName(ActiveWorkspaceId);

            //Changing control state
            WorkspaceActivateButton.Visibility = Visibility.Hidden;
            WorkspaceBuildButton.IsEnabled = true;

            //Checking S4E configuration
            if (_dbHandler.get_property("s4e_path").Length > 0)
            {
                S4ELaunchButton.IsEnabled = true;
                SetS4EWorkspacePath(ActiveWorkspaceId);
            }


                        MeteorCode.WriteToConsole("Changed selected workspace", 0);
        }

        //Sm4sh Explorer
        private void BuildWorkspace(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).BuildWorker.Launch(_builder);
        }

        private void LaunchS4E(object sender, RoutedEventArgs e)
        {
            var path = _dbHandler.get_property("s4e_exe");
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                WorkingDirectory = _dbHandler.get_property("s4e_path") + "/"
            };
            try
            {
                Process.Start(startInfo);
            }
            catch
            {
                            MeteorCode.WriteToConsole("Sm4sh Explorer couldn't be launched. Is it setup in config?", 2);
            }
        }

        //Workspace Actions
        private void DeleteWorkspace(object sender, RoutedEventArgs e)
        {
            _dbHandler.reorder_workspace();

            if (WorkspacesListBox.Items.Count > 1)
            {
                var id = _dbHandler.get_workspace_id(WorkspacesListBox.SelectedIndex + 2);
                _dbHandler.delete_workspace(id);
                _dbHandler.reorder_workspace();
                ReloadWorkspacesList();
                            MeteorCode.WriteToConsole("Workspace deleted", 0);


                if (_dbHandler.get_property("workspace") == ActiveWorkspaceId.ToString())
                    _dbHandler.set_property_value(_dbHandler.get_workspace_id(2).ToString(), "workspace");
            }
            else
            {
                            MeteorCode.WriteToConsole("You cannot delete your last workspace", 2);
            }
        }

        private void ClearWorkspace(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).ClearWorkspaceWorker.Launch(SelectedWorkspaceId);
        }

        //Imports
        private void launch_import_msl(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).MslWorkspaceWorker.Launch();
        }

        //Loads
        private void ReloadWorkspacesList()
        {
            var selected = WorkspacesListBox.SelectedIndex;
            WorkspacesListBox.Items.Clear();
            var workspaces = _dbHandler.get_workspaces();
            foreach (string s in workspaces)
            {
                var lbi = new ListBoxItem();

                var cm = new ContextMenu();
                var m1 = new MenuItem {Header = "sync skin list from active workspace"};
                m1.Click += (s1, e) => { SyncSkins(); };
                cm.Items.Add(m1);
                lbi.ContextMenu = cm;

                lbi.Content = s;
                WorkspacesListBox.Items.Add(lbi);
            }
            if (selected == -1) return;

            if (selected + 1 <= WorkspacesListBox.Items.Count)
                WorkspacesListBox.SelectedIndex = selected;
            else
                WorkspacesListBox.SelectedIndex = WorkspacesListBox.Items.Count - 1;
        }

        private void LoadWorkspaceStats()
        {
            var stats = _dbHandler.get_workspace_stats(WorkspacesListBox.SelectedIndex + 2);
            Skin_Count_Value_Label.Content = stats[0].ToString();
        }

        //S4E Interaction
        private void SetS4EWorkspacePath(int workspaceId)
        {
            //Loading local manifest
            var xml = new XmlDocument();
            var workspacePath = AppPath + "/workspaces/workspace_" + workspaceId + "/";
            if (!Directory.Exists(workspacePath + "/content/patch/"))
                Directory.CreateDirectory(workspacePath + "/content/patch/");
            if (File.Exists(_dbHandler.get_property("s4e_path") + "/sm4shmod.xml"))
            {
                xml.Load(_dbHandler.get_property("s4e_path") + "/sm4shmod.xml");
                var node = xml.SelectSingleNode("/Sm4shMod/ProjectWorkplaceFolder");
                if (node == null)
                {
                    var newnode = xml.CreateElement("ProjectWorkplaceFolder");
                    newnode.InnerText = workspacePath;
                    var root = xml.SelectSingleNode("/Sm4shMod");
                    root?.AppendChild(newnode);
                }
                else
                {
                    node.InnerText = workspacePath;
                }

                xml.Save(_dbHandler.get_property("s4e_path") + "/sm4shmod.xml");
            }
            else
            {
                            MeteorCode.WriteToConsole("Could not assign Sm4sh Explorer's workspace", 1);
            }
        }

    }
}
