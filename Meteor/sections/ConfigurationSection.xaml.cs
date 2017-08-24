using System;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Path = System.IO.Path;
using System.Xml;
using Microsoft.Win32;
using winforms = System.Windows.Forms;

namespace Meteor.sections
{
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public partial class ConfigurationSection
    {
        //Private variables
        private MeteorDatabase meteorDatabase;

        //Constructor
        public ConfigurationSection()
        {
            InitializeComponent();

            meteorDatabase = new MeteorDatabase();
            
            retreive_config();
        }

        //Makes the interface reflect the current configuration
        private void retreive_config()
        {
            //Build Options
            combo_region.SelectedIndex = int.Parse(meteorDatabase.Configurations.First(c => c.property == "region").value);
            combo_language.SelectedIndex = int.Parse(meteorDatabase.Configurations.First(c => c.property == "language").value);
            S4EPathTextbox.Text = meteorDatabase.Configurations.First(c => c.property == "SmashExplorerPath").value;

            //Advanced Options
            PasteBinCheckBox.IsChecked = meteorDatabase.Configurations.First(c => c.property == "pastebin").value == "1";
            LaunchModeOption.SelectedIndex = int.Parse(meteorDatabase.Configurations
                .First(c => c.property == "AutoSmashExplorerRelaunch").value);

            //UI Files Options
            ReloadConfigFilesList();

            //Resetting message
            MeteorCode.Message("");
        }

        //Build
        private void RegionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Getting region
            ComboBoxItem region = (ComboBoxItem) combo_region.SelectedItem;

            //Saving Database information
            meteorDatabase.Configurations.First(c => c.property == "region").value =
                combo_region.SelectedIndex.ToString();
            meteorDatabase.SaveChanges();


            MeteorCode.Message("Region changed to " + region.Content);
        }

        private void LaunguageChanged(object sender, SelectionChangedEventArgs e)
        {
            //Getting language
            ComboBoxItem language = (ComboBoxItem)combo_language.SelectedItem;

            //Saving Database information
            meteorDatabase.Configurations.First(c => c.property == "language").value = combo_language.SelectedIndex.ToString();
            meteorDatabase.SaveChanges();

            MeteorCode.Message("Language changed to " + language.Content);
        }

