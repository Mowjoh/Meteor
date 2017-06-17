using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Meteor.content;
using Meteor.database;

namespace Meteor.sections
{
    public partial class Skins
    {
        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;

        private readonly db_handler _dbHandler;

        private String SelectedCharacterName { get; set; }
        private int SelectedCharacterId { get; set;  }
        private int SelectedSkinId { get; set; }
        private int SelectedSlot { get; set; }
        private Skin SelectedSkin { get; set; }

        private int ActiveWorkspace { get; set; }

        private bool LoadingNameplates { get; set; }

        private ObservableCollection<CharactersListViewItemsData> CharactersItemsCollections { get; } = new ObservableCollection<CharactersListViewItemsData>();

        //Constructor
        public Skins()
        {
            InitializeComponent();

            _dbHandler = new db_handler();

            FillCharacters();
        }


        //Character List
        private void CharacterSelected(object sender, SelectionChangedEventArgs e)
        {
            //Getting selected item
            var li = (CharactersListViewItemsData)CharacterListView.SelectedItem;
            if (li != null)
            {
                //Setup global values
                SelectedCharacterName = li.CharacterLabelValue;
                SelectedCharacterId = _dbHandler.get_character_id(SelectedCharacterName);

                //Load the skin list for that selected character
                LoadSkinList(SelectedCharacterName);

                //Writing text on the console, for dev
                            MeteorCode.WriteToConsole("Selected char name " + SelectedCharacterName, 3);
                            MeteorCode.WriteToConsole("Selected char id " + SelectedCharacterId, 3);
            }
            //No character selected
            else
            {
                //Resetting global var
                SelectedCharacterName = "";
                SelectedCharacterId = -1;
            }
        }


        //Skin List
        private void SkinSelected(object sender, SelectionChangedEventArgs e)
        {
            //If no skin is selected
            if (SkinsListBox.SelectedIndex == -1)
            {
                ClearSkinControls();
            }
            else
            {
                ActiveWorkspace = int.Parse(_dbHandler.get_property("workspace"));
                //Setting up global variables
                SelectedSlot = SkinsListBox.SelectedIndex + 1;
                SelectedSkinId = _dbHandler.get_skin_id(SelectedCharacterId, SelectedSlot, ActiveWorkspace);

                //Getting id
                var gbUid = _dbHandler.get_skin_gb_uid(SelectedSkinId);

                if (SelectedSlot > 0)
                {
                    //Loading info
                    var infos = _dbHandler.get_skin_info(SelectedSkinId);
                    SkinNameValueTextBox.Text = infos[0];
                    SkinAuthorValueTextBox.Text = infos[1];
                    SkinSlotValueTextBox.Text = SelectedSlot.ToString();

                    //Getting locks
                    var skinLock = _dbHandler.skin_locked(SelectedSkinId);
                    var workspaceLock = _dbHandler.skin_locked(SelectedSlot, SelectedCharacterId);

                    //Setting control states
                    SetSkinState(workspaceLock, skinLock, gbUid);



                    if (SelectedSlot < 17)
                    {
                        LoadingNameplates = true;
                        SkinNameplateValueComboBox.Items.Clear();
                        SkinNameplateValueComboBox.Items.Add("No Nameplate");
                        foreach (String s in _dbHandler.get_character_nameplates(SelectedCharacterId))
                        {
                            SkinNameplateValueComboBox.Items.Add(s);
                        }
                        SkinNameplateValueComboBox.IsEnabled = true;
                        LoadingNameplates = false;
                    }
                    else
                    {
                        SkinNameplateValueComboBox.IsEnabled = false;
                    }


                    //loading skin files
                    LoadSkinFiles();
                }

                //Writing stuff
                            MeteorCode.WriteToConsole("Selected slot " + SelectedSlot, 3);
                            MeteorCode.WriteToConsole("Selected skin id " + SelectedSkinId, 3);
            }
        }

