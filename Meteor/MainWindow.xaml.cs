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
        String selected_char_name;
        int selected_char_id = -1;

        int selected_skin_id = -1;
        int selected_slot = -1;
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
                    break;
                case "Stages":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[1];
                    break;
                case "Interface":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[2];
                    break;
                case "FileBank":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[3];
                    break;
                case "Workspace":
                    TabControl_Sections.SelectedItem = TabControl_Sections.Items[4];
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
            Skinslistbox.Items.Clear();
            ListBoxItem li = (ListBoxItem)CharacterListBox.SelectedItem;
            if (li != null && Skinslistbox.Items.Count == 0)
            {
                selected_char_name = li.Content.ToString();
                Write_Console("Selected char name " + selected_char_name, 3);
                selected_char_id = db.get_character_id(selected_char_name);
                Write_Console("Selected char id " + selected_char_id, 3);

                ArrayList skins = db.get_character_skins(selected_char_name);

                foreach (String s in skins)
                {
                    ListBoxItem lbi = new ListBoxItem();
                    lbi.Content = s;
                    Skinslistbox.Items.Add(lbi);
                }
            }


        }

        private void skin_selected(object sender, SelectionChangedEventArgs e)
        {
            selected_slot = Skinslistbox.SelectedIndex + 1;
            Write_Console("Selected slot " + selected_slot, 3);
            selected_skin_id = db.get_skin_id(selected_char_id, selected_slot);
            Write_Console("Selected skin id " + selected_skin_id, 3);
            ListBoxItem li = (ListBoxItem)CharacterListBox.SelectedItem;

            if (selected_slot > 0)
            {
                String[] infos = db.get_skin_info(selected_slot, li.Content.ToString());
                skins_skin_name.Text = infos[0];
                skins_author.Text = infos[1];
                skins_slot.Text = selected_slot.ToString();
            }
        }

        private void add_skin(object sender, RoutedEventArgs e)
        {
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
        #endregion

        #region Configuration

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

        #region About Section
        //Answer to the thanks button
        private void thanks_button(object sender, RoutedEventArgs e)
        {
            Write_Console("You're Welcome !", 0);
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
