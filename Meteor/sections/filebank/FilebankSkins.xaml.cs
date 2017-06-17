using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Meteor.database;

namespace Meteor.sections.filebank
{
    /// <summary>
    /// Interaction logic for FilebankSkins.xaml
    /// </summary>
    public partial class FilebankSkins : Page
    {
        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;
        private readonly db_handler _dbHandler;

        private ArrayList Skins { get; set; }

        public FilebankSkins()
        {
            InitializeComponent();

            _dbHandler = new db_handler();
            LoadCharacters();
        }

        private void LoadCharacters()
        {
            CharacterListBox.Items.Clear();
            var characters = _dbHandler.get_characters(int.Parse(_dbHandler.get_property("sort_order")));

            var allCharactersItem = new ListBoxItem {Content = "All Characters"};
            CharacterListBox.Items.Add(allCharactersItem);

            foreach (string s in characters)
            {
                var item = new ComboBoxItem {Content = s};
                CharacterListBox.Items.Add(item);
            }

            CharacterListBox.SelectedIndex = 0;
        }

        //When a skin character is selected
        private void CharacterSelected(object sender, SelectionChangedEventArgs e)
        {
            ReloadSkins();
        }

        //When a skin is selected
        private void SkinSelected(object sender, SelectionChangedEventArgs e)
        {
            if (SkinsListBox.SelectedIndex != -1)
            {
                var infos = _dbHandler.get_skin_info((int)Skins[SkinsListBox.SelectedIndex]);
                AuthorValueLabel.Content = infos[1];
                CspsValueLabel.Content = infos[3];
                ModelsValueLabel.Content = infos[2];
                SkinIdValueLabel.Content = (int)Skins[SkinsListBox.SelectedIndex];
            }
        }

        //Deletes a skin permanently
        private void DeleteSkin(object sender, RoutedEventArgs e)
        {
            var id = (int)Skins[SkinsListBox.SelectedIndex];
            var index = SkinsListBox.SelectedIndex;
            if (!_dbHandler.check_skin_in_library(id))
            {
                var skinpath = AppPath + "/filebank/skins/" + id + "/";
                if (Directory.Exists(skinpath))
                    Directory.Delete(skinpath, true);
                _dbHandler.delete_skin(id);

                ReloadSkins();
                if (index > SkinsListBox.Items.Count - 1)
                {
                    SkinsListBox.SelectedIndex = SkinsListBox.Items.Count - 1;
                }
                else
                {
                    SkinsListBox.SelectedIndex = index;
                }
            }
            else
            {
                            MeteorCode.WriteToConsole("You have to remove it from the workspaces first", 1);
            }
        }

        private void pack_skin(object sender, RoutedEventArgs e)
        {
            var id = (int)Skins[SkinsListBox.SelectedIndex];
            _dbHandler.add_packer_item(0, id);
                        MeteorCode.WriteToConsole("Skin added to packer", 0);
        }

        //Launches forge
        private void PreviewForge(object sender, RoutedEventArgs e)
        {
            var id = (int)Skins[SkinsListBox.SelectedIndex];
            var modelpath = AppPath + "/filebank/skins/" + id + "/models/body/cxx/model.nud";

            if (File.Exists(modelpath))
                if (File.Exists(AppPath + "/forge/Smash Forge.exe"))
                {
                    var startInfo = new ProcessStartInfo();
                    startInfo.FileName = AppPath + "/forge/Smash Forge.exe";
                    startInfo.Arguments = "--superclean";
                    Process.Start(startInfo);

                    Thread.Sleep(3000);

                    var startInfo2 = new ProcessStartInfo();
                    startInfo2.FileName = AppPath + "/forge/Smash Forge.exe";
                    startInfo2.Arguments = "\" " + modelpath + "\" ";
                    Process.Start(startInfo2);
                }
                else
                {
                                MeteorCode.WriteToConsole("Smash Forge was not found in /forge", 1);
                }
            else
                            MeteorCode.WriteToConsole("There is no body/cXX to open in Smash Forge", 1);
        }

        //Inserts the skin in the active workspace
        private void InsertSkin(object sender, RoutedEventArgs e)
        {
            var id = (int)Skins[SkinsListBox.SelectedIndex];
            var workspace = int.Parse(_dbHandler.get_property("workspace"));
            int character_id = _dbHandler.get_character_id(id);
            var character = _dbHandler.get_character_name(id);

            var slot = _dbHandler.get_character_skins(character, workspace.ToString()).Count + 1;
                        MeteorCode.WriteToConsole("Trying to insert skin #" + id + " into slot #" + slot + " for character '" + character + "' into workspace #" + workspace, 3);
            _dbHandler.insert_skin(id, workspace, character_id, slot);
                        MeteorCode.WriteToConsole("The skin was inserted in the workspace", 0);
        }

        private void FilebankDeleteKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (SkinsListBox.Items.Count > 0)
                {
                    var id = (int)Skins[SkinsListBox.SelectedIndex];
                    var index = SkinsListBox.SelectedIndex;

                    if (!_dbHandler.check_skin_in_library(id))
                    {
                        var skinpath = AppPath + "/filebank/skins/" + id + "/";
                        if (Directory.Exists(skinpath))
                            Directory.Delete(skinpath, true);
                        _dbHandler.delete_skin(id);

                        ReloadSkins();
                        if (SkinsListBox.Items.Count > 0)
                        {
                            if (index > SkinsListBox.Items.Count - 1)
                            {
                                SkinsListBox.SelectedIndex = SkinsListBox.Items.Count - 1;
                            }
                            else
                            {
                                SkinsListBox.SelectedIndex = index;
                            }
                        }

                    }
                    else
                    {
                                    MeteorCode.WriteToConsole("You have to remove it from the workspaces first", 1);
                    }
                }

            }
        }

        //Reloads
        private void ReloadSkins()
        {
            if (CharacterListBox.SelectedIndex == -1) return;

            var character = (ListBoxItem)CharacterListBox.Items[CharacterListBox.SelectedIndex];
            var characterName = character.Content.ToString();
            ArrayList skins;
            if (characterName == "All Characters")
            {
                skins = _dbHandler.GetCustomSkins();
                Skins =
                    _dbHandler.get_custom_skins_id();
                SkinsListBox.Items.Clear();
            }
            else
            {
                skins = _dbHandler.get_character_custom_skins(characterName, _dbHandler.get_property("workspace"));
                Skins =
                    _dbHandler.get_character_custom_skins_id(character.Content.ToString(), _dbHandler.get_property("workspace"));
                SkinsListBox.Items.Clear();
            }




            foreach (string s in skins)
            {
                var lbi = new ListBoxItem {Content = s};
                SkinsListBox.Items.Add(lbi);
            }
        }

    }
}