        private void AddSkin(object sender, RoutedEventArgs e)
        {
            try
            {
                _dbHandler.add_skin("Custom Skin", "", "", "", SelectedCharacterId, SkinsListBox.Items.Count + 1);
                LoadSkinList(SelectedCharacterName);
            }
            catch (Exception ee)
            {
                        MeteorCode.WriteToConsole("There was an error while adding the skin", 2);
                MeteorCode.Paste(ee.Message, ee.StackTrace);
            }


        }

        private void RemoveSkin()
        {
            if (!_dbHandler.skin_locked(SelectedSlot, SelectedCharacterId))
            {
                _dbHandler.remove_skin(SelectedSlot, SelectedCharacterId, _dbHandler.get_property("workspace"));
                _dbHandler.reorder_skins(SelectedCharacterId, ActiveWorkspace);
                LoadSkinList(SelectedCharacterName);
                            MeteorCode.WriteToConsole("Skin removed from the workspace", 0);
            }
            else
            {
                if (!_dbHandler.skin_locked(SelectedSkinId))
                    RestoreSkin();
                else
                                MeteorCode.WriteToConsole("Sorry, you cannot remove default skin slots!", 1);
            }
        }


        //Skin Information
        private void SaveSkinName(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var newName = SkinNameValueTextBox.Text;
            _dbHandler.set_skin_name(newName, SelectedSkinId);

                        MeteorCode.WriteToConsole("Skin name saved", 0);

            var lbi = (ListBoxItem)SkinsListBox.SelectedItem;
            lbi.Content = newName;
        }

        private void SaveSkinAuthor(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var newName = SkinAuthorValueTextBox.Text;
                _dbHandler.set_skin_author(newName, SelectedSkinId);

                            MeteorCode.WriteToConsole("Skin author saved", 0);
            }
        }

