using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GongSolutions.Wpf.DragDrop;
using Meteor.content;
using Meteor.database;
    
namespace Meteor.sections
{
    public partial class SkinsSection : IDropTarget
    {
        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;

        private readonly BackgroundWorker _LoadWorker = new BackgroundWorker();

        private MeteorDatabase meteorDatabase;

        public string SelectedCharacterName { get; set; }
        private int SelectedCharacterId { get; set;  }
        private int SelectedSkinId { get; set; }
        public int SelectedSlot { get; set; }

        private int ActiveWorkspace { get; set; }

        private bool LoadingNameplates { get; set; }
        private bool SlotSelectedStatus;
        private bool SkinSelectedStatus;

        private bool loading;
        private bool dropped;

        ListBox dragSource = null;

        private ObservableCollection<CharactersListViewItemsData> CharactersItemsCollections { get; } = new ObservableCollection<CharactersListViewItemsData>();
        private ObservableCollection<SkinListBoxItemData> CustomSkinItems { get; set; } = new ObservableCollection<SkinListBoxItemData>();
        private ObservableCollection<SkinListBoxItemData> SlotItems { get; set; } = new ObservableCollection<SkinListBoxItemData>();

        //Constructor
        public SkinsSection()
        {
            InitializeComponent();

            meteorDatabase = new MeteorDatabase();

            LoadCharacters();

            CustomSkinItems.CollectionChanged += (SkinCollectionChanged);
            SlotItems.CollectionChanged += (SlotCollectionChanged);

            _LoadWorker.DoWork += LoadWorkerDoWork;
            _LoadWorker.RunWorkerCompleted += LoadWorkerCompleted;
            _LoadWorker.ProgressChanged += LoadWorkerProgress;
            _LoadWorker.WorkerReportsProgress = true;

            _LoadWorker.RunWorkerAsync();

            loading = false;
            dropped = false;
        }

