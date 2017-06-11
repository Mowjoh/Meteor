using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
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
        private readonly Pastebin.Pastebin _pastebin;
        private readonly db_handler _dbHandler;

        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;

        public Configuration()
        {
            InitializeComponent();
            
            _dbHandler = new db_handler();
            _pastebin = new Pastebin.Pastebin("f165a49418f0ed6c5f61e9e233889d91");

            retreive_config();
        }

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
            UicharStatusLabel.Content = _dbHandler.get_property("ui_char") == "1" ? "Status : imported" : "Status : not present";

        }

        //Regional options
        private void RegionChanged(object sender, SelectionChangedEventArgs e)
        {
            var region = combo_region.SelectedIndex;
            _dbHandler.set_property_value(region.ToString(), "region");
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
            _dbHandler.set_property_value(language.ToString(), "language");
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
                                "The file you selected is not called 'Sm4shFileExplorer.exe' but was still saved.", 1);

                        _dbHandler.set_property_value(exepath, "s4e_exe");
                        _dbHandler.set_property_value(path, "s4e_path");
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

        private void S4EPathSave(object sender, RoutedEventArgs e)
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
                                "The file you selected is not called 'Sm4shFileExplorer.exe' but was still saved.", 1);

                        _dbHandler.set_property_value(exepath, "s4e_exe");
                        _dbHandler.set_property_value(path, "s4e_path");
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


        //UI_Char Options
        private void AutomaticSearchUi(object sender, RoutedEventArgs e)
        {
            var baseFolder = _dbHandler.get_property("s4e_path");
            var destination = AppPath + "/filebank/configuration/uichar/ui_character_db.bin";
            if (Directory.Exists(baseFolder))
            {
                var files = Directory.GetFiles(baseFolder, "ui_character_db.bin", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    if (!Directory.Exists(AppPath + "/filebank/configuration/uichar/"))
                        Directory.CreateDirectory(AppPath + "/filebank/configuration/uichar/");
                    var filepath = files[0];
                    File.Copy(filepath, destination, true);
                    WriteToConsole("found the file at : " + filepath, 0);
                    UicharStatusLabel.Content = "Status : imported";
                    _dbHandler.set_property_value("1", "ui_char");
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
            var ofd = new winforms.OpenFileDialog();
            ofd.ShowDialog();
            var filename = ofd.FileName;
            var destination = AppPath + "filebank/configuration/uichar/ui_character_db.bin";


            if (Path.GetFileName(filename) != "ui_character_db.bin") return;

            if (!Directory.Exists(AppPath + "filebank/configuration/uichar/"))
                Directory.CreateDirectory(AppPath + "filebank/configuration/uichar/");
            File.Copy(filename, destination, true);

            WriteToConsole("ui_character_db.bin selected", 0);
            UicharStatusLabel.Content = "Status : imported";
            _dbHandler.set_property_value("1", "ui_char");
        }


        //Configuration actions
        private void SortbyChanged(object sender, SelectionChangedEventArgs e)
        {
            

            var order = SortbyComboBox.SelectedIndex;
            _dbHandler.set_property_value(order.ToString(), "sort_order");
            WriteToConsole(order == 0 ? "Sort Order changed to Alphabetical" : "Sort Order changed to Game Order", 0);
        }

        private void GbuidSave(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                /* if (ConfigurationSection.SaveGamebananaUserId(config_gbuser.Text))
                 {
                     Write_Console("Gamebanana user changed to " + config_gbuser.Text, 0);
                 }
                 else
                 {
                     Write_Console("Failed to change the Gamebanana user ID", 2);
                 }*/
            }
        }

        private void SaveUsername(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                /* if (ConfigurationSection.SaveUsername(config_username.Text))
                 {
                     Write_Console("Username changed to " + config_username.Text, 0);
                 }
                 else
                 {
                     Write_Console("Failed to change the Gamebanana user ID", 2);
                 }*/
            }
        }

        private void ChangeDevlocks(object sender, RoutedEventArgs e)
        {
            var val = DevLogsCheckBox.IsChecked == true ? "1" : "0";
            _dbHandler.set_property_value(val, "dev_logs");
            WriteToConsole(val == "1" ? "Dev Logs Activated" : "Dev Logs Deactivated", 0);
        }

        private void ChangePastebin(object sender, RoutedEventArgs e)
        {
            var val = PasteBinCheckBox.IsChecked == true ? "1" : "0";
            _dbHandler.set_property_value(val, "pastebin");
            WriteToConsole(val == "1" ? "Pastebin Activated" : "Pastebin Deactivated", 0);
        }

        private void S4EAutoLaunchChanged(object sender, RoutedEventArgs e)
        {
            var val = AutoLaunchS4ECheckBox.IsChecked == true ? "1" : "0";
            _dbHandler.set_property_value(val, "s4e_launch");
            WriteToConsole(
                val == "1"
                    ? "Sm4sh Explorer will be launched after each export"
                    : "Sm4sh Explorer won't be launched after the export", 0);
        }

        private void SaveColor(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(RedValueTextBox.Text, out int r))
                if (int.TryParse(BlueValueTextBox.Text, out int b))
                    if (int.TryParse(GreenValueTextBox.Text, out int g))
                        if ((r > 255) | (g > 255) | (b > 255))
                        {
                            WriteToConsole("Values cannot be > 255", 1);
                        }
                        else
                        {
                            if ((r < 0) | (g < 0) | (b < 0))
                            {
                                WriteToConsole("Values cannot be < 0", 1);
                            }
                            else
                            {
                                var redValue = r.ToString("X");
                                var greenValue = g.ToString("X");
                                var blueValue = b.ToString("X");
                                redValue = redValue.Length == 1 ? "0" + redValue : redValue;
                                greenValue = greenValue.Length == 1 ? "0" + greenValue : greenValue;
                                blueValue = blueValue.Length == 1 ? "0" + blueValue : blueValue;

                                var color = "FF" + redValue + blueValue + greenValue;
                                _dbHandler.set_property_value(color, "background");
                                UpdateBackground();
                            }
                        }
                    else
                        WriteToConsole("Green value is not a number", 1);
                else
                    WriteToConsole("Blue value is not a number", 1);
            else
                WriteToConsole("Red value is not a number", 1);
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

                var gradientColor2= ColorConverter.ConvertFromString("#" + color);
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

            ((MainWindow)Application.Current.MainWindow).Background = lgb;
        }

        private void FixRegistryClick(object sender, RoutedEventArgs e)
        {
            FixRegistry();
        }

        private void TestPaste(object sender, RoutedEventArgs e)
        {
            Paste("Test Paste", "Test Stack");
        }


        //Various Functions
        private void ChangeS4EWorkspaceLocation(int workspaceId)
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
                    customProtocol.SetValue("", "\"" + AppPath + "\\Meteor.exe\" \"%1\"", RegistryValueKind.String);
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
                ((MainWindow)Application.Current.MainWindow).Console.Text = date + " | " + typeText + " | " + s + "\n" + ((MainWindow)Application.Current.MainWindow).Console.Text;
            }
            else
            {
                if (_dbHandler.get_property("dev_logs") == "1")
                    ((MainWindow)Application.Current.MainWindow).Console.Text = date + " | " + typeText + " | " + s + "\n" + ((MainWindow)Application.Current.MainWindow).Console.Text;
            }
        }

        private void Paste(String message, String stack)
        {
            if (_dbHandler.get_property("pastebin") == "1")
            {
                var result = MessageBox.Show("An error happened with Meteor. Open the pastebin?", "Segtendo WARNING",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {

                    try
                    {
                        var url = _pastebin.CreatePaste("Meteor Error", "csharp", "Error Message : " + message + "\n StackTrace:" + stack, Pastebin.PasteExposure.Public, Pastebin.PasteExpiration.OneWeek);

                        Process.Start(url);
                    }
                    catch (Exception ee)
                    {
                        WriteToConsole("Pastebin error : " + ee.Message, 2);
                    }
                }

            }
        }

    }

}
