using Meteor.database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
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
using WinForms = System.Windows;

namespace Meteor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables
        db_handler db;
        int workspace;

        //Selected
        String selected_char_name ="";
        int selected_char_id = -1;

        int selected_skin_id = -1;
        int selected_slot = -1;

        int selected_workspace = -1;
        #endregion

        #region Constructor
        //Launch
        public MainWindow()
        {
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
            Write_Console("Welcome to Meteor !", 0);

            #endregion
            
            
        }
        #endregion

        //Init procedures
        #region Inits
        private void fill_lists()
        {
            ArrayList chars = db.get_characters(1);
            CharacterListBox.Items.Clear();
            foreach (String s in chars)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s;
                CharacterListBox.Items.Add(lbi);
            }

            reload_workspaces_list();


        }
        private void retreive_config()
        {
            //Region Select
            combo_region.SelectedIndex = int.Parse(db.get_property("region"));
            combo_language.SelectedIndex = int.Parse(db.get_property("language"));

            //S4E path
            config_s4e_path.Text = db.get_property("s4e_path");

            //Config
            config_username.Text = db.get_property("username");
            config_sortby.SelectedIndex = int.Parse(db.get_property("sort_order"));
            config_ui_rebuild.IsChecked = db.get_property("reload_ui") == "1" ? true : false;
            config_S4E.IsChecked = db.get_property("s4e_launch") == "1" ? true : false;

            //Dev Options
            config_devlogs.IsChecked = db.get_property("dev_logs") == "1" ? true : false;

            //GB Options
            config_gbuser.Text = db.get_property("gb_uid");

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
                    if(selected_char_name != "")
                    {
                        reload_skins(selected_char_name);
                    }
                    break;
                case "Stages":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[1];
                    break;
                case "Interface":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[2];
                    break;
                case "FileBank":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[3];
                    load_skin_bank();
                    break;
                case "Workspace":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[4];
                    load_workspace_stats();
                    break;
                case "Configuration":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[5];
                    break;
                case "About":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[6];
                    break;
            }
        }


        #region Skin Section
        private void character_selected(object sender, SelectionChangedEventArgs e)
        {
            
            ListBoxItem li = (ListBoxItem)CharacterListBox.SelectedItem;
            if (li != null)
            {
                selected_char_name = li.Content.ToString();
                Write_Console("Selected char name " + selected_char_name, 3);
                selected_char_id = db.get_character_id(selected_char_name);
                Write_Console("Selected char id " + selected_char_id, 3);

                reload_skins(selected_char_name);
            }


        }

        private void skin_selected(object sender, SelectionChangedEventArgs e)
        {
            selected_slot = Skinslistbox.SelectedIndex + 1;
            Write_Console("Selected slot " + selected_slot, 3);
            selected_skin_id = db.get_skin_id(selected_char_id, selected_slot);
            Write_Console("Selected skin id " + selected_skin_id, 3);
            ListBoxItem li = (ListBoxItem)CharacterListBox.SelectedItem;

            int gb_uid = db.get_skin_gb_uid(selected_skin_id);

            if (selected_slot > 0)
            {
                //Loading info
                String[] infos = db.get_skin_info(selected_skin_id);
                skins_skin_name.Text = infos[0];
                skins_author.Text = infos[1];
                skins_slot.Text = selected_slot.ToString();

                //Checking locked skin
                if (db.skin_locked(selected_slot, selected_char_id))
                {
                    skins_author.IsReadOnly = true;
                    skins_skin_name.IsReadOnly = true;
                    convert_button.Visibility = Visibility.Visible;
                    author_button.Visibility = Visibility.Hidden;

                }
                else
                {
                    convert_button.Visibility = Visibility.Hidden;
                    if (gb_uid > 0)
                    {
                        skins_skin_name.IsReadOnly = true;
                        skins_author.IsReadOnly = true;
                        author_button.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        skins_skin_name.IsReadOnly = false;
                        skins_author.IsReadOnly = false;
                        author_button.Visibility = Visibility.Hidden;
                    }
                }
                
               
                
            }
        }

        private void add_skin(object sender, RoutedEventArgs e)
        {
            db.add_skin("Custom Skin", "", "", "", this.selected_char_id, Skinslistbox.Items.Count + 1);
            reload_skins(selected_char_name);
        }

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

        private void skin_author_save(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                String new_name = skins_author.Text;
                db.set_skin_author(new_name, selected_skin_id);

                Write_Console("Skin author saved", 0);

            }
        }

        private void mod_link(object sender, RoutedEventArgs e)
        {
            int gb_id = db.get_skin_gb_id(selected_skin_id);
            System.Diagnostics.Process.Start("http://gamebanana.com/skins/"+gb_id);
        }

        private void mod_author(object sender, RoutedEventArgs e)
        {
            int gb_uid = db.get_skin_gb_uid(selected_skin_id);
            System.Diagnostics.Process.Start("http://gamebanana.com/members/" + gb_uid);
        }

        private void reload_skins(String selected_char_name)
        {
            ArrayList skins = db.get_character_skins(selected_char_name, db.get_property("workspace"));
            Skinslistbox.Items.Clear();
            foreach (String s in skins)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s;
                ContextMenu cm = new ContextMenu();
                MenuItem m1 = new MenuItem(); m1.Header = "Remove skin from workspace";m1.Click += (s1, e) => { delete_skin(); };
                cm.Items.Add(m1);
                lbi.ContextMenu = cm;
                
                Skinslistbox.Items.Add(lbi);
            }
        }

        private void delete_skin()
        {
            if (!db.skin_locked(selected_slot, selected_char_id))
            {
                db.remove_skin(selected_slot, selected_char_id, db.get_property("workspace"));
                reload_skins(selected_char_name);
                Write_Console("Skin removed from the workspace", 0);
            }else
            {
                Write_Console("Sorry, you cannot remove default skins!", 1);
            }
            
        }
        #endregion

        #region Workspace Section
        private void add_workspace(object sender, RoutedEventArgs e)
        {
            if(workspace_listbox.Items.Count < 9)
            {
                long id = db.add_workspace("New Workspace", workspace_listbox.Items.Count+1);
                db.add_default_skins(id);
                reload_workspaces_list();

                Write_Console("Added a new workspace", 0);
            }else
            {
                Write_Console("You cannot have more than 9 workspaces", 1);
            }
            
        }

        private void set_workspace_name(object sender, RoutedEventArgs e)
        {
            String name = workspace_name_textbox.Text;
            int slot = workspace_listbox.SelectedIndex +1;
            db.set_workspace_name(name, slot);

            ListBoxItem lbi = new ListBoxItem();
            lbi = (ListBoxItem)workspace_listbox.SelectedItem;
            lbi.Content = name;

            reload_workspaces_list();

            Write_Console("Changed workspace name to " + name, 0);
        }

        private void set_active_workspace(object sender, RoutedEventArgs e)
        {
            
            db.set_property_value(db.get_workspace_id(workspace_listbox.SelectedIndex + 1).ToString(),"workspace");
            workspace_set_active.Visibility = Visibility.Hidden;
            Write_Console("Changed selected workspace", 0);

            
        }

        private void workspace_selected(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem li = (ListBoxItem)workspace_listbox.SelectedItem;
            if(li != null)
            {
                workspace_name_textbox.Text = li.Content.ToString();
            }

            if (db.workspace_default(workspace_listbox.SelectedIndex + 1))
            {
                workspace_save_name.IsEnabled = false;
                workspace_clear.IsEnabled = false;
                workspace_name_textbox.IsEnabled = false;
                workspace_set_active.Visibility = Visibility.Hidden;
            }
            else
            {
                workspace_save_name.IsEnabled = true;
                workspace_clear.IsEnabled = true;
                workspace_name_textbox.IsEnabled = true;

                if (db.get_property("workspace") == db.get_workspace_id(workspace_listbox.SelectedIndex + 1).ToString())
                {
                    workspace_set_active.Visibility = Visibility.Hidden;
                }
                else
                {
                    workspace_set_active.Visibility = Visibility.Visible;
                }

            }

           

            load_workspace_stats();

        }

        private void reload_workspaces_list()
        {
            workspace_listbox.Items.Clear();
            ArrayList workspaces = db.get_workspaces();
            foreach (String s in workspaces)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s;
                workspace_listbox.Items.Add(lbi);
            }

        }

        private void load_workspace_stats()
        {
            int[] stats = db.get_workspace_stats(workspace_listbox.SelectedIndex + 1);
            skin_count_text.Content = stats[0].ToString();
        }

        private void add_default_skins()
        {
            db.add_default_skins(workspace_listbox.SelectedIndex+1);
        }

        private void workspace_clear_Click(object sender, RoutedEventArgs e)
        {
            int slot = workspace_listbox.SelectedIndex + 1;
            int id = db.get_workspace_id(slot);

            db.clear_workspace(id);

            db.add_default_skins(Convert.ToInt64(id));

            load_workspace_stats();

            Write_Console("Workspace cleared", 0);

        }
        #endregion

        #region Configuration

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

        private void config_sortby_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int order = config_sortby.SelectedIndex;
            db.set_property_value(order.ToString(), "sort_order"); 
            if(order == 0)
            {
                Write_Console("Sort Order changed to Alphabetical", 0);
            }else
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
                Write_Console("Gamebanana user changed to "+uid, 0);
            }
        }

        private void config_username_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                String uid = config_username.Text;
                db.set_property_value(uid, "username");
                Write_Console("Username changed to " + uid, 0);
            }
        }

        private void config_s4e_path_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                String path = config_s4e_path.Text;
                db.set_property_value(path, "s4e_path");
                Write_Console("Sm4sh Explorer's workspace path set to : " + path, 0);
            }
        }

        private void config_s4e_path_save_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new WinForms.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            db.set_property_value(dialog.SelectedPath, "s4e_path");
            Write_Console("Sm4sh Explorer's workspace path set to : " + dialog.SelectedPath, 0);
            config_s4e_path.Text = dialog.SelectedPath;

        }

        private void config_devlogs_Checked(object sender, RoutedEventArgs e)
        {
            String val = config_devlogs.IsChecked == true ? "1" : "0";
            db.set_property_value(val, "dev_logs");
            if(val == "1")
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

        #region Filebank
        public void load_skin_bank()
        {
            filebank_skins_datagrid.ItemsSource = db.get_custom_skins().Tables[0].DefaultView;
        }
        #endregion

        #region About Section
        //Answer to the thanks button
        private void thanks_button(object sender, RoutedEventArgs e)
        {
            Write_Console("You're Welcome !", 0);
        }

        private void goto_wiki(object sender,RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Mowjoh/Meteor/wiki");

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



















        #endregion

        
    }
}
