using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Meteor.database;
using Meteor.sections.filebank;

namespace Meteor.sections
{
    /// <summary>
    /// Interaction logic for Filebank.xaml
    /// </summary>
    public partial class Filebank 
    {
        private readonly db_handler _dbHandler;
        public Filebank()
        {
            InitializeComponent();
             _dbHandler = new db_handler();
        }

        private void filebank_action_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var li = (ListBoxItem)FilebankSectionListBox.SelectedItem;
            var val = li.Content.ToString();

            switch (val)
            {
                case "Skins":
                    FilebankContentArea.SelectedItem = FilebankContentArea.Items[0];
                    WriteToConsole("Filebank changed to Skins", 3);
                    
                    FilebankNameplates packerNameplate = (FilebankNameplates)FilebankNameplateFrame.Content;
                    packerNameplate.ReloadNameplates();
                    break;
                case "Packer":
                    FilebankContentArea.SelectedItem = FilebankContentArea.Items[2];
                    WriteToConsole("Filebank changed to Packer", 3);

                    FilebankPacker packerPage = (FilebankPacker)FilebankPackerFrame.Content;
                    packerPage.Reload();
                    break;

                case "Nameplates":
                    FilebankContentArea.SelectedItem = FilebankContentArea.Items[1];
                    WriteToConsole("Filebank changed to Nameplates", 3);

                    FilebankNameplates nameplatePage = (FilebankNameplates) FilebankNameplateFrame.Content;
                    nameplatePage.ReloadNameplates();
                    break;

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
    }
}