        void SkinCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!loading)
            {
                int countSlot = 1;
                foreach (SkinListBoxItemData skinListBoxItemData in SlotItems)
                {
                    SkinLibrary entry = meteorDatabase.SkinLibraries
                        .First(sl => sl.slot == countSlot && sl.workspace_id == ActiveWorkspace &&
                                     sl.character_id == SelectedCharacterId);

                    if (entry != null)
                    {
                        meteorDatabase.SkinLibraries.Add(new SkinLibrary()
                        {
                            skin_id = skinListBoxItemData.SkinId,
                            character_id = SelectedCharacterId,
                            workspace_id = ActiveWorkspace,
                            slot = countSlot
                        });
                    }
                    else
                    {
                        entry.Id = skinListBoxItemData.SkinId;
                    }
                    countSlot++;
                }
                meteorDatabase.SaveChanges();
                dropped = true;
            }
        }
        void SlotCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!loading)
            {
                int countSlot = 1;
                bool inserted = false;
                foreach (SkinListBoxItemData skinListBoxItemData in SlotItems)
                {
                    //Getting Skin ID
                    Skin skin = meteorDatabase.Skins.First(s => s.Id == skinListBoxItemData.SkinId);

                    //Getting max slot
                    int maxSlot =
                        meteorDatabase.SkinLibraries.Count(
                            m => m.character_id == SelectedCharacterId && m.workspace_id == ActiveWorkspace);

                    int defaultCount =
                        meteorDatabase.SkinLibraries.Count(
                            sl => sl.workspace_id == 1 && sl.character_id == SelectedCharacterId);

                    // if current slot is default
                    if (countSlot <= defaultCount)
                    {
                        //If the item was dropped
                        if (skinListBoxItemData.isSkin)
                        {
                            inserted = true;
                            SkinLibrary entry = meteorDatabase.SkinLibraries
                                .First(sl => sl.slot == countSlot && sl.workspace_id == ActiveWorkspace &&
                                             sl.character_id == SelectedCharacterId);
                            entry.skin_id = skinListBoxItemData.SkinId;
                            countSlot++;
                        }
                        //If item was already there
                        else
                        {
                            //Ignores when the skin is default and after an inserted one
                            if (!(inserted && (bool) skin.skinLock))
                            {
                                SkinLibrary entry = meteorDatabase.SkinLibraries
                                    .First(sl => sl.slot == countSlot && sl.workspace_id == ActiveWorkspace &&
                                                 sl.character_id == SelectedCharacterId);
                                entry.skin_id = skinListBoxItemData.SkinId;
                                countSlot++;
                            }
                            else
                            {
                                inserted = false;
                            }
                        }
                        
                    }
                    //Additionnal slot
                    else
                    {
                        
                        //New entry
                        if (countSlot > maxSlot)
                        {
                            if (skin.skinLock == false)
                            {
                                SkinLibrary newEntry = new SkinLibrary()
                                {
                                    skin_id = skin.Id,
                                    character_id = SelectedCharacterId,
                                    slot = countSlot,
                                    workspace_id = ActiveWorkspace
                                };
                                meteorDatabase.SkinLibraries.Add(newEntry);
                            }
                        }
                        //Existing entry
                        else
                        {
                            SkinLibrary entry = meteorDatabase.SkinLibraries
                                .First(sl => sl.slot == countSlot && sl.workspace_id == ActiveWorkspace &&
                                             sl.character_id == SelectedCharacterId);


                            if (entry.Skin.skinLock == true)
                            {
                                meteorDatabase.SkinLibraries.Remove(entry);
                                ReorderSkins(entry.character_id);
                            }
                            else
                            {
                                entry.skin_id = skinListBoxItemData.SkinId;
                            }
                        }
                        countSlot++;
                    }
                }
                meteorDatabase.SaveChanges();
                dropped = true;
            }
        }


        //Character List
        private void CharacterSelected(object sender, SelectionChangedEventArgs e)
        {
            ActiveWorkspace = int.Parse(meteorDatabase.Configurations.First(c => c.property == "activeWorkspace")
                .value);

            //Getting selected item
            var li = (CharactersListViewItemsData)CharacterListView.SelectedItem;
            if (li != null)
            {
                //Setup global values
                SelectedCharacterId = li.Itemid;

                //Load the skin list for that selected character
                LoadLists();

                //Writing text on the console, for dev
                            MeteorCode.WriteToConsole("Selected char name " + SelectedCharacterName, 3);
                            MeteorCode.WriteToConsole("Selected char id " + SelectedCharacterId, 3);
                //DisplayTabControl.SelectedIndex = 1;
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
            if(SkinsListBox.SelectedIndex != -1)
            {
                SkinSelectedStatus = true;
                SlotSelectedStatus = false;
                RefreshInformations(0);
                DisplayTabControl.SelectedIndex = 0;
            }
           
        }

        private void SlotSelected(object sender, SelectionChangedEventArgs e)
        {
            if (SlotListBox.SelectedIndex != -1)
            {
                try
                {
                    SkinSelectedStatus = false;
                    SlotSelectedStatus = true;
                    SelectedSlot = SlotListBox.SelectedIndex + 1;
                    RefreshInformations(1);
                    DisplayTabControl.SelectedIndex = 0;
                }
                catch
                {
                    errorSlots();
                }
               
            }
            
        }

        private void AddSkin(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedCharacterId > 0)
                {
                    Skin skin = new Skin()
                    {
                        name = "New Skin",
                        character_id = SelectedCharacterId,
                        skinLock = false,
                        skin_models = "",
                        skin_csps = "",
                        gb_uid = 0
                    };
                    meteorDatabase.Skins.Add(skin);
                    meteorDatabase.Entry(skin).State = EntityState.Added;
                    meteorDatabase.SaveChanges();

                    LoadLists();
                    SkinsListBox.SelectedIndex = SkinsListBox.Items.Count - 1;
                    SkinNameValueTextBox.Focus();
                    SkinNameValueTextBox.Select(0,SkinNameValueTextBox.Text.Length);
                }
                
            }
            catch (Exception ee)
            {
                        MeteorCode.WriteToConsole("There was an error while adding the skin", 2);
                        MeteorCode.Paste(ee.Message, ee.StackTrace);
            }


        }

        private void RemoveSkin()
        {
            SkinLibrary entry = meteorDatabase.SkinLibraries
                .First(sl => sl.slot == SlotListBox.SelectedIndex + 1 && sl.character_id == SelectedCharacterId);
            Skin skin = entry.Skin;

            if (entry.library_lock == false)
            {
                meteorDatabase.SkinLibraries.Remove(entry);
                ReorderSkins(SelectedCharacterId);
                LoadLists();
                            MeteorCode.WriteToConsole("Skin removed from the workspace", 0);
            }
            else
            {
                if (skin.skinLock == true)
                    RestoreSkin();
                else
                    MeteorCode.WriteToConsole("Sorry, you cannot remove default skin slots!", 1);
            }
        }


        //Skin Information
        private void SaveSkinName(object sender, KeyEventArgs e)
        {
            var slot = SlotSelectedStatus ? SlotListBox.SelectedIndex : SkinsListBox.SelectedIndex;
            if (e.Key != Key.Enter) return;

            Skin skin = meteorDatabase.Skins.First(s => s.Id == SelectedSkinId);
            skin.name = SkinNameValueTextBox.Text;
            meteorDatabase.SaveChanges();
            LoadLists();
            
            if (SlotSelectedStatus)
            {
                SlotListBox.SelectedIndex = slot;
            }
            else
            {
                SkinsListBox.SelectedIndex = slot;
            }
        }

        private void SaveSkinAuthor(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Skin skin = meteorDatabase.Skins.First(s => s.Id == SelectedSkinId);
                skin.author = SkinAuthorValueTextBox.Text;
                meteorDatabase.SaveChanges();
            }
        }

        private void NameplateSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!LoadingNameplates)
            {
                ArrayList nameplates = new ArrayList {0};

                foreach (Nameplate nameplate in meteorDatabase.Characters.First(c => c.Id == SelectedCharacterId).Nameplates)
                {
                    nameplates.Add(nameplate.Id);
                }
                if (SkinNameplateValueComboBox.Items.Count > 1)
                {
                    int selectedNameplateId = (int)nameplates[SkinNameplateValueComboBox.SelectedIndex];

                    SkinLibrary entry =
                        meteorDatabase.SkinLibraries.First(
                            sl => sl.slot == SelectedSlot && sl.character_id == SelectedCharacterId &&
                                  sl.workspace_id == ActiveWorkspace);
                    entry.nameplate_id = selectedNameplateId;
                    meteorDatabase.SaveChanges();
                }
            }
        }

        private void ConvertSkin(object sender, RoutedEventArgs e)
        {
            SkinLibrary entry =
                meteorDatabase.SkinLibraries.First(sl => sl.slot == SelectedSlot && sl.workspace_id == ActiveWorkspace && sl.character_id == SelectedCharacterId);
            Skin newSkin = new Skin()
            {
                name = "Converted Skin",
                character_id = entry.character_id,
                skin_models = "",
                skin_csps = "",
                gb_uid = 0,
                skinLock = false
            };
            meteorDatabase.Skins.Add(newSkin);
            entry.skin_id = newSkin.Id;
            meteorDatabase.SaveChanges();
            LoadLists();
            SlotListBox.SelectedIndex = SelectedSlot-1;

        }

        private void RestoreSkin()
        {
            var slot = SlotListBox.SelectedIndex +1;
            SkinLibrary entry =
                meteorDatabase.SkinLibraries.First(sl => sl.slot == slot && sl.workspace_id == ActiveWorkspace && sl.character_id == SelectedCharacterId);
            entry.skin_id = meteorDatabase.SkinLibraries
                .First(sl => sl.slot == slot && sl.character_id == entry.character_id && sl.workspace_id == 1).Id;
            meteorDatabase.SaveChanges();
            LoadLists();
            SlotListBox.SelectedIndex = slot;
        }

        private void InsertSkin()
        {
            Skin skin = meteorDatabase.Skins.First(s => s.Id == SelectedSkinId);
            Character character = skin.Character;

            var count = character.SkinLibraries.Count(sl => sl.workspace_id == ActiveWorkspace);

            meteorDatabase.SkinLibraries.Add(new SkinLibrary()
            {
                skin_id = skin.Id,
                character_id = character.Id,
                slot = count+1,
                workspace_id = ActiveWorkspace
            });
            meteorDatabase.SaveChanges();

            LoadLists();
            SlotListBox.SelectedIndex = count-1;
        }

        private void GoToAuthorPage(object sender, RoutedEventArgs e)
        {
            
            var gbUid = meteorDatabase.Skins.First(s => s.Id == SelectedSkinId).gb_uid;
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
            var slot = SlotSelectedStatus ? SlotListBox.SelectedIndex + 1 : SkinsListBox.SelectedIndex +1;


            //Checking if there is an actual skin selected


            var selectedSkin = new SkinObject(slot, SelectedSkinId, SelectedCharacterId, ActiveWorkspace, meteorDatabase);

            if (folderlist != null)
            {
                foreach (var s in folderlist)
                    selectedSkin.AddModelsFromPath(s);
            }

            LoadLists();

            if (SlotSelectedStatus)
            {
                SlotListBox.SelectedIndex = slot - 1;
            }
            else
            {
                SkinsListBox.SelectedIndex = slot - 1;
            }
            
            LoadSkinFiles();
        }

        private void DropCsp(object sender, DragEventArgs e)
        {
            var folderlist = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (folderlist == null) return;

            var attr = File.GetAttributes(folderlist[0]);

            //parsing selected index
            var slot = SlotSelectedStatus ? SlotListBox.SelectedIndex + 1 : SkinsListBox.SelectedIndex + 1;

            var selectedSkin = new SkinObject(slot, SelectedSkinId, SelectedCharacterId, ActiveWorkspace, meteorDatabase);
            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                //Checking if there is an actual skin selected
                if (slot != 0)
                {
                    foreach (var s in folderlist)
                        selectedSkin.AddCspsFromPath(s);

                    LoadLists();
                    if (SlotSelectedStatus)
                    {
                        SlotListBox.SelectedIndex = slot - 1;
                    }
                    else
                    {
                        SkinsListBox.SelectedIndex = slot - 1;
                    }
                    LoadSkinFiles();
                }
            }

            else
            {
                //Checking if there is an actual skin selected
                if (slot != 0)
                {
                    foreach (var s in folderlist)
                        selectedSkin.AddCspFile(s);
                    LoadLists();
                    if (SlotSelectedStatus)
                    {
                        SlotListBox.SelectedIndex = slot - 1;
                    }
                    else
                    {
                        SkinsListBox.SelectedIndex = slot - 1;
                    }
                    LoadSkinFiles();
                }
            }
        }

        private void DeleteModel()
        {
            //parsing selected index
            var slot = SelectedSlot;

            //Checking if there is an actual skin selected
            if (slot != 0)
            {
                var selectedSkin = new SkinObject(slot, SelectedSkinId, SelectedCharacterId, ActiveWorkspace, meteorDatabase);
                var selectedModel = (ListBoxItem)ModelListBox.Items[ModelListBox.SelectedIndex];
                selectedSkin.RemoveModel(selectedModel.Content.ToString());
            }

            LoadSkinFiles();
        }

        private void DeleteCsp()
        {
            //parsing selected index
            var slot = SelectedSlot;

            //Checking if there is an actual skin selected
            if (slot != 0)
            {
                var selectedSkin = new SkinObject(slot, SelectedSkinId, SelectedCharacterId, ActiveWorkspace, meteorDatabase);
                var selectedCsp = (ListBoxItem)CspListBox.Items[CspListBox.SelectedIndex];
                selectedSkin.RemoveCspFile(selectedCsp.Content.ToString());
            }
            LoadSkinFiles();
        }


        //Loads
        public void LoadCharacters()
        {
            var chars = meteorDatabase.Characters;

            CharactersItemsCollections.Clear();

            foreach (Character character in chars)
            {
                var characterName = character.name.Replace("&", "and");
                characterName = characterName.Replace(".", "");
                characterName = characterName == "All Miis" ? "Mii" : characterName;

                CharactersItemsCollections.Add(new CharactersListViewItemsData()
                {
                    CharacterImageSource = AppPath + "/images/CharacterIcon/" + characterName + ".png",
                    CharacterLabelValue = character.name,
                    Itemid = character.Id

                });
            }

            CharacterListView.ItemsSource = CharactersItemsCollections;
        }

        public void LoadLists()
        {
            loading = true;
            //Loading character skins
            
            LoadSkinList();
            LoadSlotList();
           
            loading = false;
        }

        public void LoadSkinList()
        {
            meteorDatabase = new MeteorDatabase();
            int workspaceId = int.Parse(meteorDatabase.Configurations.First(c => c.property == "activeWorkspace")
                .value);

            ObservableCollection<SkinListBoxItemData> TempCustomSkinItems = new ObservableCollection<SkinListBoxItemData>();
            foreach (Skin skin in meteorDatabase.Skins.Where(s => s.character_id == SelectedCharacterId && s.skinLock == false))
            {
                var cm = new ContextMenu();
                var m1 = new MenuItem { Header = "Insert skin in last slot" };
                m1.Click += (s1, e) => { InsertSkin(); };
                cm.Items.Add(m1);
                TempCustomSkinItems.Add(new SkinListBoxItemData() { SkinId = skin.Id, isSkin = true, Content = skin.name, ContextMenu = cm, ToolTip = "Select this to display it's information and edit the files" });
            }

            CustomSkinItems = TempCustomSkinItems;
            SkinsListBox.ItemsSource = CustomSkinItems;

        }

        public void LoadSlotList()
        {
            try
            {
                meteorDatabase = new MeteorDatabase();
                int workspaceId = int.Parse(meteorDatabase.Configurations.First(c => c.property == "activeWorkspace")
                    .value);

                SlotItems.Clear();
                //Loading character slots
                int DSCount = 1;
                int ASCount = 1;
                foreach (SkinLibrary entry in meteorDatabase.SkinLibraries.Where(
                    sl => sl.workspace_id == workspaceId && sl.character_id == SelectedCharacterId))
                {

                    if (entry.library_lock)
                    {
                        var cm = new ContextMenu();
                        var m1 = new MenuItem {Header = "Remove skin from slots"};
                        m1.Click += (s1, e) => { RemoveSkin(); };
                        cm.Items.Add(m1);

                        SlotItems.Add(new SkinListBoxItemData()
                        {
                            SkinId = entry.Skin.Id,
                            isSkin = false,
                            Content = "DS" + DSCount + " - " + entry.Skin.name,
                            ContextMenu = cm,
                            ToolTip = "Select this to display it's information and edit the files"
                        });
                        DSCount++;
                    }
                    else
                    {
                        var cm = new ContextMenu();
                        var m1 = new MenuItem {Header = "Remove skin from slots"};
                        m1.Click += (s1, e) => { RemoveSkin(); };
                        cm.Items.Add(m1);
                        SlotItems.Add(new SkinListBoxItemData()
                        {
                            SkinId = entry.Skin.Id,
                            isSkin = false,
                            Content = "AS" + ASCount + " - " + entry.Skin.name,
                            ContextMenu = cm,
                            ToolTip = "Select this to display it's information and edit the files"
                        });
                        ASCount++;
                    }
                }

                SlotListBox.ItemsSource = SlotItems;
            }
            catch
            {
                errorSlots();
            }
        }

        private void LoadSkinFiles()
        {
            //Loading models
            ModelListBox.Items.Clear();
            Skin skin = meteorDatabase.Skins.First(s => s.Id == SelectedSkinId);
         

            var cm = new ContextMenu();
            var m1 = new MenuItem {Header = "Remove model from skin"};
            m1.Click += (s1, e) => { DeleteModel(); };
            cm.Items.Add(m1);

            var cm2 = new ContextMenu();
            var m2 = new MenuItem {Header = "Remove csp from skin"};
            m2.Click += (s1, e) => { DeleteCsp(); };
            cm2.Items.Add(m2);
            foreach (var model in skin.skin_models.Split(';'))
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

            foreach (var csp in skin.skin_csps.Split(';'))
                if (csp != "")
                {
                    var lbi = new ListBoxItem
                    {
                        Content = csp,
                        ContextMenu = cm2
                    };
                    CspListBox.Items.Add(lbi);
                }

            LoadingNameplates = true;
            if (SlotSelectedStatus)
            {
                
                SkinLibrary entry =
                    meteorDatabase.SkinLibraries.First(sl => sl.workspace_id == ActiveWorkspace &&
                                                             sl.character_id == SelectedCharacterId &&
                                                             sl.slot == SelectedSlot);
                Nameplate nameplate = entry.Nameplate;

                if (nameplate != null)
                {
                    foreach (object t in SkinNameplateValueComboBox.Items)
                    {
                        String cbi = (String) t;
                        if (cbi == nameplate.name)
                        {
                            SkinNameplateValueComboBox.SelectedItem = cbi;
                        }
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

        public void RefreshInformations(int list)
        {
            //If no skin is selected
            if (SkinsListBox.SelectedIndex == -1 && SlotListBox.SelectedIndex == -1)
            {
                ClearSkinControls();
            }
            else
            {
                ActiveWorkspace = int.Parse(meteorDatabase.Configurations.First(c => c.property == "activeWorkspace").value);
                SkinListBoxItemData item = null;
                if (list == 0)
                {
                    item = (SkinListBoxItemData)SkinsListBox.SelectedItems[0];
                }
                else
                {
                    item = (SkinListBoxItemData)SlotListBox.SelectedItems[0];
                }

                //Setting up global variables
                SelectedSlot = list == 0 ? SkinsListBox.SelectedIndex + 1 : SlotListBox.SelectedIndex + 1;
                SelectedSkinId = item.SkinId;
                MeteorCode.Message("Skin ID : " + item.SkinId);
                Skin skin = meteorDatabase.Skins.First(s => s.Id == SelectedSkinId);

                //Getting id
                if (skin != null)
                {
                    int gbUid = -1;
                    if (skin.gb_uid != null)
                    {
                        gbUid = (int) skin.gb_uid;
                    }
                    if (SelectedSlot > 0)
                    {
                        //Loading info
                        SkinNameValueTextBox.Text = skin.name;
                        SkinAuthorValueTextBox.Text = skin.author;
                        SkinSlotValueTextBox.Text = list == 1? SelectedSlot.ToString() : "Filebank";


                        //Getting locks
                        bool skinLock = skin.skinLock != null && (bool)skin.skinLock;
                        var workspaceLock = false;
                        if (SlotSelectedStatus)
                        {
                            workspaceLock = meteorDatabase.SkinLibraries.FirstOrDefault(sl => sl.workspace_id == ActiveWorkspace && sl.character_id == skin.character_id && sl.slot == SelectedSlot).library_lock;
                        }

                        //Setting control states
                        SetSkinState(workspaceLock, skinLock, gbUid);

                        if (SelectedSlot < 17)
                        {
                            LoadingNameplates = true;
                            SkinNameplateValueComboBox.Items.Clear();
                            SkinNameplateValueComboBox.Items.Add("No Nameplate");
                            foreach (Nameplate nameplate in meteorDatabase.Characters.First(c=> c.Id == skin.character_id).Nameplates)
                            {
                                SkinNameplateValueComboBox.Items.Add(nameplate.name);
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
                }

                //Writing stuff
                MeteorCode.WriteToConsole("Selected slot " + SelectedSlot, 3);
                MeteorCode.WriteToConsole("Selected skin id " + SelectedSkinId, 3);
            }
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

        

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            SkinListBoxItemData sourceItem = dropInfo.Data as SkinListBoxItemData;
            SkinListBoxItemData targetItem = dropInfo.TargetItem as SkinListBoxItemData;

            if (sourceItem != null && targetItem != null && targetItem.CanAcceptChildren)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            LoadLists();
        }

        private void CheckDrop(object sender, MouseButtonEventArgs e)
        {
                LoadLists();
                dropped = false;
        }

        //Workers
        private void LoadWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            bool test = false;
            while (!test)
            {
                if (dropped)
                {
                    test = true;

                }
            }
        }

        private void LoadWorkerProgress(object sender, ProgressChangedEventArgs e)
        {
          
        }

        private void LoadWorkerCompleted(object sender, AsyncCompletedEventArgs e)
        {
            LoadLists();
            _LoadWorker.RunWorkerAsync();
            dropped = false;
        }

        private void ReorderSkins(int characterId)
        {
            int slotCount = 1;
            foreach (SkinLibrary entry in meteorDatabase.SkinLibraries.Where(
                sl => sl.character_id == characterId && sl.workspace_id == ActiveWorkspace))
            {
                entry.slot = slotCount;
                slotCount++;
            }
            meteorDatabase.SaveChanges();
        }

        private void errorSlots()
        {
            var result = MessageBox.Show("An error has been detected with this character's slots. Reset them?", "Segtendo WARNING",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                foreach (Character chara in meteorDatabase.Characters)
                {
                    ReorderSkins(chara.Id);
                }
            }
            if (result == MessageBoxResult.Cancel)
            {
                ReorderSkins(SelectedCharacterId);
            }
        }
    }

    public class CharactersListViewItemsData 
    {
        public string CharacterImageSource { get; set; }
        public string CharacterLabelValue { get; set; }
        public int Itemid { get; set; }
    }

    public class SkinListBoxItemData  : ListBoxItem
    {
        public int SkinId { get; set; }
        public string SkinLabelValue { get; set; }
        public bool isSkin { get; set; }
        public bool CanAcceptChildren { get; set; }
        public ObservableCollection<SkinListBoxItemData> Children { get; }
    }


}
