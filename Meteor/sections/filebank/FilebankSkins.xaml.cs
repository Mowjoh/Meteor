using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Meteor.database;

namespace Meteor.sections.filebank
{
    /// <summary>
    /// Interaction logic for FilebankSkins.xaml
    /// </summary>
    public partial class FilebankSkins : Page
    {
        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;
        private MeteorDatabase meteorDatabase;

        private Skin selectedSkin;

        public FilebankSkins()
        {
            InitializeComponent();

            meteorDatabase = ((MainWindow) Application.Current.MainWindow).meteorDatabase;
            LoadCharacters();
        }

        private void LoadCharacters()
        {
            CharacterListBox.Items.Clear();

            var allCharactersItem = new CharacterItem() {Content = "All Characters", Id = 0};
            CharacterListBox.Items.Add(allCharactersItem);

            foreach (Character character in meteorDatabase.Characters)
            {
                var item = new CharacterItem() {Content = character.name, Id = character.Id};
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
                SkinItem item = (SkinItem) SkinsListBox.SelectedItem;
                selectedSkin = meteorDatabase.Skins.First(s => s.Id == item.Id);
                AuthorValueLabel.Content = selectedSkin.author;
                CspsValueLabel.Content = selectedSkin.skin_csps;
                ModelsValueLabel.Content = selectedSkin.skin_models;
                CharacterValueLabel.Content = selectedSkin.Character.name;
                SkinIdValueLabel.Content = selectedSkin.Id;
            }
        }

        //Deletes a skin permanently
        private void DeletePressed(object sender, RoutedEventArgs e)
        {
            DeleteSkin();
        }

        private void FilebankDeleteKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteSkin();
            }
        }

        private void DeleteSkin()
        {
            //Getting index
            int index = SkinsListBox.SelectedIndex;

            //Setting folder path
            var skinpath = AppPath + "/filebank/skins/" + selectedSkin.Id + "/";

            //Deleting files
            if (Directory.Exists(skinpath))
                Directory.Delete(skinpath, true);

            //Removing SkinLibrary entries and Skin
            foreach (SkinLibrary entry in meteorDatabase.SkinLibraries.Where(sl => sl.skin_id == selectedSkin.Id))
            {
                meteorDatabase.SkinLibraries.Remove(entry);
            }
            meteorDatabase.Skins.Remove(selectedSkin);
            meteorDatabase.SaveChanges();

            //Reordering slots after deletion
            ReorderCharacterSkins(selectedSkin.character_id);

            //Reloading interface after delete

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

        private void pack_skin(object sender, RoutedEventArgs e)
        {
            Packer packItem = new Packer()
            {
                content_id = selectedSkin.Id,
                content_type = 0
            };

            meteorDatabase.Packers.Add(packItem);
            meteorDatabase.SaveChanges();

            MeteorCode.WriteToConsole("Skin added to packer", 0);
        }

        //Launches forge
        private void PreviewForge(object sender, RoutedEventArgs e)
        {
            var id = selectedSkin.Id;
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

        //Reloads
        public void ReloadSkins()
        {
            meteorDatabase = new MeteorDatabase();

            if (CharacterListBox.SelectedIndex == -1) return;

            CharacterItem character = (CharacterItem) CharacterListBox.Items[CharacterListBox.SelectedIndex];
            var characterName = character.Content.ToString();
            if (characterName == "All Characters")
            {
                SkinsListBox.Items.Clear();
                foreach (Skin skin in meteorDatabase.Skins.Where(s => s.skinLock == false))
                {
                    var lbi = new SkinItem {Content = skin.name, Id = skin.Id};
                    SkinsListBox.Items.Add(lbi);
                }
            }
            else
            {
                SkinsListBox.Items.Clear();
                foreach (Skin skin in meteorDatabase.Skins.Where(
                    s => s.character_id == character.Id && s.skinLock == false))
                {
                    var lbi = new SkinItem {Content = skin.name, Id = skin.Id};
                    SkinsListBox.Items.Add(lbi);
                }
            }
        }

        public void ReorderCharacterSkins(int CharacterId)
        {
            foreach (Workspace workspace in meteorDatabase.Workspaces.Where(w => w.slot > 0))
            {
                int count = 1;
                foreach (SkinLibrary entry in meteorDatabase.SkinLibraries.Where(
                    sl => sl.workspace_id == workspace.Id && sl.character_id == CharacterId))
                {
                    entry.slot = count;
                    count++;
                }
            }
            meteorDatabase.SaveChanges();
        }
    }

    public class SkinItem : ListBoxItem
    {
        public int Id { get; set; }
    }

    public class CharacterItem : ComboBoxItem
    {
        public int Id { get; set; }
    }
}
