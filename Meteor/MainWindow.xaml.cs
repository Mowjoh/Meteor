using Meteor.content;
using Meteor.database;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using winforms = System.Windows.Forms;
using newpath = System.IO.Path;
using SharpCompress.Reader;
using SharpCompress.Common;
using Microsoft.Win32;

namespace Meteor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Application Variables
        #region Variables
        //Application vars
        String[] args;
        String app_path = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName;
        Mutex mutex;

        String download_resource_link = "";

        //Skin Character
        String selected_char_name = "";
        int selected_char_id = -1;
        int install_char = -1;

        //Skin List
        int selected_skin_id = -1;
        int selected_slot = -1;
        Boolean selected_skin_workspace_lock = true;
        Boolean selected_skin_author_lock = true;

        int install_skin_slot;

        //Workspace
        int selected_workspace = -1;
        int active_workspace = -1;
        int active_workspace_slot = -1;
        int sel_work_thread = -1;

        //Handlers
        uichar_handler uichar;
        builder build;
        db_handler db;

        //Configuration options
        Boolean uipresent = false;

        //Tab Values
        ArrayList filebank_skins = new ArrayList();

        //Workers
        private readonly BackgroundWorker workspace_worker = new BackgroundWorker();
        private readonly BackgroundWorker build_worker = new BackgroundWorker();
        private readonly BackgroundWorker url_worker = new BackgroundWorker();
        private readonly BackgroundWorker download_worker = new BackgroundWorker();

        int workspace_action = -1;
        String msl_path = "";

        //Web Client
        WebClient webClient = new WebClient();

        #endregion

        //Application constructor
        #region Constructor
        //Launch
        public MainWindow()
        {
            Directory.SetCurrentDirectory(app_path);
            mutexcheck(Environment.GetCommandLineArgs());

            InitializeComponent();


            //Init functions
            #region Inits
            try
            {
                db = new db_handler();
                Write_Console("The connection to the database was successful.", 3);
            }
            catch
            {
                Write_Console("The connection to the database was unsuccessful. Please check that the Library is there.", 2);
            }

            fill_lists();
            retreive_config();
            Console.Text = "";

            uichar = new uichar_handler();
            if (File.Exists(uichar.filepath))
            {
                uipresent = true;
            }

            int region = int.Parse(db.get_property("region"));
            int language = int.Parse(db.get_property("language"));
            int workspace = int.Parse(db.get_property("workspace"));

            this.build = new builder(workspace, region, language, db, uichar);
            setup_workers();




            Write_Console("Welcome to Meteor !", 0);
            #endregion

            check_update();

            this.args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                Write_Console(args[1], 0);
                download(args[1]);
            }


        }
        #endregion

        //Init procedures
        #region Inits
        private void fill_lists()
        {
            ArrayList chars = db.get_characters(int.Parse(db.get_property("sort_order")));
            CharacterListBox.Items.Clear();
            foreach (String s in chars)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s;
                CharacterListBox.Items.Add(lbi);
            }

            reload_workspaces_list();
            load_skin_bank();

        }
        private void retreive_config()
        {
            //Region Select
            combo_region.SelectedIndex = int.Parse(db.get_property("region"));
            combo_language.SelectedIndex = int.Parse(db.get_property("language"));

            //S4E path
            config_s4e_path.Text = db.get_property("s4e_exe");

            //Config
            config_username.Text = db.get_property("username");
            config_sortby.SelectedIndex = int.Parse(db.get_property("sort_order"));
            config_ui_rebuild.IsChecked = db.get_property("reload_ui") == "1" ? true : false;
            config_S4E.IsChecked = db.get_property("s4e_launch") == "1" ? true : false;

            //Dev Options
            config_devlogs.IsChecked = db.get_property("dev_logs") == "1" ? true : false;

            //GB Options
            config_gbuser.Text = db.get_property("gb_uid");

            //ui_char status
            ui_char_status.Content = db.get_property("ui_char") == "1" ? "Status : imported" : "Status : not present";

            //Workspace
            active_workspace = int.Parse(db.get_property("workspace"));
            active_workspace_slot = db.get_workspace_slot(active_workspace);
            active_workspace_name_label.Content = db.get_workspace_name(active_workspace);

        }

        private void mutexcheck(String[] args)
        {

            Mutex mt;
            if (Mutex.TryOpenExisting("meteor_mutex", out mt))
            {
                if (!Directory.Exists(app_path + "/downloads/"))
                {
                    Directory.CreateDirectory(app_path + "/downloads/");
                }
                else
                {
                    Directory.Delete(app_path + "/downloads/", true);
                    Directory.CreateDirectory(app_path + "/downloads/");
                }

                System.IO.File.WriteAllLines(app_path + "/downloads/url.txt", args);

                Mutex test = new Mutex(true, "meteor_url");
                Application.Current.Shutdown();
            }
            else
            {
                this.mutex = new Mutex(true, "meteor_mutex");
            }
        }
        #endregion

        //Interface functions
        #region Interface

        #region Locks & Visibility
        public void clear_skin_view()
        {

            //Emptying info
            skins_skin_name.Text = "";
            skins_slot.Text = "";
            skins_author.Text = "";

            //Emptying lists
            skin_modelbox.Items.Clear();
            skin_cspbox.Items.Clear();

            //Hiding buttons
            forge_button.Visibility = Visibility.Hidden;
            author_button.Visibility = Visibility.Hidden;
            convert_button.Visibility = Visibility.Hidden;
        }

        public void set_skin_state(Boolean workspace_lock, Boolean skin_lock, int skin_gb_id)
        {
            //If Default Skin on Default Slot
            if (workspace_lock && skin_lock)
            {
                //Showing Buttons
                convert_button.Visibility = Visibility.Visible;
                author_button.Visibility = Visibility.Hidden;
                forge_button.Visibility = Visibility.Hidden;


                //Blocking fields
                skins_author.IsReadOnly = true;
                skins_skin_name.IsReadOnly = true;
            }
            else
            {

                //Showing Buttons
                convert_button.Visibility = Visibility.Hidden;
                author_button.Visibility = Visibility.Hidden;
                forge_button.Visibility = Visibility.Visible;

                if (skin_gb_id != 0)
                {
                    //Blocking fields
                    skins_author.IsReadOnly = true;


                    if (skin_gb_id != -1)
                    {
                        //Making author visible
                        author_button.Visibility = Visibility.Visible;

                        skins_skin_name.IsReadOnly = true;
                    }
                }
                else
                {
                    //enabling fields
                    skins_author.IsReadOnly = false;
                    skins_skin_name.IsReadOnly = false;
                }


            }

        }


        public void lock_grids()
        {
            skin_grid.IsEnabled = false;
            stage_grid.IsEnabled = false;
            config_grid.IsEnabled = false;
            interface_grid.IsEnabled = false;
            workspace_grid.IsEnabled = false;
            filebank_grid.IsEnabled = false;

        }

        public void unlock_grids()
        {
            skin_grid.IsEnabled = true;
            stage_grid.IsEnabled = true;
            config_grid.IsEnabled = true;
            interface_grid.IsEnabled = true;
            workspace_grid.IsEnabled = true;
            filebank_grid.IsEnabled = true;
        }
        #endregion

        #region Processing
        private void change_statusbars(int mode, int mode2)
        {
            switch (mode)
            {
                case 0:
                    statusbar.IsIndeterminate = true;
                    break;
                case 1:
                    statusbar.IsIndeterminate = false;
                    statusbar.Value = 0;
                    break;
                case 2:
                    statusbar.IsIndeterminate = false;
                    statusbar.Value = 100;
                    break;
            }

            switch (mode2)
            {
                case 0:
                    operationbar.IsIndeterminate = true;
                    break;
                case 1:
                    operationbar.IsIndeterminate = false;
                    operationbar.Value = 0;
                    break;
                case 2:
                    operationbar.IsIndeterminate = false;
                    operationbar.Value = 100;
                    break;
            }
        }
        private void change_status(String text)
        {
            app_status_text.Content = text;
        }
        private void change_operation(String text)
        {
            operation_status_text.Content = text;
        }
        private void update_progress(int val, int val2)
        {
            statusbar.Value = val;
            operationbar.Value = val2;
        }
        #endregion

        #endregion

        //Console actions
        #region Console
        private void Write_Console(String s, int type)
        {
            String type_text = "";
            String date = DateTime.Now.ToString();
            switch (type)
            {
                case 0:
                    type_text = "Success";
                    break;
                case 1:
                    type_text = "Warning";
                    break;
                case 2:
                    type_text = "Error";
                    break;
                case 3:
                    type_text = "Dev Log";
                    break;
            }

            if (type != 3)
            {
                Console.Text = date + " | " + type_text + " | " + s + "\n" + Console.Text;
            }
            else
            {
                if (db.get_property("dev_logs") == "1")
                {
                    Console.Text = date + " | " + type_text + " | " + s + "\n" + Console.Text;
                }
            }

        }

        private void report()
        {

        }

        #endregion

        //Section actions
        #region Sections

        //When a section is selected
        private void section_change(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem li = (ListBoxItem)Section_List.SelectedItem;
            String val = li.Content.ToString();

            switch (val)
            {
                case "Skins":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[0];
                    Write_Console("Section changed to Skins", 3);
                    if (selected_char_name != "")
                    {
                        load_skin_list(selected_char_name);
                    }
                    break;
                case "Stages":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[1];
                    Write_Console("Section changed to Stages", 3);
                    break;
                case "Interface":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[2];
                    Write_Console("Section changed to Interface", 3);
                    break;
                case "FileBank":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[3];
                    Write_Console("Section changed to Filebank", 3);

                    break;
                case "Workspace":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[4];
                    Write_Console("Section changed to Workspace", 3);
                    if (workspace_listbox.Items.Count > 0)
                    {
                        try
                        {
                            workspace_listbox.SelectedIndex = this.active_workspace_slot - 2;
                        }
                        catch
                        {
                            workspace_listbox.SelectedIndex = 0;
                        }
                    }

                    load_workspace_stats();

                    break;
                case "Configuration":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[5];
                    Write_Console("Section changed to Configuration", 3);
                    break;
                case "About":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[6];
                    Write_Console("Section changed to About", 3);
                    break;
            }
        }

        //Skin Code
        #region Skins

        //Interface actions, Selects, Reloads
        #region Interface

        //-----------------------------------Selects---------------------------------------------------

        //When the character is selected
        private void character_selected(object sender, SelectionChangedEventArgs e)
        {
            //Getting selected item
            ListBoxItem li = (ListBoxItem)CharacterListBox.SelectedItem;
            if (li != null)
            {
                //Setup global values
                selected_char_name = li.Content.ToString();
                selected_char_id = db.get_character_id(selected_char_name);

                //Load the skin list for that selected character
                load_skin_list(selected_char_name);

                //Writing text on the console, for dev
                Write_Console("Selected char name " + selected_char_name, 3);
                Write_Console("Selected char id " + selected_char_id, 3);

            }
            //No character selected
            else
            {
                //Resetting global var
                selected_char_name = "";
                selected_char_id = -1;
            }


        }

        //When a skin is selected
        private void skin_selected(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem li = (ListBoxItem)CharacterListBox.SelectedItem;

            //If no skin is selected
            if (Skinslistbox.SelectedIndex == -1)
            {
                clear_skin_view();
            }
            else
            {
                //Setting up global variables
                selected_slot = Skinslistbox.SelectedIndex + 1;
                selected_skin_id = db.get_skin_id(selected_char_id, selected_slot, active_workspace);

                //Getting id
                int gb_uid = db.get_skin_gb_uid(selected_skin_id);

                if (selected_slot > 0)
                {
                    //Loading info
                    String[] infos = db.get_skin_info(selected_skin_id);
                    skins_skin_name.Text = infos[0];
                    skins_author.Text = infos[1];
                    skins_slot.Text = selected_slot.ToString();

                    //Getting locks
                    Boolean skin_lock = db.skin_locked(selected_skin_id);
                    Boolean workspace_lock = db.skin_locked(selected_slot, selected_char_id);

                    //Setting control states
                    set_skin_state(workspace_lock, skin_lock, gb_uid);

                    //loading skin files
                    load_skin_files();

                }

                //Writing stuff
                Write_Console("Selected slot " + selected_slot, 3);
                Write_Console("Selected skin id " + selected_skin_id, 3);

            }
        }

        //Selects a character programatically
        private void select_character(int character_id)
        {
            int count = 0;
            int slot = -1;
            foreach (String s in db.get_characters(int.Parse(db.get_property("sort_order"))))
            {
                if (character_id == db.get_character_id(s))
                {
                    slot = count;
                }
                count++;
            }

            CharacterListBox.SelectedIndex = slot;
        }

        //Select a skin programatically
        private void select_skin_slot(int slot)
        {
            Skinslistbox.SelectedIndex = slot - 1;
        }

        //-----------------------------------Loads-----------------------------------------------------

        //Load the skins for the selected character name
        private void load_skin_list(String selected_char_name)
        {
            ArrayList skins = db.get_character_skins(selected_char_name, db.get_property("workspace"));
            Skinslistbox.Items.Clear();
            foreach (String s in skins)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s;
                ContextMenu cm = new ContextMenu();
                MenuItem m1 = new MenuItem(); m1.Header = "Remove skin from workspace"; m1.Click += (s1, e) => { remove_skin(); };
                cm.Items.Add(m1);
                lbi.ContextMenu = cm;

                Skinslistbox.Items.Add(lbi);
            }
        }

        //Loads the files of the selected skin
        private void load_skin_files()
        {
            //Loading models
            skin_modelbox.Items.Clear();

            String[] models = db.get_skin_models(selected_skin_id);

            ContextMenu cm = new ContextMenu();
            MenuItem m1 = new MenuItem(); m1.Header = "Remove model from skin"; m1.Click += (s1, e) => { delete_model(); };
            cm.Items.Add(m1);

            ContextMenu cm2 = new ContextMenu();
            MenuItem m2 = new MenuItem(); m2.Header = "Remove csp from skin"; m2.Click += (s1, e) => { delete_csp(); };
            cm2.Items.Add(m2);
            foreach (String model in models)
            {
                if (model != "")
                {
                    ListBoxItem lbi = new ListBoxItem();
                    lbi.Content = model;
                    lbi.ContextMenu = cm;
                    skin_modelbox.Items.Add(lbi);
                }
            }

            //Loading csps
            skin_cspbox.Items.Clear();

            String[] csps = db.get_skin_csps(selected_skin_id);
            foreach (String csp in csps)
            {
                if (csp != "")
                {
                    ListBoxItem lbi = new ListBoxItem();
                    lbi.Content = csp;
                    lbi.ContextMenu = cm2;
                    skin_cspbox.Items.Add(lbi);
                }
            }
        }

        #endregion

        //Changing skin information
        #region Skin Information

        //Save name
        private void skin_name_save(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                String new_name = skins_skin_name.Text;
                db.set_skin_name(new_name, selected_skin_id);

                Write_Console("Skin name saved", 0);

                ListBoxItem lbi = new ListBoxItem();
                lbi = (ListBoxItem)Skinslistbox.SelectedItem;
                lbi.Content = new_name;
            }
        }

        //Save author
        private void skin_author_save(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                String new_name = skins_author.Text;
                db.set_skin_author(new_name, selected_skin_id);

                Write_Console("Skin author saved", 0);

            }
        }

        #endregion

        //Database actions
        #region Database

        //Add a skin to the database and skin library
        private void add_skin(object sender, RoutedEventArgs e)
        {
            db.add_skin("Custom Skin", "", "", "", this.selected_char_id, Skinslistbox.Items.Count + 1);
            if (uipresent)
            {
                uichar.setFile(db.get_character_uichar_position(this.selected_char_id), 7, Skinslistbox.Items.Count + 1);
            }

            load_skin_list(selected_char_name);
        }

        //Removes the skin from the workspace
        private void remove_skin()
        {
            if (!db.skin_locked(selected_slot, selected_char_id))
            {

                db.remove_skin(selected_slot, selected_char_id, db.get_property("workspace"));
                if (uipresent)
                {
                    uichar.setFile(db.get_character_uichar_position(this.selected_char_id), 7, Skinslistbox.Items.Count - 1);
                }
                db.reorder_skins(selected_char_id, active_workspace);
                load_skin_list(selected_char_name);
                Write_Console("Skin removed from the workspace", 0);
            }
            else
            {
                if (!db.skin_locked(selected_skin_id))
                {
                    restore_skin();
                }
                else
                {
                    Write_Console("Sorry, you cannot remove default skin slots!", 1);
                }

            }

        }

        //Converts a default skin to a custom one
        private void convert_skin(object sender, RoutedEventArgs e)
        {
            db.convert_skin("Converted skin", "", "", "", selected_char_id, selected_slot);
            load_skin_list(selected_char_name);
            Write_Console("The skin was converted to a custom one", 3);
        }

        //Restores a custom skin into a default one
        private void restore_skin()
        {
            db.restore_default(selected_slot, selected_char_id, db.get_property("workspace"));
            load_skin_list(selected_char_name);
        }
        #endregion

        //File actions
        #region File Actions

        //Drag enter call for the models
        private void skin_modelbox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        //When a model is dropped
        private void skin_modelbox_Drop(object sender, DragEventArgs e)
        {
            string[] folderlist = (string[])e.Data.GetData(DataFormats.FileDrop, false);


            //parsing selected index
            int slot = Skinslistbox.SelectedIndex + 1;

            //Checking if there is an actual skin selected
            if (slot != 0)
            {
                Skin selected_skin = new Skin(slot, selected_skin_id, selected_char_id, this.active_workspace, db);
                foreach (String s in folderlist)
                {
                    selected_skin.get_models(s);
                }

                load_skin_list(selected_char_name);
                Skinslistbox.SelectedIndex = slot - 1;
                load_skin_files();

            }
        }

        //Drag enter call for csps
        private void skin_cspbox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        //When csps are dropped
        private void skin_cspbox_Drop(object sender, DragEventArgs e)
        {
            string[] folderlist = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            FileAttributes attr = File.GetAttributes(folderlist[0]);
            //parsing selected index
            int slot = Skinslistbox.SelectedIndex + 1;
            Skin selected_skin = new Skin(slot, selected_skin_id, selected_char_id, this.active_workspace, db);
            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                //Checking if there is an actual skin selected
                if (slot != 0)
                {

                    foreach (String s in folderlist)
                    {
                        selected_skin.get_csps(s);
                    }

                    load_skin_list(selected_char_name);
                    Skinslistbox.SelectedIndex = slot - 1;
                    load_skin_files();
                }
            }

            else
            {
                //Checking if there is an actual skin selected
                if (slot != 0)
                {

                    foreach (String s in folderlist)
                    {
                        selected_skin.add_csp_file(s);
                    }


                    load_skin_list(selected_char_name);
                    Skinslistbox.SelectedIndex = slot - 1;
                    load_skin_files();
                }
            }




        }

        //Deletes the selected model
        private void delete_model()
        {
            //parsing selected index
            int slot = Skinslistbox.SelectedIndex + 1;

            //Checking if there is an actual skin selected
            if (slot != 0)
            {
                Skin selected_skin = new Skin(slot, selected_skin_id, selected_char_id, this.active_workspace, db);
                ListBoxItem selected_model = (ListBoxItem)skin_modelbox.Items[skin_modelbox.SelectedIndex];
                selected_skin.remove_model(selected_model.Content.ToString());
            }

            load_skin_files();

        }

        //Deletes the selected csp
        private void delete_csp()
        {
            //parsing selected index
            int slot = Skinslistbox.SelectedIndex + 1;

            //Checking if there is an actual skin selected
            if (slot != 0)
            {
                Skin selected_skin = new Skin(slot, selected_skin_id, selected_char_id, this.active_workspace, db);
                ListBoxItem selected_csp = (ListBoxItem)skin_cspbox.Items[skin_cspbox.SelectedIndex];
                selected_skin.remove_csp_file(selected_csp.Content.ToString());

            }
            load_skin_files();
        }

        #endregion

        //Link actions
        #region Link actions

        //Launches the web page of the author
        private void mod_author(object sender, RoutedEventArgs e)
        {
            int gb_uid = db.get_skin_gb_uid(selected_skin_id);
            System.Diagnostics.Process.Start("http://gamebanana.com/members/" + gb_uid);
        }

        //Forge launch
        private void forge_button_Click(object sender, RoutedEventArgs e)
        {
            String modelpath = app_path + "/filebank/skins/" + selected_skin_id + "/models/body/cxx/model.nud";

            if (File.Exists(modelpath))
            {
                if (File.Exists(app_path + "/forge/Smash Forge.exe"))
                {
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = app_path + "/forge/Smash Forge.exe";
                    startInfo.Arguments = "--superclean";
                    System.Diagnostics.Process.Start(startInfo);

                    System.Threading.Thread.Sleep(3000);

                    System.Diagnostics.ProcessStartInfo startInfo2 = new System.Diagnostics.ProcessStartInfo();
                    startInfo2.FileName = app_path + "/forge/Smash Forge.exe";
                    startInfo2.Arguments = "\" " + modelpath + "\" ";
                    System.Diagnostics.Process.Start(startInfo2);
                }
                else
                {
                    Write_Console("Smash Forge was not found in /forge", 1);
                }

            }
            else
            {
                Write_Console("There is no body/cXX to open in Smash Forge", 1);
            }

        }

        #endregion

        #endregion

        //Workspace Code
        #region Workspace

        //Loads
        #region Loads

        //Loads the workspace list
        private void reload_workspaces_list()
        {
            int selected = workspace_listbox.SelectedIndex;
            workspace_listbox.Items.Clear();
            ArrayList workspaces = db.get_workspaces();
            foreach (String s in workspaces)
            {
                ListBoxItem lbi = new ListBoxItem();

                ContextMenu cm = new ContextMenu();
                MenuItem m1 = new MenuItem(); m1.Header = "sync skin list from active workspace"; m1.Click += (s1, e) => { workspace_sync_skins(); };
                cm.Items.Add(m1);
                lbi.ContextMenu = cm;

                lbi.Content = s;
                workspace_listbox.Items.Add(lbi);
            }
            if (selected != -1)
            {
                if (selected + 1 > workspace_listbox.Items.Count)
                {
                    workspace_listbox.SelectedIndex = selected;
                }
                else
                {
                    workspace_listbox.SelectedIndex = workspace_listbox.Items.Count - 1;
                }
            }

        }

        //Loads a workspace's stats
        private void load_workspace_stats()
        {
            int[] stats = db.get_workspace_stats(workspace_listbox.SelectedIndex + 2);
            skin_count_text.Content = stats[0].ToString();
        }

        #endregion

        //Workspace actions
        #region Workspace actions

        //When a workspace is selected
        private void workspace_selected(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem li = (ListBoxItem)workspace_listbox.SelectedItem;
            if (li != null)
            {
                workspace_name_textbox.Text = li.Content.ToString();
            }

            if (db.workspace_default(workspace_listbox.SelectedIndex + 2))
            {
                workspace_clear.IsEnabled = false;
                delete_workspace.IsEnabled = false;
                workspace_name_textbox.IsEnabled = false;
                workspace_set_active.Visibility = Visibility.Hidden;
                launch_s4e_button.IsEnabled = false;
                workspace_build.IsEnabled = false;
            }
            else
            {
                workspace_clear.IsEnabled = true;
                delete_workspace.IsEnabled = true;
                workspace_name_textbox.IsEnabled = true;

                if (db.get_property("workspace") == db.get_workspace_id(workspace_listbox.SelectedIndex + 2).ToString())
                {
                    workspace_set_active.Visibility = Visibility.Hidden;
                    launch_s4e_button.IsEnabled = true;
                    workspace_build.IsEnabled = true;
                }
                else
                {
                    workspace_set_active.Visibility = Visibility.Visible;
                    launch_s4e_button.IsEnabled = false;
                    workspace_build.IsEnabled = false;
                }

            }



            load_workspace_stats();

        }

        #region Database

        //Sets the selected worskpace as active
        private void set_active_workspace(object sender, RoutedEventArgs e)
        {

            db.set_property_value(db.get_workspace_id(workspace_listbox.SelectedIndex + 2).ToString(), "workspace");

            active_workspace = db.get_workspace_id(workspace_listbox.SelectedIndex + 2);
            active_workspace_name_label.Content = db.get_workspace_name(active_workspace);
            active_workspace_slot = db.get_workspace_slot(workspace_listbox.SelectedIndex + 2);

            workspace_set_active.Visibility = Visibility.Hidden;

            workspace_build.IsEnabled = true;


            if (db.get_property("s4e_path").Length > 0)
            {
                launch_s4e_button.IsEnabled = true;
                set_s4e_workspace(db.get_workspace_id(workspace_listbox.SelectedIndex + 1));
            }
            Write_Console("Changed selected workspace", 0);


        }

        //Adds a new workspace
        private void add_workspace(object sender, RoutedEventArgs e)
        {
            if (workspace_listbox.Items.Count < 9)
            {
                lock_grids();
                change_statusbars(0, 0);
                change_status("Creating Workspace");
                change_operation("Creating Workspace");
                workspace_action = 1;
                workspace_worker.RunWorkerAsync();

                Write_Console("Added a new workspace", 0);
            }
            else
            {
                Write_Console("You cannot have more than 9 workspaces", 1);
            }

        }

        //Deletes the workspace from meteor
        private void workspace_delete(object sender, RoutedEventArgs e)
        {
            db.reorder_workspace();

            if (workspace_listbox.Items.Count > 1)
            {
                int id = db.get_workspace_id(workspace_listbox.SelectedIndex + 2);
                db.delete_workspace(id);
                db.reorder_workspace();
                reload_workspaces_list();
                Write_Console("Workspace deleted", 0);

                if (db.get_property("workspace") == active_workspace.ToString())
                {
                    db.set_property_value(db.get_workspace_id(2).ToString(), "workspace");
                }

            }
            else
            {
                Write_Console("You cannot delete your last workspace", 2);
            }

        }

        //Clears a workspace and adds the default back to it
        private void workspace_clear_Click(object sender, RoutedEventArgs e)
        {
            int id = db.get_workspace_id(workspace_listbox.SelectedIndex + 2);
            db.clear_workspace(id);

            db.add_default_skins(Convert.ToInt64(id));

            load_workspace_stats();

            Write_Console("Workspace cleared", 0);

        }

        //Sets a workspace name
        private void set_workspace_name(object sender, RoutedEventArgs e)
        {
            String name = workspace_name_textbox.Text;

            db.set_workspace_name(name, workspace_listbox.SelectedIndex + 2);
            if (workspace_listbox.SelectedIndex + 2 == active_workspace_slot)
            {
                active_workspace_name_label.Content = name;
            }

            ListBoxItem lbi = new ListBoxItem();
            lbi = (ListBoxItem)workspace_listbox.SelectedItem;
            lbi.Content = name;

            reload_workspaces_list();

            Write_Console("Changed workspace name to " + name, 0);
        }
        #endregion


        #region Skins

        //Adds default skins to a workspace
        private void add_default_skins()
        {
            db.add_default_skins(active_workspace_slot);
        }

        //Syncs the skins from the active workspace to the selected workspace
        public void workspace_sync_skins()
        {
            int selected = workspace_listbox.SelectedIndex + 2;
            if (selected == active_workspace_slot)
            {
                Write_Console("It's the same workspace, you dummy", 1);
            }
            else
            {
                change_status("operating");
                change_operation("copying");
                change_statusbars(0, 0);
                lock_grids();
                sel_work_thread = db.get_workspace_id(selected);
                workspace_action = 2;
                workspace_worker.RunWorkerAsync();
            }

        }

        #endregion

        //Build the workspace
        private void build_workspace(object sender, RoutedEventArgs e)
        {
            lock_grids();
            change_statusbars(0, 0);
            change_status("Exporting to S4E");
            change_operation("Building Workspace");
            build_worker.RunWorkerAsync();
        }

        #endregion

        #endregion

        //Configuration Code
        #region Configuration

        //Regional options
        #region Region Select

        //When the region is changed
        private void combo_region_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int region = combo_region.SelectedIndex;
            db.set_property_value(region.ToString(), "region");
            switch (region)
            {
                case 0:
                    Write_Console("Region changed to Europe", 0);
                    break;
                case 1:
                    Write_Console("Region changed to United States", 0);
                    break;
                case 2:
                    Write_Console("Region changed to Japan", 0);
                    break;
            }

        }

        //When the language is changed
        private void combo_language_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int language = combo_language.SelectedIndex;
            db.set_property_value(language.ToString(), "language");
            switch (language)
            {
                case 0:
                    Write_Console("Language changed to English", 0);
                    break;
                case 1:
                    Write_Console("Language changed to French", 0);
                    break;
                case 2:
                    Write_Console("Language changed to Spanish", 0);
                    break;
                case 3:
                    Write_Console("Language changed to German", 0);
                    break;
                case 4:
                    Write_Console("Language changed to Italian", 0);
                    break;
                case 5:
                    Write_Console("Language changed to Dutch", 0);
                    break;
                case 6:
                    Write_Console("Language changed to Portugese", 0);
                    break;
                case 7:
                    Write_Console("Language changed to Japanese", 0);
                    break;
            }
        }

        #endregion

        //Sm4shExplorer actions
        #region Sm4shExplorer
        //Validating a path by pressing enter
        private void config_s4e_path_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                String exepath = config_s4e_path.Text;
                String s4epath = new FileInfo(exepath).Directory.FullName;
                String filetype = new FileInfo(exepath).Extension;

                if (filetype == ".exe")
                {
                    if (new FileInfo(exepath).Exists)
                    {
                        if (new FileInfo(exepath).Name != "Sm4shFileExplorer.exe")
                        {
                            Write_Console("The file you selected is not called 'Sm4shFileExplorer.exe' but was still saved.", 1);
                        }

                        db.set_property_value(exepath, "s4e_exe");
                        db.set_property_value(s4epath, "s4e_path");
                        Write_Console("Sm4sh Explorer's executable path set to : " + exepath, 0);
                        config_s4e_path.Text = exepath;
                        set_s4e_workspace(active_workspace);
                    }
                    else
                    {
                        Write_Console("The path is invalid. There is no file at that location", 2);
                    }

                }
                else
                {
                    Write_Console("The path is invalid. It's not pointing to a .exe", 2);
                }
            }
        }

        //Validating a path by picking it
        private void config_s4e_path_save_Click(object sender, RoutedEventArgs e)
        {

            //Dialog
            winforms.OpenFileDialog openFileDialog = new winforms.OpenFileDialog();
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "")
            {
                //Getting the values
                String exepath = openFileDialog.FileName;
                String s4epath = new FileInfo(exepath).Directory.FullName;
                String filetype = new FileInfo(exepath).Extension;

                if (filetype == ".exe")
                {
                    if (new FileInfo(exepath).Name != "Sm4shFileExplorer.exe")
                    {
                        Write_Console("The file you selected is not called 'Sm4shFileExplorer.exe' but was still saved.", 1);
                    }

                    db.set_property_value(exepath, "s4e_exe");
                    db.set_property_value(s4epath, "s4e_path");
                    Write_Console("Sm4sh Explorer's executable path set to : " + openFileDialog.FileName, 0);
                    config_s4e_path.Text = openFileDialog.FileName;
                    set_s4e_workspace(active_workspace);
                }
                else
                {
                    Write_Console("The file you selected is not a .exe", 2);

                }
            }
        }
        #endregion

        //UI_Character_DB actions
        #region UI_Char
        //Searches automatically for ui_char
        private void search_ui(object sender, RoutedEventArgs e)
        {
            String base_folder = db.get_property("s4e_path");
            String destination = app_path + "/filebank/configuration/uichar/ui_character_db.bin";
            if (Directory.Exists(base_folder))
            {
                String[] files = System.IO.Directory.GetFiles(base_folder, "ui_character_db.bin", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    if (!Directory.Exists(app_path + "/filebank/configuration/uichar/"))
                    {
                        Directory.CreateDirectory(app_path + "/filebank/configuration/uichar/");
                    }
                    String filepath = files[0];
                    File.Copy(filepath, destination, true);
                    Write_Console("found the file at : " + filepath, 0);
                    ui_char_status.Content = "Status : imported";
                    db.set_property_value("1", "ui_char");
                    uipresent = true;

                }
                else
                {
                    Write_Console("ui_character_db was not found in the specified SmashExplorer folder", 2);
                }
            }
            else
            {
                Write_Console("Please make sure Sm4sh Explorer's path is properly setup", 2);

            }


        }

        //Lets the user pick for ui_char
        private void pick_ui(object sender, RoutedEventArgs e)
        {
            winforms.OpenFileDialog ofd = new winforms.OpenFileDialog();
            winforms.DialogResult result = ofd.ShowDialog();
            String filename = ofd.FileName;
            String destination = app_path + "filebank/configuration/uichar/ui_character_db.bin";


            if (System.IO.Path.GetFileName(filename) == "ui_character_db.bin")
            {
                if (!Directory.Exists(app_path + "filebank/configuration/uichar/"))
                {
                    Directory.CreateDirectory(app_path + "filebank/configuration/uichar/");
                }
                File.Copy(filename, destination, true);

                Write_Console("ui_character_db.bin selected", 0);
                ui_char_status.Content = "Status : imported";
                db.set_property_value("1", "ui_char");
                uipresent = true;
            }

        }
        #endregion

        //Configuration actions
        #region Config
        private void config_sortby_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int order = config_sortby.SelectedIndex;
            db.set_property_value(order.ToString(), "sort_order");
            if (order == 0)
            {
                Write_Console("Sort Order changed to Alphabetical", 0);
            }
            else
            {
                Write_Console("Sort Order changed to Game Order", 0);
            }

        }

        private void gb_uid_save(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                String uid = config_gbuser.Text;
                db.set_property_value(uid, "gb_uid");
                Write_Console("Gamebanana user changed to " + uid, 0);
            }
        }

        private void config_username_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                String uid = config_username.Text;
                db.set_property_value(uid, "username");
                Write_Console("Username changed to " + uid, 0);
            }
        }

        private void config_devlogs_Checked(object sender, RoutedEventArgs e)
        {
            String val = config_devlogs.IsChecked == true ? "1" : "0";
            db.set_property_value(val, "dev_logs");
            if (val == "1")
            {
                Write_Console("Dev Logs Activated", 0);
            }
            else
            {
                Write_Console("Dev Logs Deactivated", 0);
            }

        }

        private void config_S4E_Checked(object sender, RoutedEventArgs e)
        {
            String val = config_S4E.IsChecked == true ? "1" : "0";
            db.set_property_value(val, "s4e_launch");
            if (val == "1")
            {
                Write_Console("Sm4sh Explorer will be launched after each export", 0);
            }
            else
            {
                Write_Console("Sm4sh Explorer won't be launched after the export", 0);
            }
        }

        private void config_ui_rebuild_Checked(object sender, RoutedEventArgs e)
        {
            String val = config_ui_rebuild.IsChecked == true ? "1" : "0";
            db.set_property_value(val, "reload_ui");
            if (val == "1")
            {
                Write_Console("ui_character_db will be refreshed before the export", 0);
            }
            else
            {
                Write_Console("ui_character_db won't be refreshed before the export", 0);
            }
        }
        #endregion

        #endregion

        //Filebank Code
        #region Filebank

        //Loads
        #region loads

        //Loads the components
        public void load_skin_bank()
        {
            filebank_character_select.Items.Clear();
            ArrayList characters = db.get_characters(int.Parse(db.get_property("sort_order")));
            foreach (String s in characters)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = s;
                filebank_character_select.Items.Add(item);
            }

            if (selected_char_name != "")
            {
                filebank_character_select.SelectedItem = filebank_character_select.FindName("selected_char_name");
            }
        }

        //Loads skins for a certain character
        private void filebank_reload_skins()
        {
            if (filebank_character_select.SelectedIndex != -1)
            {
                ListBoxItem character = (ListBoxItem)filebank_character_select.Items[filebank_character_select.SelectedIndex];

                ArrayList skins = db.get_character_custom_skins(character.Content.ToString(), db.get_property("workspace"));
                filebank_skins = db.get_character_custom_skins_id(character.Content.ToString(), db.get_property("workspace"));
                filebank_skins_list.Items.Clear();
                foreach (String s in skins)
                {
                    ListBoxItem lbi = new ListBoxItem();
                    lbi.Content = s;

                    filebank_skins_list.Items.Add(lbi);
                }
            }
        }

        #endregion

        //Skin Actions
        #region Skin Actions

        //When a skin character is selected
        private void filebank_character_select_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filebank_reload_skins();
        }

        //When a skin is selected
        private void filebank_skin_selected(object sender, SelectionChangedEventArgs e)
        {
            if (filebank_skins_list.SelectedIndex != -1)
            {
                String[] infos = db.get_skin_info((int)filebank_skins[filebank_skins_list.SelectedIndex]);
                filebank_skin_author.Content = infos[1];
                filebank_skin_csps.Content = infos[3];
                filebank_skin_models.Content = infos[2];
                filebank_skin_id.Content = (int)filebank_skins[filebank_skins_list.SelectedIndex];
            }

        }

        //Deletes a skin permanently
        private void filebank_skin_delete(object sender, RoutedEventArgs e)
        {
            int id = (int)filebank_skins[filebank_skins_list.SelectedIndex];
            if (!db.check_skin_in_library(id))
            {
                String skinpath = app_path + "/filebank/skins/" + id + "/";
                if (Directory.Exists(skinpath))
                {
                    Directory.Delete(skinpath, true);

                }
                db.delete_skin(id);

                filebank_reload_skins();
            }
            else
            {
                Write_Console("You have to remove it from the workspaces first", 1);
            }
        }

        //Launches forge
        private void filebank_forge_preview(object sender, RoutedEventArgs e)
        {
            int id = (int)filebank_skins[filebank_skins_list.SelectedIndex];
            String modelpath = app_path + "/filebank/skins/" + id + "/models/body/cxx/model.nud";

            if (File.Exists(modelpath))
            {
                if (File.Exists(app_path + "/forge/Smash Forge.exe"))
                {
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = app_path + "/forge/Smash Forge.exe";
                    startInfo.Arguments = "--superclean";
                    System.Diagnostics.Process.Start(startInfo);

                    System.Threading.Thread.Sleep(3000);

                    System.Diagnostics.ProcessStartInfo startInfo2 = new System.Diagnostics.ProcessStartInfo();
                    startInfo2.FileName = app_path + "/forge/Smash Forge.exe";
                    startInfo2.Arguments = "\" " + modelpath + "\" ";
                    System.Diagnostics.Process.Start(startInfo2);
                }
                else
                {
                    Write_Console("Smash Forge was not found in /forge", 1);
                }

            }
            else
            {
                Write_Console("There is no body/cXX to open in Smash Forge", 1);
            }
        }

        //Inserts the skin in the active workspace
        private void insert_skin(object sender, RoutedEventArgs e)
        {
            int id = (int)filebank_skins[filebank_skins_list.SelectedIndex];
            int workspace = int.Parse(db.get_property("workspace"));
            ListBoxItem character = (ListBoxItem)filebank_character_select.Items[filebank_character_select.SelectedIndex];
            int character_id = db.get_character_id(character.Content.ToString());

            int slot = db.get_character_skins(character.Content.ToString(), workspace.ToString()).Count + 1;

            db.insert_skin(id, workspace, character_id, slot);
            Write_Console("The skin was inserted in the workspace", 0);
        }

        #endregion

        #endregion

        //About Code
        #region About
        //Answer to the thanks button
        private void thanks_button(object sender, RoutedEventArgs e)
        {
            Write_Console("You're Welcome !", 0);
        }

        //Launch the wiki web page
        private void goto_wiki(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Mowjoh/Meteor/wiki");

        }

        #endregion

        #endregion

        //Web
        #region Web
        //Downloads a file at a certain url
        private void download(String url)
        {
            lock_grids();
            this.download_resource_link = url;
            Write_Console("Downloading file at '" + url + "'", 0);
            change_statusbars(1, 1);
            change_status("Downloading");
            change_operation("Downloading");

            download_worker.RunWorkerAsync();

        }
        #endregion

        //Workers
        #region Workers

        //Sets up the worker instances
        private void setup_workers()
        {

            //Workspace worker setup
            workspace_worker.DoWork += workspace_dowork;
            workspace_worker.RunWorkerCompleted += workspace_completed;
            workspace_worker.ProgressChanged += workspace_report;

            build_worker.DoWork += export_dowork;
            build_worker.RunWorkerCompleted += export_complete;
            build_worker.ProgressChanged += export_report;

            download_worker.DoWork += download_dowork;
            download_worker.RunWorkerCompleted += download_completed;
            download_worker.WorkerReportsProgress = true;

            url_worker.DoWork += url_dowork;
            url_worker.RunWorkerCompleted += url_complete;
            url_worker.ProgressChanged += url_report;
            url_worker.WorkerReportsProgress = true;

            url_worker.RunWorkerAsync();

            //Setting up webclient
            webClient.DownloadProgressChanged += download_progress;

        }

        //-----------------------------------Workspace---------------------------------------------------

        private void workspace_dowork(object sender, DoWorkEventArgs e)
        {
            switch (workspace_action)
            {
                case 1:
                    long id = db.add_workspace("New Workspace", workspace_listbox.Items.Count + 2);
                    db.add_default_skins(id);
                    break;

                case 2:
                    long selected = sel_work_thread;
                    db.copy_skins(Convert.ToInt64(active_workspace), selected);
                    break;
                case 3:
                    import_msl_content();
                    break;

            }




        }

        private void workspace_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            reload_workspaces_list();
            change_statusbars(2, 2);
            change_status("Complete");
            change_operation("Complete");
            unlock_grids();
        }

        private void workspace_report(object sender, ProgressChangedEventArgs e)
        {

        }

        //-----------------------------------Build---------------------------------------------------

        private void export_dowork(object sender, DoWorkEventArgs e)
        {

            //Making sure it's the proper ID
            build.set_workspace_id(active_workspace);

            //Cleaning folders
            build.clean_workspace();

            //Building folders
            build.build();

        }

        private void export_complete(object sender, RunWorkerCompletedEventArgs e)
        {
            Write_Console("Export complete !", 0);
            change_statusbars(2, 2);
            change_status("Complete");
            change_operation("Complete");
            unlock_grids();
        }

        public void export_report(object sender, ProgressChangedEventArgs e)
        {

        }

        //-----------------------------------Link Parser------------------------------------------------

        public void url_dowork(object sender, DoWorkEventArgs e)
        {
            Mutex meteor_url;
            int i = 0;
            int y = 1;
            while (i == 0)
            {
                if (Mutex.TryOpenExisting("meteor_url", out meteor_url))
                {
                    meteor_url.Dispose();
                    url_worker.ReportProgress(y);
                    y = y == 99 ? 1 : y++;
                }
            }
        }

        public void url_report(object sender, ProgressChangedEventArgs e)
        {
            if (File.Exists(app_path + "/downloads/url.txt"))
            {

                string[] lines = System.IO.File.ReadAllLines(app_path + "/downloads/url.txt");

                Write_Console("Url Detected", 0);
                download(lines[1]);

                File.Delete(app_path + "/downloads/url.txt");
            }
        }

        public void url_complete(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        //-----------------------------------Download---------------------------------------------------

        public void download_dowork(object sender, DoWorkEventArgs e)
        {

            if (!Directory.Exists(app_path + "/downloads/"))
            {
                Directory.CreateDirectory(app_path + "/downloads/");
            }
            else
            {
                Directory.Delete(app_path + "/downloads/", true);
                Directory.CreateDirectory(app_path + "/downloads/");
            }

            String link = download_resource_link.Substring(7, download_resource_link.Length - 7);
            String extension = download_resource_link.Split('.')[download_resource_link.Split('.').Length - 1];
            webClient.DownloadFile(link, app_path + "/downloads/archive." + extension);
            String source = app_path + "/downloads/archive." + extension;
            String dest = app_path + "/downloads/archive/";

            if (extension == "rar")
            {
                //Extracting archive
                using (Stream stream = File.OpenRead(source))
                {
                    var reader = ReaderFactory.Open(stream);
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            reader.WriteEntryToDirectory(dest, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                        }
                    }
                    reader.Dispose();

                }
                add_extracted_content();


            }
            else
            {
                //Extracting archive
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Hidden;
                pro.FileName = app_path + "/7za.exe";
                String arguments = "x \"" + source + "\" -o\"" + dest + "\"";
                pro.Arguments = arguments;
                Process x = Process.Start(pro);
                x.WaitForExit();
                add_extracted_content();
            }





        }

        public void download_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Write_Console("Download complete !", 0);
            change_statusbars(2, 2);
            change_status("Complete");
            change_operation("Complete");
            unlock_grids();
            select_character(install_char);
            select_skin_slot(install_skin_slot);

        }

        public void download_progress(object sender, DownloadProgressChangedEventArgs e)
        {
            change_statusbars(e.ProgressPercentage, e.ProgressPercentage);
        }

        //Adds extracted content after the download process
        public void add_extracted_content()
        {
            String[] subfolders = Directory.GetDirectories(app_path + "/downloads/archive/");
            ArrayList msl_folders = new ArrayList();
            if (subfolders.Length > 0)
            {
                foreach (String s in subfolders)
                {
                    String name = newpath.GetFileName(s);
                    if (db.check_msl_character_name(name))
                    {
                        msl_folders.Add(s);
                    }
                }
            }
            //MSL tagged content
            if (msl_folders.Count > 0)
            {
                //MSL Downloads
                foreach (String s in msl_folders)
                {
                    String name = newpath.GetFileName(s);
                    add_msl_chara_skins(s, name);
                }
            }
            else
            {
                //Meteor downloads
            }
        }

        //Adds meteor skin library skins with a character folder
        public void add_msl_chara_skins(String chara_path, String chara_name)
        {
            String[] meteor_skins = Directory.GetDirectories(chara_path);
            foreach (String skin in meteor_skins)
            {
                String skin_folder = newpath.GetFileName(skin);

                try
                {
                    String skin_name = skin.Split('_')[2];
                    if (skin_folder.Split('_')[0] == "meteor" && skin_folder.Split('_')[1] == "xx" && skin_folder.Split('_').Length == 3)
                    {

                        int character_id = db.get_character_id_msl(chara_name);
                        int skin_count = db.get_character_skin_count(character_id, active_workspace);
                        int last_id = db.add_skin(skin_name, "", "", "", character_id, skin_count + 1);

                        Skin created_skin = new Skin(skin_count + 1, last_id, character_id, active_workspace, db);
                        created_skin.get_models(skin + "/model/");
                        created_skin.get_csps(skin + "/csp/");

                        String xmlpath = skin + "/meta/meta.xml";
                        if (File.Exists(xmlpath))
                        {
                            XmlDocument xml = new XmlDocument();
                            xml.Load(xmlpath);
                            XmlNode nodes = xml.SelectSingleNode("/metadata/meta[attribute::name='author']");
                            String author = nodes.InnerText;
                            XmlNode names = xml.SelectSingleNode("/metadata/meta[attribute::name='name']");
                            String name = names.InnerText;

                            db.set_skin_author(author, last_id);
                            db.set_skin_gb_uid(-1, last_id);
                            db.set_skin_name(name, last_id);

                            this.install_char = character_id;
                            this.install_skin_slot = skin_count + 1;

                        }

                    }
                }
                catch
                {

                }
            }
        }


        //-----------------------------------MSL Import---------------------------------------------------

        //Launches an import from Meteor Skin Library
        private void launch_import_msl(object sender, RoutedEventArgs e)
        {
            //Dialog
            winforms.OpenFileDialog openFileDialog = new winforms.OpenFileDialog();
            openFileDialog.Title = "Select Meteor Skin Library.exe";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Filter = "MSL Executable (Meteor Skin Library.exe)|Meteor Skin Library.exe";
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "")
            {
                msl_path = openFileDialog.FileName;
                change_status("operating");
                change_operation("importing");
                change_statusbars(0, 0);
                lock_grids();
                workspace_action = 3;
                workspace_worker.RunWorkerAsync();
            }

        }

        //Import from MSL logic
        public void import_msl_content()
        {
            long id = db.add_workspace("MSL Import", workspace_listbox.Items.Count + 2);
            db.add_default_skins(id);

            String workspacepath = new FileInfo(msl_path).DirectoryName;

        }

        #endregion

        //Updates
        #region Updates

        //New updater
        private void check_update()
        {

            try
            {
                //Loading local manifest
                XmlDocument xml2 = new XmlDocument();
                String local_version = "";
                if (File.Exists(app_path + "/Meteor.exe.manifest"))
                {
                    xml2.Load(app_path + "/Meteor.exe.manifest");
                    XmlNode nodes2 = xml2.SelectSingleNode("//*[local-name()='assembly']/*[local-name()='assemblyIdentity']");
                    String version2 = nodes2.Attributes[1].Value;
                    app_version_lbel.Content = "Application Version : " + version2;
                    local_version = version2.Replace('.', '_');
                }
                else
                {
                    local_version = "0_0_0_0";
                }


                //Searching for last update
                String last_version = get_last_version();

                //Loading remote manifest
                XmlDocument xml = new XmlDocument();
                xml.Load("http://mowjoh.com/meteor/Application Files/Meteor_" + last_version + "/Meteor.exe.manifest");
                XmlNode nodes = xml.SelectSingleNode("//*[local-name()='assembly']/*[local-name()='assemblyIdentity']");
                String version = nodes.Attributes[1].Value;
                String remote_version = version.Replace('.', '_');

                if (compare_version(local_version, remote_version))
                {
                    //Update logic
                    update();
                }
            }
            catch
            {
                Write_Console("The update couldn't be checked", 2);
            }

        }

        //Tells if the remoteversion is newer
        private Boolean compare_version(String localversion, String remoteversion)
        {
            try
            {
                int l_major = int.Parse(localversion.Split('_')[0]);
                int l_minor = int.Parse(localversion.Split('_')[1]);
                int l_build = int.Parse(localversion.Split('_')[2]);
                int l_revision = int.Parse(localversion.Split('_')[3]);

                int r_major = int.Parse(remoteversion.Split('_')[0]);
                int r_minor = int.Parse(remoteversion.Split('_')[1]);
                int r_build = int.Parse(remoteversion.Split('_')[2]);
                int r_revision = int.Parse(remoteversion.Split('_')[3]);

                //remote major is superior
                if (r_major > l_major)
                {
                    return true;
                }
                else
                {
                    if (r_minor > l_minor)
                    {
                        return true;
                    }
                    else
                    {
                        if (r_build > l_build)
                        {
                            return true;
                        }
                        else
                        {
                            if (r_revision > l_revision)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                Write_Console("could not compare versions", 2);
                return false;
            }
        }

        //Searches for last update
        private string get_last_version()
        {
            try
            {
                //Getting remote info
                String remote_path = "http://mowjoh.com/meteor/Application Files/patchnotes.xml";
                XmlDocument xml = new XmlDocument();
                xml.Load(remote_path);
                XmlNode nodes = xml.SelectSingleNode("package");
                String version = nodes.Attributes[0].Value.ToString();
                version = version.Replace('.', '_');
                return version;
            }
            catch (Exception)
            {
                Write_Console("the update search failed", 2);
                return "";
            }
        }

        //Launches the update process
        private void update()
        {
            MessageBoxResult result = MessageBox.Show("An update is available. Proceed with the update?", "Segtendo WARNING", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {

                ProcessStartInfo pro = new ProcessStartInfo();
                pro.FileName = app_path + "/Meteor Updater.exe";
                Process x = Process.Start(pro);
                Application.Current.Shutdown();
            }
        }

        #endregion

        //External Tools
        #region Tools

        //Sm4shExplorer actions
        #region Sm4shExplorer

        //Launch Sm4shExplorer
        private void launch_s4e(object sender, RoutedEventArgs e)
        {
            String path = db.get_property("s4e_exe");
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = path;
            startInfo.WorkingDirectory = db.get_property("s4e_path") + "/";
            System.Diagnostics.Process.Start(startInfo);
        }

        //Sets Sm4shExplorer's workspace path to one of Meteor's workspaces
        public void set_s4e_workspace(int workspace_id)
        {
            //Loading local manifest
            XmlDocument xml = new XmlDocument();
            String workspace_path = app_path + "/workspaces/workspace_" + workspace_id + "/";
            if (!Directory.Exists(workspace_path + "/content/patch/"))
            {
                Directory.CreateDirectory(workspace_path + "/content/patch/");
            }
            if (File.Exists(db.get_property("s4e_path") + "/sm4shmod.xml"))
            {
                xml.Load(db.get_property("s4e_path") + "/sm4shmod.xml");
                XmlNode node = xml.SelectSingleNode("/Sm4shMod/ProjectWorkplaceFolder");
                if (node == null)
                {
                    XmlElement newnode = xml.CreateElement("ProjectWorkplaceFolder");
                    newnode.InnerText = workspace_path;
                    XmlNode root = xml.SelectSingleNode("/Sm4shMod");
                    root.AppendChild(newnode);
                }
                else
                {
                    node.InnerText = workspace_path;
                }

                xml.Save(db.get_property("s4e_path") + "/sm4shmod.xml");
            }
            else
            {
                Write_Console("Could not assign Sm4sh Explorer's workspace", 1);
            }
        }


        #endregion

        //7-Zip Actions
        #region 7-zip

        //Launches an extract
        private void extract(String path, String output_path)
        {
            //Extracting archive
            ProcessStartInfo pro = new ProcessStartInfo();
            pro.WindowStyle = ProcessWindowStyle.Hidden;
            pro.FileName = app_path + "/7za.exe";
            String arguments = "x \"" + (path) + "\" -o\"" + (app_path + output_path) + "\"";
            pro.Arguments = arguments;
            Process x = Process.Start(pro);
            x.WaitForExit();
        }

        #endregion

        #endregion




    }
}
