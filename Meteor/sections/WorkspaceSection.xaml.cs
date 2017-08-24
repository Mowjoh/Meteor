using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Meteor.database;
using System.Xml;

namespace Meteor.sections
{
    public partial class WorkspaceSection
    {
        private int ActiveWorkspaceSlot { get; set; }
        private int SelectedWorkspaceId { get; set; }

        private MeteorDatabase meteorDatabase = new MeteorDatabase();

        public WorkspaceSection()
        {
            InitializeComponent();

            ReloadWorkspacesList();

            if (WorkspacesListBox.Items.Count > 0)
                try
                {
                    WorkspacesListBox.SelectedIndex = ActiveWorkspaceSlot - 2;
                }
                catch
                {
                    WorkspacesListBox.SelectedIndex = 0;
                }

            LoadWorkspaceStats();
            
        }

        //Listbox Actions
        private void workspace_selected(object sender, SelectionChangedEventArgs e)
        {
            var li = (ListBoxItem)WorkspacesListBox.SelectedItem;
            if (li != null)
                WorkspaceNameTextBox.Text = li.Content.ToString();

            ToggleButtons();

            LoadWorkspaceStats();
        }

        private void AddWorkspace(object sender, RoutedEventArgs e)
        {
            if (WorkspacesListBox.Items.Count < 9)
            {
                int slot = WorkspacesListBox.Items.Count + 1;
                meteorDatabase.AddWorkspace(DateTime.Now.ToLongDateString(), slot );
                ReloadWorkspacesList();
                WorkspacesListBox.SelectedIndex = slot;
                MeteorCode.Message("Workspace added");
                ((MainWindow) Application.Current.MainWindow).LoadWorkspaces();
            }
            else
            {
                 MeteorCode.Message("You cannot have more than 9 workspaces");
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
            }
        }

        //Information
        private void SetWorkspaceName()
        {
            var name = WorkspaceNameTextBox.Text;

            Workspace workspace = meteorDatabase.Workspaces.First(w => w.Id == SelectedWorkspaceId);
            workspace.name = WorkspaceNameTextBox.Text;

            var lbi = (ListBoxItem)WorkspacesListBox.SelectedItem;
            lbi.Content = name;

            ReloadWorkspacesList();
            TabItem item = (TabItem)((MainWindow)Application.Current.MainWindow).WorkspaceTabControl.Items[workspace.slot - 1];
            item.Header = name;
            meteorDatabase.SaveChanges();
            MeteorCode.Message("Changed workspace name to " + name);
        }