        private void NameplateSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!LoadingNameplates)
            {
                ArrayList nameplates = new ArrayList {0};

                foreach (int s in _dbHandler.get_character_nameplates_id(SelectedCharacterId))
                {
                    nameplates.Add(s);
                }
                if (SkinNameplateValueComboBox.Items.Count > 1)
                {
                    int selectedNameplateId = (int)nameplates[SkinNameplateValueComboBox.SelectedIndex];

                    if (selectedNameplateId == 0)
                    {
                        _dbHandler.set_skin_nameplate(SelectedCharacterId, SelectedSlot, selectedNameplateId, ActiveWorkspace);
                                    MeteorCode.WriteToConsole("Nameplate dissociated", 0);
                    }
                    else
                    {
                        _dbHandler.set_skin_nameplate(SelectedCharacterId, SelectedSlot, selectedNameplateId, ActiveWorkspace);
                                    MeteorCode.WriteToConsole("Nameplate associated", 0);
                    }
                }
            }
        }

        private void ConvertSkin(object sender, RoutedEventArgs e)
        {
            _dbHandler.convert_skin("Converted skin", "", "", "", SelectedCharacterId, SelectedSlot);
            LoadSkinList(SelectedCharacterName);
                        MeteorCode.WriteToConsole("The skin was converted to a custom one", 3);
        }

        private void RestoreSkin()
        {
            _dbHandler.restore_default(SelectedSlot, SelectedCharacterId, _dbHandler.get_property("workspace"));
            LoadSkinList(SelectedCharacterName);
        }

        private void GoToAuthorPage(object sender, RoutedEventArgs e)
        {
            var gbUid = _dbHandler.get_skin_gb_uid(SelectedSkinId);
            Process.Start("http://gamebanana.com/members/" + gbUid);
        }

        private void StartForge(object sender, RoutedEventArgs e)
        {
            var modelpath = AppPath + "/filebank/skins/" + SelectedSkinId + "/models/body/cxx/model.nud";

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


        //Drag & Drop
        private void DragEnterModel(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void DragEnterCsp(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void DropModel(object sender, DragEventArgs e)
        {
            var folderlist = (string[])e.Data.GetData(DataFormats.FileDrop, false);


            //parsing selected index
            var slot = SkinsListBox.SelectedIndex + 1;

            //Checking if there is an actual skin selected
            if (slot == 0) return;

            var selectedSkin = new Skin(slot, SelectedSkinId, SelectedCharacterId, ActiveWorkspace, _dbHandler);
            if (folderlist != null)
            {
                foreach (var s in folderlist)
                    selectedSkin.get_models(s);
            }

            LoadSkinList(SelectedCharacterName);
            SkinsListBox.SelectedIndex = slot - 1;
            LoadSkinFiles();
        }

        private void DropCsp(object sender, DragEventArgs e)
        {
            var folderlist = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (folderlist == null) return;

            var attr = File.GetAttributes(folderlist[0]);
            //parsing selected index
            var slot = SkinsListBox.SelectedIndex + 1;
            var selectedSkin = new Skin(slot, SelectedSkinId, SelectedCharacterId, ActiveWorkspace, _dbHandler);
            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                //Checking if there is an actual skin selected
                if (slot != 0)
                {
                    foreach (var s in folderlist)
                        selectedSkin.get_csps(s);

                    LoadSkinList(SelectedCharacterName);
                    SkinsListBox.SelectedIndex = slot - 1;
                    LoadSkinFiles();
                }
            }

            else
            {
                //Checking if there is an actual skin selected
                if (slot != 0)
                {
                    foreach (var s in folderlist)
                        selectedSkin.add_csp_file(s);


                    LoadSkinList(SelectedCharacterName);
                    SkinsListBox.SelectedIndex = slot - 1;
                    LoadSkinFiles();
                }
            }
        }

        private void DeleteModel()
        {
            //parsing selected index
            var slot = SkinsListBox.SelectedIndex + 1;

            //Checking if there is an actual skin selected
            if (slot != 0)
            {
                var selectedSkin = new Skin(slot, SelectedSkinId, SelectedCharacterId, ActiveWorkspace, _dbHandler);
                var selectedModel = (ListBoxItem)ModelListBox.Items[ModelListBox.SelectedIndex];
                selectedSkin.remove_model(selectedModel.Content.ToString());
            }

            LoadSkinFiles();
        }

        private void DeleteCsp()
        {
            //parsing selected index
            var slot = SkinsListBox.SelectedIndex + 1;

            //Checking if there is an actual skin selected
            if (slot != 0)
            {
                var selectedSkin = new Skin(slot, SelectedSkinId, SelectedCharacterId, ActiveWorkspace, _dbHandler);
                var selectedCsp = (ListBoxItem)CspListBox.Items[CspListBox.SelectedIndex];
                selectedSkin.remove_csp_file(selectedCsp.Content.ToString());
            }
            LoadSkinFiles();
        }



        //Loads
        private void LoadSkinList(string selectedCharacter)
        {
            var skins = _dbHandler.get_character_skins(selectedCharacter, _dbHandler.get_property("workspace"));
            SkinsListBox.Items.Clear();
            foreach (string s in skins)
            {
                var lbi = new ListBoxItem {Content = s};
                var cm = new ContextMenu();
                var m1 = new MenuItem {Header = "Remove skin from workspace"};
                m1.Click += (s1, e) => { RemoveSkin(); };
                cm.Items.Add(m1);
                lbi.ContextMenu = cm;

                SkinsListBox.Items.Add(lbi);
            }
        }

        private void LoadSkinFiles()
        {
            //Loading models
            ModelListBox.Items.Clear();

            var models = _dbHandler.get_skin_models(SelectedSkinId);

            var cm = new ContextMenu();
            var m1 = new MenuItem {Header = "Remove model from skin"};
            m1.Click += (s1, e) => { DeleteModel(); };
            cm.Items.Add(m1);

            var cm2 = new ContextMenu();
            var m2 = new MenuItem {Header = "Remove csp from skin"};
            m2.Click += (s1, e) => { DeleteCsp(); };
            cm2.Items.Add(m2);
            foreach (var model in models)
                if (model != "")
                {
                    var lbi = new ListBoxItem
                    {
                        Content = model,
                        ContextMenu = cm
                    };
                    ModelListBox.Items.Add(lbi);
                }

            //Loading csps
            CspListBox.Items.Clear();

            var csps = _dbHandler.get_skin_csps(SelectedSkinId);
            foreach (var csp in csps)
                if (csp != "")
                {
                    var lbi = new ListBoxItem
                    {
                        Content = csp,
                        ContextMenu = cm2
                    };
                    CspListBox.Items.Add(lbi);
                }

            String nameplateName = _dbHandler.get_skin_nameplate(SelectedSlot, SelectedCharacterId, ActiveWorkspace);
            LoadingNameplates = true;
            if (nameplateName != "")
            {
                foreach (object t in SkinNameplateValueComboBox.Items)
                {
                    String cbi = (String)t;
                    if (cbi == nameplateName)
                    {
                        SkinNameplateValueComboBox.SelectedItem = cbi;
                    }
                }
            }
            else
            {
                SkinNameplateValueComboBox.SelectedIndex = 0;
            }

            LoadingNameplates = false;
        }


        //Interface Actions
        private void SelectCharacter(int characterId)
        {
            var count = 0;
            var slot = -1;
            foreach (string s in _dbHandler.get_characters(int.Parse(_dbHandler.get_property("sort_order"))))
            {
                if (characterId == _dbHandler.get_character_id(s))
                    slot = count;
                count++;
            }

            CharacterListView.SelectedIndex = slot;
        }

        private void SelectSkinSlot(int slot)
        {
            SkinsListBox.SelectedIndex = slot - 1;
        }

        private void ClearSkinControls()
        {
            //Emptying info
            SkinNameValueTextBox.Text = "";
            SkinSlotValueTextBox.Text = "";
            SkinAuthorValueTextBox.Text = "";

            //Emptying lists
            ModelListBox.Items.Clear();
            CspListBox.Items.Clear();

            //Hiding buttons
            ForgeButton.Visibility = Visibility.Hidden;
            AuthorButton.Visibility = Visibility.Hidden;
            ConvertButton.Visibility = Visibility.Hidden;
        }

        private void SetSkinState(bool workspaceLock, bool skinLock, int skinGbId)
        {
            //If Default Skin on Default Slot
            if (workspaceLock && skinLock)
            {
                //Showing Buttons
                ConvertButton.Visibility = Visibility.Visible;
                AuthorButton.Visibility = Visibility.Hidden;
                ForgeButton.Visibility = Visibility.Hidden;


                //Blocking fields
                SkinAuthorValueTextBox.IsReadOnly = true;
                SkinNameValueTextBox.IsReadOnly = true;
            }
            else
            {
                //Showing Buttons
                ConvertButton.Visibility = Visibility.Hidden;
                AuthorButton.Visibility = Visibility.Hidden;
                ForgeButton.Visibility = Visibility.Visible;

                if (skinGbId != 0)
                {
                    //Blocking fields
                    SkinAuthorValueTextBox.IsReadOnly = true;


                    if (skinGbId != -1)
                    {
                        //Making author visible
                        AuthorButton.Visibility = Visibility.Visible;

                        SkinNameValueTextBox.IsReadOnly = true;
                    }
                }
                else
                {
                    //enabling fields
                    SkinAuthorValueTextBox.IsReadOnly = false;
                    SkinNameValueTextBox.IsReadOnly = false;
                }
            }
        }

        private void FillCharacters()
        {
            var chars = _dbHandler.get_characters(int.Parse(_dbHandler.get_property("sort_order")));

            CharactersItemsCollections.Clear();

            foreach (string s in chars)
            {
                var characterName = s.Replace("&", "and");
                characterName = characterName.Replace(".", "");
                characterName = characterName == "All Miis" ? "Mii" : characterName;

                CharactersItemsCollections.Add(new CharactersListViewItemsData()
                {
                    CharacterImageSource = AppPath+"/images/CharacterIcon/"+ characterName + ".png",
                    CharacterLabelValue = s
                });
            }

            CharacterListView.ItemsSource = CharactersItemsCollections;
        }


        

    }

    public class CharactersListViewItemsData
    {
        public string CharacterImageSource { get; set; }
        public string CharacterLabelValue { get; set; }
    }
}