        private void S4EPathKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var exepath = S4EPathTextbox.Text;
            try
            {
                var directoryInfo = new FileInfo(exepath).Directory;
                if (directoryInfo != null)
                {
                    var path = directoryInfo.FullName;
                    var filetype = new FileInfo(exepath).Extension;

                    if (filetype == ".exe")
                        if (new FileInfo(exepath).Exists)
                        {
                            if (new FileInfo(exepath).Name != "Sm4shFileExplorer.exe")
                            meteorDatabase.Configurations.First(c => c.property == "smashExplorerExe").value =
                                exepath;
                            meteorDatabase.Configurations.First(c => c.property == "SmashExplorerPath").value =
                                path;

                            meteorDatabase.SaveChanges();
                            ChangeS4EWorkspaceLocation(int.Parse(meteorDatabase.Configurations.First(c => c.property == "activeWorkspace").value));
                            S4EPathTextbox.Text = exepath;
                        }
                    
                }
                
            }
            catch (Exception s4EpathException)
            {
                MeteorCode.Paste(s4EpathException.Message, s4EpathException.StackTrace);
            }
        }

        private void S4EPathSave(object sender, RoutedEventArgs e)
        {
            try
            {
                //Dialog
                var openFileDialog = new winforms.OpenFileDialog();
                openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "")
                {
                    //Getting the values
                    string exepath = openFileDialog.FileName;
                    var directoryInfo = new FileInfo(exepath).Directory;
                    if (directoryInfo != null)
                    {
                        string path = directoryInfo.FullName;
                        var filetype = new FileInfo(exepath).Extension;

                        if (filetype == ".exe")
                        {
                            if (new FileInfo(exepath).Name == "Sm4shFileExplorer.exe")
                            {
                                //Saving database information
                                Configuration exepathc = meteorDatabase.Configurations.First(c => c.property == "smashExplorerExe");
                                exepathc.value = exepath;
                                Configuration pathconfig =
                                    meteorDatabase.Configurations.First(c => c.property == "SmashExplorerPath");
                                pathconfig.value = path;
                                meteorDatabase.SaveChanges();

                                S4EPathTextbox.Text = openFileDialog.FileName;
                                //Changing S4E's workspace path to Meteor's current workspace
                                ChangeS4EWorkspaceLocation(int.Parse(meteorDatabase.Configurations
                                    .First(c => c.property == "activeWorkspace").value));

                                MeteorCode.Message("Sm4shFileExplorer path saved and associated with Meteor");
                            }
                            else
                            {
                                MeteorCode.Message("You did not select 'Sm4shFileExplorer.exe'");
                            }
                        }
                        else
                        {
                            MeteorCode.Message("The selected file is not an executable (.exe)");
                        }
                    }
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
            }
        }

        //UI Options
        private void PickUi(object sender, RoutedEventArgs e)
        {
            //Setting up OpenFileDialog
            var ofd = new winforms.OpenFileDialog();
            ofd.ShowDialog();
            var sourceFilePath = ofd.FileName;
            var filename = Path.GetFileName(sourceFilePath);

            ConfigurationFile file =  meteorDatabase.ConfigurationFiles.FirstOrDefault(cf => cf.name == filename);

            if (file != null)
            {
                //Checking destination directory
                if (!Directory.Exists(MeteorCode.AppPath + "/filebank/configuration/" + file.folder + "/"))
                {
                    Directory.CreateDirectory(MeteorCode.AppPath + "/filebank/configuration/" + file.folder + "/");
                }
                var destination = MeteorCode.AppPath + "/filebank/configuration/" + file.folder + "/" + file.name;

                File.Copy(sourceFilePath, destination, true);

                //Setting database status
                file.present = true;
                meteorDatabase.SaveChanges();

                ReloadConfigFilesList();

                MeteorCode.Message("Config file : '" + file.name + "' has been recognised and added to Meteor");
            }
            else
            {
                MeteorCode.Message("Config file : '" + filename + "' was not recognised by Meteor");
            }
        }

        private void ReloadConfigFilesList()
        {
            ConfigFileListBox.Items.Clear();
            foreach (ConfigurationFile file in meteorDatabase.ConfigurationFiles.Where(cf => cf.present == true))
            {
                ListBoxItem lbi = new ListBoxItem()
                {
                    Content = file.name
                };
                ConfigFileListBox.Items.Add(lbi);
            }
        }

        //Color Options
        private void SaveColor(object sender, RoutedEventArgs e)
        {
            var color = ColorPicker.SelectedColor.ToString();
            meteorDatabase.Configurations.First(c => c.property == "background").value = color;
            meteorDatabase.SaveChanges();
            UpdateBackground();

            MeteorCode.Message("Color saved");
        }

        private void UpdateBackground()
        {
            var color = meteorDatabase.Configurations.First(c => c.property == "background").value;

            if (color == "")
                color = "FF4764EA";

            var lgb = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };


            var gradientColor1 = ColorConverter.ConvertFromString("#FF2E2E2E");
            if (gradientColor1 != null)
            {
                var gs1 = new GradientStop
                {
                    Color = (Color)gradientColor1,
                    Offset = 0.12
                };

                var gradientColor2 = ColorConverter.ConvertFromString(color);
                if (gradientColor2 != null)
                {
                    var gs2 = new GradientStop
                    {
                        Color = (Color)gradientColor2,
                        Offset = 0.927
                    };

                    lgb.GradientStops.Add(gs1);
                    lgb.GradientStops.Add(gs2);
                }
            }

            ((MainWindow)Application.Current.MainWindow).Background = lgb;
        }

        //Advanced Options
        private void ChangePastebin(object sender, RoutedEventArgs e)
        {
            var val = PasteBinCheckBox.IsChecked == true ? "1" : "0";
            meteorDatabase.Configurations.First(c => c.property == "pastebin").value = val;
            meteorDatabase.SaveChanges();
            MeteorCode.Message(val == "1" ? "Pastebin activated" : "Pastebin deactivated");
        }

        private void FixRegistryClick(object sender, RoutedEventArgs e)
        {
            FixRegistry();

        }

        private void TestPaste(object sender, RoutedEventArgs e)
        {
            MeteorCode.Paste("Test Paste", "Test Stack");
        }

        private void S4ELaunchSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = LaunchModeOption.SelectedIndex;
            switch (index)
            {
                case 0:
                    MeteorCode.Message("Sm4shExplorer will not launch after a build");
                    break;
                case 1:
                    MeteorCode.Message("Sm4shExplorer will be launched after a build");
                    break;

                case 2:
                    MeteorCode.Message("Sm4shExplorer will be closed and relaunched after a build");
                    break;
            }
            
            meteorDatabase.Configurations.First(c => c.property == "AutoSmashExplorerRelaunch").value = index.ToString();
            meteorDatabase.SaveChanges();
        }

        //Various Functions
        private void ChangeS4EWorkspaceLocation(int workspaceId)
        {
            var s4ePath = meteorDatabase.Configurations.First(c => c.property == "SmashExplorerPath").value;
            //Loading local manifest
            var xml = new XmlDocument();
            var workspacePath = MeteorCode.AppPath + "/workspaces/workspace_" + workspaceId + "/";
            if (!Directory.Exists(workspacePath + "/content/patch/"))
                Directory.CreateDirectory(workspacePath + "/content/patch/");
            if (File.Exists(s4ePath + "/sm4shmod.xml"))
            {
                xml.Load(s4ePath + "/sm4shmod.xml");
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

                xml.Save(s4ePath + "/sm4shmod.xml");
            }
            else
            {
            }
        }

        private void FixRegistry()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                var customProtocol = Registry.ClassesRoot.OpenSubKey("meteor\\shell\\open\\command", true);
                if (customProtocol != null)
                {
                    customProtocol.SetValue("", "\"" + MeteorCode.AppPath + "\\Meteor.exe\" \"%1\"",
                        RegistryValueKind.String);
                    customProtocol.Close();

                    MeteorCode.Message("Fix successful");
                }
            }
            else
            {
                MeteorCode.Message("You cannot do that without Administrator rights. Relaunch the app in Administrator mode and try again");
            }
        }

        
    }
}