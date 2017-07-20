using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Meteor.database;
using Path = System.IO.Path;
using System.Xml;
using Microsoft.Win32;
using winforms = System.Windows.Forms;

namespace Meteor.sections
{
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public partial class Configuration
    {
        //Private variables
        private readonly db_handler _dbHandler;

        private bool _loading = true;

        //Constructor
        public Configuration()
        {
            InitializeComponent();

            _dbHandler = new db_handler();

            retreive_config();
        }

        //Makes the interface reflect the current configuration
        private void retreive_config()
        {
            //Region Select
            combo_region.SelectedIndex = int.Parse(_dbHandler.get_property("region"));
            combo_language.SelectedIndex = int.Parse(_dbHandler.get_property("language"));

            //S4E path
            S4EPathTextbox.Text = _dbHandler.get_property("s4e_exe");

            //Config
            UsernameTextBox.Text = _dbHandler.get_property("username");
            SortbyComboBox.SelectedIndex = int.Parse(_dbHandler.get_property("sort_order"));
            AutoLaunchS4ECheckBox.IsChecked = _dbHandler.get_property("s4e_launch") == "1";

            //Dev Options
            DevLogsCheckBox.IsChecked = _dbHandler.get_property("dev_logs") == "1";

            //Pastebin Options
            PasteBinCheckBox.IsChecked = _dbHandler.get_property("pastebin") == "1";

            //GB Options
            GbuidTextBox.Text = _dbHandler.get_property("gb_uid");

            //ui_char status
            UicharStatusLabel.Content = _dbHandler.get_property("ui_char") == "1"
                ? "Status : imported"
                : "Status : not present";

            //Disables the lock and allows the console to be written again
            _loading = false;
        }

        //Regional options
        private void RegionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Getting the region
            var region = combo_region.SelectedIndex;
           _dbHandler.set_property_value("region",region.ToString());
            switch (region)
            {
                case 0:
                    WriteToConsole("Region changed to Europe", 0);
                    break;
                case 1:
                    WriteToConsole("Region changed to United States", 0);
                    break;
                case 2:
                    WriteToConsole("Region changed to Japan", 0);
                    break;
            }
        }

        private void LaunguageChanged(object sender, SelectionChangedEventArgs e)
        {
            var language = combo_language.SelectedIndex;
            _dbHandler.set_property_value("language", language.ToString());
            switch (language)
            {
                case 0:
                    WriteToConsole("Language changed to English", 0);
                    break;
                case 1:
                    WriteToConsole("Language changed to French", 0);
                    break;
                case 2:
                    WriteToConsole("Language changed to Spanish", 0);
                    break;
                case 3:
                    WriteToConsole("Language changed to German", 0);
                    break;
                case 4:
                    WriteToConsole("Language changed to Italian", 0);
                    break;
                case 5:
                    WriteToConsole("Language changed to Dutch", 0);
                    break;
                case 6:
                    WriteToConsole("Language changed to Portugese", 0);
                    break;
                case 7:
                    WriteToConsole("Language changed to Japanese", 0);
                    break;
            }
        }


        //S4E Options
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
                                WriteToConsole(
                                    "The file you selected is not called 'Sm4shFileExplorer.exe' but was still saved.",
                                    1);