        private void SetWorkspaceNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetWorkspaceName();
            }
        }


        //Sm4sh Explorer
        private void BuildWorkspace(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).BuildWorker.Launch();
        }

        private void LaunchS4E(object sender, RoutedEventArgs e)
        {
            var path = meteorDatabase.Configurations.First(c => c.property == "smashExplorerExe").value;
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                WorkingDirectory = meteorDatabase.Configurations.First(c => c.property == "SmashExplorerPath").value + "/"
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

            if (WorkspacesListBox.Items.Count > 1)
            {
                Workspace workspace = meteorDatabase.Workspaces.First(w => w.slot == WorkspacesListBox.SelectedIndex + 1);
                int id = workspace.Id;
                meteorDatabase.Workspaces.Remove(workspace);
                meteorDatabase.SaveChanges();
                ReorderWorkspaces();
                ReloadWorkspacesList();
                MeteorCode.WriteToConsole("Workspace deleted", 0);

                //If you deleted the active workspace
                if (meteorDatabase.Configurations.First(c => c.property == "activeWorkspace").value ==
                    id.ToString())
                {
                    //Setting first slot as active
                    meteorDatabase.Configurations.First(c => c.property == "activeWorkspace").value = meteorDatabase
                        .Workspaces.First(w => w.slot == 1).Id.ToString();
                }

                ((MainWindow)Application.Current.MainWindow).LoadWorkspaces();
            }
            else
            {
                MeteorCode.WriteToConsole("You cannot delete your last workspace", 2);
            }
        }

        private void ClearWorkspace(object sender, RoutedEventArgs e)
        {
            Workspace workspace = meteorDatabase.Workspaces.First(w => w.slot == WorkspacesListBox.SelectedIndex + 1);
            meteorDatabase.ClearWorkspace(workspace.Id);
            MeteorCode.Message("Workspace cleared");
        }

        //Imports
        private void launch_import_msl(object sender, RoutedEventArgs e)
        {
        }

        //Loads
        public void ReloadWorkspacesList()
        {
            var selected = WorkspacesListBox.SelectedIndex;
            WorkspacesListBox.Items.Clear();
            foreach (Workspace workspace in meteorDatabase.Workspaces.Where(w => w.slot > 0))
            {
                var lbi = new ListBoxItem();

                var cm = new ContextMenu();
                var m1 = new MenuItem {Header = "sync skin list from active workspace"};
                m1.Click += (s1, e) => { SyncSkins(); };
                cm.Items.Add(m1);
                lbi.ContextMenu = cm;

                lbi.Content = workspace.name;
                WorkspacesListBox.Items.Add(lbi);
            }
            if (selected == -1) return;

            if (selected + 1 <= WorkspacesListBox.Items.Count)
                WorkspacesListBox.SelectedIndex = selected;
            else
                WorkspacesListBox.SelectedIndex = WorkspacesListBox.Items.Count - 1;
        }

        public void LoadWorkspaceStats()
        {
            Workspace workspace = meteorDatabase.Workspaces.First(w => w.slot == WorkspacesListBox.SelectedIndex + 1);
            
            SkinsValueLabel.Content = meteorDatabase.SkinLibraries.Count(sl => sl.Skin.skinLock == false && sl.workspace_id == workspace.Id && sl.character_id != 0);
            NameplatesValueLabel.Content = meteorDatabase.SkinLibraries.Count(sl => sl.nameplate_id != 0 && sl.character_id != 0);
            DateValueLabel.Content = workspace.date;
            BuildCountValueLabel.Content = workspace.buildcount;
            //MostSkinsValueLabel.Content = 
        }

        public void ToggleButtons()
        {
            Workspace workspace = meteorDatabase.Workspaces.First(w => w.slot == WorkspacesListBox.SelectedIndex + 1);
            SelectedWorkspaceId = workspace.Id;
            if (workspace.locked)
            {
                ClearWorkspaceButton.IsEnabled = false;
                DeleteWorkspaceButton.IsEnabled = false;
                WorkspaceNameTextBox.IsEnabled = false;
                S4ELaunchButton.IsEnabled = false;
                WorkspaceBuildButton.IsEnabled = false;
            }
            else
            {
                ClearWorkspaceButton.IsEnabled = true;
                DeleteWorkspaceButton.IsEnabled = true;
                WorkspaceNameTextBox.IsEnabled = true;
                int activeWorkspace = int.Parse(meteorDatabase.Configurations.First(c => c.property == "activeWorkspace").value);
                if (activeWorkspace == workspace.Id)
                {
                    S4ELaunchButton.IsEnabled = true;
                    WorkspaceBuildButton.IsEnabled = true;
                }
                else
                {
                    S4ELaunchButton.IsEnabled = false;
                    WorkspaceBuildButton.IsEnabled = false;
                }
            }
        }

        public void ReloadWorkspaceScreen()
        {
            meteorDatabase = new MeteorDatabase();
            ToggleButtons();
            LoadWorkspaceStats();
        }

        public void ReorderWorkspaces()
        {
            int count = 1;
            foreach (Workspace workspace in meteorDatabase.Workspaces.Where(w => w.slot > 0))
            {
                workspace.slot = count;
                count++;
            }
            meteorDatabase.SaveChanges();
        }

    }
}
