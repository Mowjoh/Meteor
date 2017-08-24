using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Meteor.content;
using Meteor.database;

namespace Meteor.sections.filebank
{
    public partial class FilebankNameplates
    {
        private MeteorDatabase meteorDatabase;

        private int SelectedId { get; set; }
        private int ActiveWorkspace { get; set; }

        public FilebankNameplates()
        {
            InitializeComponent();

            meteorDatabase = new MeteorDatabase();
            ActiveWorkspace = int.Parse(meteorDatabase.Configurations.First(c => c.property == "activeWorkspace")
                .value);

            LoadCharacters();
            //ReloadNameplates();
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
                NameplateListItem lbi = (NameplateListItem) NameplateListBox.Items[NameplateListBox.SelectedIndex];
                Nameplate nameplate = meteorDatabase.Nameplates.First(n => n.Id == lbi.Id);

                AuthorValueTextBox.Text = nameplate.author;
                NameValueTextBox.Text = lbi.Content.ToString();
                IdValueLabel.Content = nameplate.Id;
                CharacterValueLabel.Content = nameplate.Character.name;

                SelectedId = nameplate.Id;
            }
        }

        //Attributes
        private void SaveName(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            Nameplate nameplate = meteorDatabase.Nameplates.First(n => n.Id == SelectedId);
            nameplate.name = NameValueTextBox.Text;
            meteorDatabase.SaveChanges();

            MeteorCode.WriteToConsole("Nameplate name saved", 0);

            var lbi = (ListBoxItem) NameplateListBox.SelectedItem;
            lbi.Content = nameplate.name;
        }

        private void SaveAuthor(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Nameplate nameplate = meteorDatabase.Nameplates.First(n => n.Id == SelectedId);
            nameplate.author = AuthorValueTextBox.Text;
            meteorDatabase.SaveChanges();

            MeteorCode.WriteToConsole("Nameplate author saved", 0);
        }

        //Actions
        private void DeleteNameplate(object sender, RoutedEventArgs e)
        {
            Nameplate nameplate = meteorDatabase.Nameplates.First(n => n.Id == SelectedId);
            NameplateObject no = new NameplateObject(SelectedId, nameplate.Character.Id, ActiveWorkspace);
            if (File.Exists(no.full_path))
            {
                File.Delete(no.full_path);
            }
            meteorDatabase.Nameplates.Remove(nameplate);
            meteorDatabase.SaveChanges();
            ReloadNameplates();
        }

        private void PackNameplate(object sender, RoutedEventArgs e)
        {
            if (NameplateListBox.SelectedIndex != -1)
            {
                Nameplate nameplate = meteorDatabase.Nameplates.First(n => n.Id == SelectedId);
                Packer packItem = new Packer()
                {
                    content_id = nameplate.Id,
                    content_type = 1
                };
                meteorDatabase.Packers.Add(packItem);
                meteorDatabase.SaveChanges();

                MeteorCode.WriteToConsole("Nameplate added to packer", 0);
            }
        }


        //Drop Zone
        private void NameplateDrop(object sender, DragEventArgs e)
        {
            var folderlist = (string[]) e.Data.GetData(DataFormats.FileDrop, false);

            if (folderlist == null) return;

            var attr = File.GetAttributes(folderlist[0]);

            //parsing selected index
            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                MeteorCode.Message("Please drop a chrn_11 file");
            }

            else
            {
                foreach (var s in folderlist)
                {
                    String filename = new FileInfo(s).Name;
                    
                        String type = filename.Split('_')[0];
                        String number = filename.Split('_')[1];
                        String charactername = filename.Split('_')[2];
                        if (type == "chrn" && number == "11")
                        {
                            Character character = meteorDatabase.Characters.First(c => c.msl_name == charactername);
                            var nameplate = new NameplateObject(s, character.Id, ActiveWorkspace);
                            ReloadNameplates();
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

            meteorDatabase = new MeteorDatabase();

            if (CharacterComboBox.SelectedIndex == -1) return;
            NameplateListBox.Items.Clear();
            
            var characterItem = (CharacterCombo) CharacterComboBox.Items[CharacterComboBox.SelectedIndex];

            if (characterItem.Id == 0)
            {
                foreach (Nameplate nameplate in meteorDatabase.Nameplates)
                {
                    var lbi = new NameplateListItem
                    {
                        Content = nameplate.name,
                        Id = nameplate.Id
                    };

                    NameplateListBox.Items.Add(lbi);
                }
            }
            else
            {
                
                foreach (Nameplate nameplate in meteorDatabase.Nameplates.Where(n => n.character_id == characterItem.Id))
                {
                    var lbi = new NameplateListItem
                    {
                        Content = nameplate.name,
                        Id = nameplate.Id
                    };

                    NameplateListBox.Items.Add(lbi);
                }
            }
        }

        private void LoadCharacters()
        {
            CharacterComboBox.Items.Clear();

            CharacterCombo allCharactersItem = new CharacterCombo() {Content = "All Characters", Id = 0};
            CharacterComboBox.Items.Add(allCharactersItem);

            foreach (Character character in meteorDatabase.Characters)
            {
                var item = new CharacterCombo {Content = character.name, Id = character.Id};
                CharacterComboBox.Items.Add(item);
            }

            CharacterComboBox.SelectedIndex = 0;
        }
    }

    public class CharacterCombo : ComboBoxItem
    {
        public int Id { get; set; }
    }

    public class NameplateListItem : ListBoxItem
    {
        public int Id { get; set; }
    }
}