                            _dbHandler.set_property_value("s4e_exe",exepath);
                            _dbHandler.set_property_value("s4e_path", path);
                            WriteToConsole("Sm4sh Explorer's executable path set to : " + exepath, 0);
                            S4EPathTextbox.Text = exepath;
                            ChangeS4EWorkspaceLocation(int.Parse(_dbHandler.get_property("workspace")));
                        }
                        else
                        {
                            WriteToConsole("The path is invalid. There is no file at that location", 2);
                        }
                    else
                        WriteToConsole("The path is invalid. It's not pointing to a .exe", 2);
                }
            }
            catch (Exception s4EpathException)
            {
                WriteToConsole("An error happend while setting up the path", 2);
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
                    var exepath = openFileDialog.FileName;
                    var directoryInfo = new FileInfo(exepath).Directory;
                    if (directoryInfo != null)
                    {
                        var path = directoryInfo.FullName;
                        var filetype = new FileInfo(exepath).Extension;

                        if (filetype == ".exe")
                        {
                            if (new FileInfo(exepath).Name != "Sm4shFileExplorer.exe")
                                WriteToConsole(
                                    "The file you selected is not called 'Sm4shFileExplorer.exe' but was still saved.",
                                    1);

                            _dbHandler.set_property_value("s4e_exe" , exepath);
                            _dbHandler.set_property_value("s4e_path" , path);
                            WriteToConsole("Sm4sh Explorer's executable path set to : " + openFileDialog.FileName, 0);
                            S4EPathTextbox.Text = openFileDialog.FileName;
                            ChangeS4EWorkspaceLocation(int.Parse(_dbHandler.get_property("workspace")));
                        }
                        else
                        {
                            WriteToConsole("The file you selected is not a .exe", 2);
                        }
                    }
                }
            }
            catch (Exception s4EpathException)
            {
                WriteToConsole("An error happend while setting up the path", 2);
                MeteorCode.Paste(s4EpathException.Message, s4EpathException.StackTrace);
            }
        }


        //UI_Char Options
        private void AutomaticSearchUi(object sender, RoutedEventArgs e)
        {
            var baseFolder = _dbHandler.get_property("s4e_path");
            var destination = MeteorCode.AppPath + "/filebank/configuration/uichar/ui_character_db.bin";
            if (Directory.Exists(baseFolder))
            {
                var files = Directory.GetFiles(baseFolder, "ui_character_db.bin", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    if (!Directory.Exists(MeteorCode.AppPath + "/filebank/configuration/uichar/"))
                        Directory.CreateDirectory(MeteorCode.AppPath + "/filebank/configuration/uichar/");
                    var filepath = files[0];
                    File.Copy(filepath, destination, true);
                    WriteToConsole("found the file at : " + filepath, 0);
                    UicharStatusLabel.Content = "Status : imported";
                    _dbHandler.set_property_value("ui_char" , "1");
                }
                else
                {
                    WriteToConsole("ui_character_db was not found in the specified SmashExplorer folder", 2);
                }
            }
            else
            {
                WriteToConsole("Please make sure Sm4sh Explorer's path is properly setup", 2);
            }
        }

        private void PickUi(object sender, RoutedEventArgs e)
        {
            //Setting up OpenFileDialog
            var ofd = new winforms.OpenFileDialog();
            ofd.ShowDialog();
            var filename = ofd.FileName;

            //Setting destination path
            var destination = MeteorCode.AppPath + "filebank/configuration/uichar/ui_character_db.bin";

            //Returning if the filename is incorrect
            if (Path.GetFileName(filename) != "ui_character_db.bin") return;

            //Checking destination directory
            if (!Directory.Exists(MeteorCode.AppPath + "filebank/configuration/uichar/"))
            {
                Directory.CreateDirectory(MeteorCode.AppPath + "filebank/configuration/uichar/");
            }

            File.Copy(filename, destination, true);

            //Setting database status
            UicharStatusLabel.Content = "Status : imported";
            _dbHandler.set_property_value("ui_char", "1");

            //Writing success
            WriteToConsole("ui_character_db.bin selected", 0);
        }


        //Configuration actions
        private void SortbyChanged(object sender, SelectionChangedEventArgs e)
        {
            var order = SortbyComboBox.SelectedIndex;
            _dbHandler.set_property_value(order.ToString(), "sort_order");
            WriteToConsole(order == 0 ? "Sort Order changed to Alphabetical" : "Sort Order changed to Game Order", 0);
            if (!_loading)
            {
                MessageBox.Show("Meteor will now shut down to apply the changes");
                Application.Current.Shutdown();
            }
        }

        private void GbuidSave(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _dbHandler.set_property_value("gb_uid", GbuidTextBox.Text );
                WriteToConsole("Gamebanana User Id set", 0);
            }
        }

        private void SaveUsername(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _dbHandler.set_property_value("username", UsernameTextBox.Text);
                WriteToConsole("Username set", 0);
            }
        }

        private void ChangeDevlocks(object sender, RoutedEventArgs e)
        {
            var val = DevLogsCheckBox.IsChecked == true ? "1" : "0";
            _dbHandler.set_property_value("dev_logs", val);
            WriteToConsole(val == "1" ? "Dev Logs Activated" : "Dev Logs Deactivated", 0);
        }

        private void ChangePastebin(object sender, RoutedEventArgs e)
        {
            var val = PasteBinCheckBox.IsChecked == true ? "1" : "0";
            _dbHandler.set_property_value("pastebin", val);
            WriteToConsole(val == "1" ? "Pastebin Activated" : "Pastebin Deactivated", 0);
        }

        private void S4EAutoLaunchChanged(object sender, RoutedEventArgs e)
        {
            var val = AutoLaunchS4ECheckBox.IsChecked == true ? "1" : "0";
            _dbHandler.set_property_value("s4e_launch", val);
            WriteToConsole(
                val == "1"
                    ? "Sm4sh Explorer will be launched after each export"
                    : "Sm4sh Explorer won't be launched after the export", 0);
        }

        private void SaveColor(object sender, RoutedEventArgs e)
        {
            var color = ColorPicker.SelectedColor.ToString();
            _dbHandler.set_property_value("background", color);
            UpdateBackground();
        }

        private void UpdateBackground()
        {
            var color = _dbHandler.get_property("background");

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
                    Color = (Color) gradientColor1,
                    Offset = 0.12
                };

                var gradientColor2 = ColorConverter.ConvertFromString(color);
                if (gradientColor2 != null)
                {
                    var gs2 = new GradientStop
                    {
                        Color = (Color) gradientColor2,
                        Offset = 0.927
                    };

                    lgb.GradientStops.Add(gs1);
                    lgb.GradientStops.Add(gs2);
                }
            }

            ((MainWindow) Application.Current.MainWindow).Background = lgb;
        }

        private void FixRegistryClick(object sender, RoutedEventArgs e)
        {
            FixRegistry();
        }

        private void TestPaste(object sender, RoutedEventArgs e)
        {
            MeteorCode.Paste("Test Paste", "Test Stack");
        }


        //Various Functions
        private void ChangeS4EWorkspaceLocation(int workspaceId)
        {
            //Loading local manifest
            var xml = new XmlDocument();
            var workspacePath = MeteorCode.AppPath + "/workspaces/workspace_" + workspaceId + "/";
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
                WriteToConsole("Could not assign Sm4sh Explorer's workspace", 1);
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
                }
                WriteToConsole("Fixed the download issue", 0);
            }
            else
            {
                WriteToConsole("You must launch the application as Admin to do that", 2);
            }
        }

        private void WriteToConsole(string s, int type)
        {
            if (_loading) return;

            var typeText = "";
            var date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            switch (type)
            {
                case 0:
                    typeText = "Success";
                    break;
                case 1:
                    typeText = "Warning";
                    break;
                case 2:
                    typeText = "Error";
                    break;
                case 3:
                    typeText = "Dev Log";
                    break;
            }

            if (type != 3)
            {
                ((MainWindow) Application.Current.MainWindow).Console.Text =
                    date + " | " + typeText + " | " + s + "\n" +
                    ((MainWindow) Application.Current.MainWindow).Console.Text;
            }
            else
            {
                if (_dbHandler.get_property("dev_logs") == "1")
                    ((MainWindow) Application.Current.MainWindow).Console.Text =
                        date + " | " + typeText + " | " + s + "\n" +
                        ((MainWindow) Application.Current.MainWindow).Console.Text;
            }
        }
    }
}