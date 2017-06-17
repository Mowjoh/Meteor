using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Meteor.content;
using Meteor.database;

namespace Meteor.sections.filebank
{
    public partial class FilebankNameplates
    {
        private readonly db_handler _dbHandler;

        private int SelectedId { get; set; }
        private int ActiveWorkspace { get; set; }
        private ArrayList Nameplates { get; set; }

        public FilebankNameplates()
        {
            InitializeComponent();

            _dbHandler = new db_handler();
            ActiveWorkspace = int.Parse(_dbHandler.get_property("workspace"));

            LoadCharacters();
            ReloadNameplates();

            
        }

        //Character Select
        private void CharacterChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadNameplates();
        }

        //Nameplate List
        private void NameplateSelected(object sender, SelectionChangedEventArgs e)
        {
            if (NameplateListBox.SelectedIndex != -1)
            {
                var infos = _dbHandler.get_nameplate_info((int)Nameplates[NameplateListBox.SelectedIndex]);
                AuthorValueTextBox.Text = infos[1];
                ListBoxItem lbi = (ListBoxItem)NameplateListBox.Items[NameplateListBox.SelectedIndex];
                NameValueTextBox.Text = lbi.Content.ToString();
                IdValueLabel.Content = (int)Nameplates[NameplateListBox.SelectedIndex];
                CharacterValueLabel.Content = infos[2];

                SelectedId = (int)Nameplates[NameplateListBox.SelectedIndex];
            }
        }

        //Attributes
        private void SaveName(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var newName = NameValueTextBox.Text;
            _dbHandler.set_nameplate_name(newName, SelectedId);

                        MeteorCode.WriteToConsole("Nameplate name saved", 0);

            var lbi = (ListBoxItem)NameplateListBox.SelectedItem;
            lbi.Content = newName;
        }

        private void SaveAuthor(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var newName = AuthorValueTextBox.Text;
            _dbHandler.set_nameplate_author(newName, SelectedId);

                        MeteorCode.WriteToConsole("Nameplate author saved", 0);
        }

        //Actions
        private void DeleteNameplate(object sender, RoutedEventArgs e)
        {
            nameplate n = new nameplate(SelectedId, _dbHandler.get_character_id_nameplate(SelectedId), ActiveWorkspace, _dbHandler);
            if (File.Exists(n.full_path))
            {
                File.Delete(n.full_path);
            }
            _dbHandler.delete_nameplate(SelectedId);
            ReloadNameplates();
        }

        private void PackNameplate(object sender, RoutedEventArgs e)
        {
            if (NameplateListBox.SelectedIndex != -1)
            {
                var id = (int)Nameplates[NameplateListBox.SelectedIndex];
                _dbHandler.get_custom_nameplates_id();
                _dbHandler.add_packer_item(1, id);
                MeteorCode.WriteToConsole("Nameplate added to packer", 0);
            }
            
        }

        
        //Drop Zone
        private void NameplateDrop(object sender, DragEventArgs e)
        {
            var folderlist = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (folderlist == null) return;

            var attr = File.GetAttributes(folderlist[0]);

            //parsing selected index
            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                            MeteorCode.WriteToConsole("Please drop a chrn_11 file", 2);
            }

            else
            {

                foreach (var s in folderlist)
                {
                    String filename = new FileInfo(s).Name;
                    try
                    {
                        String type = filename.Split('_')[0];
                        String number = filename.Split('_')[1];
                        String character = filename.Split('_')[2];
                        if (type == "chrn" && number == "11")
                        {
                            int charId = _dbHandler.get_character_id_cspfoldername(character);
                            var nameplate = new nameplate(s, charId, ActiveWorkspace, _dbHandler);
                            ReloadNameplates();
                        }
                    }
                    catch (Exception ex)
                    {
                                    MeteorCode.WriteToConsole("Error " + ex.Message, 2);
                    }

                }

            }
        }

        private void NameplateDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        //Reloads
        public void ReloadNameplates()
        {
            if (CharacterComboBox.SelectedIndex == -1) return;

            var character = (ListBoxItem)CharacterComboBox.Items[CharacterComboBox.SelectedIndex];
            var characterName = character.Content.ToString();
            ArrayList nameplates;
            if (characterName == "All Characters")
            {
                nameplates = _dbHandler.GetCustomNameplates();
                Nameplates = _dbHandler.get_custom_nameplates_id();
                NameplateListBox.Items.Clear();
            }
            else
            {
                nameplates = _dbHandler.get_character_custom_nameplates(characterName, _dbHandler.get_property("workspace"));
                Nameplates =
                    _dbHandler.get_character_custom_nameplates_id(character.Content.ToString(), _dbHandler.get_property("workspace"));
                NameplateListBox.Items.Clear();
            }




            foreach (string n in nameplates)
            {
                var lbi = new ListBoxItem();
                lbi.Content = n;

                NameplateListBox.Items.Add(lbi);
            }
        }

        private void LoadCharacters()
        {
            CharacterComboBox.Items.Clear();
            var characters = _dbHandler.get_characters(int.Parse(_dbHandler.get_property("sort_order")));

            var allCharactersItem = new ListBoxItem { Content = "All Characters" };
            CharacterComboBox.Items.Add(allCharactersItem);

            foreach (string s in characters)
            {
                var item = new ComboBoxItem { Content = s };
                CharacterComboBox.Items.Add(item);
            }

            CharacterComboBox.SelectedIndex = 0;
        }
    }
}
