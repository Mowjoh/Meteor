using System.Windows.Controls;
using Meteor.database;
using Meteor.sections.filebank;

namespace Meteor.sections
{
    /// <summary>
    /// Interaction logic for Filebank.xaml
    /// </summary>
    public partial class FilebankSection 
    {
        public FilebankSection()
        {
            InitializeComponent();
        }

        private void filebank_action_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var li = (ListBoxItem)FilebankSectionListBox.SelectedItem;
            var val = li.Content.ToString();

            switch (val)
            {
                case "Skins":
                    FilebankContentArea.SelectedItem = FilebankContentArea.Items[0];
                                MeteorCode.WriteToConsole("Filebank changed to Skins", 3);
                    
                    FilebankNameplates packerNameplate = (FilebankNameplates)FilebankNameplateFrame.Content;
                    packerNameplate.ReloadNameplates();
                    break;
                case "Packer":
                    FilebankContentArea.SelectedItem = FilebankContentArea.Items[2];
                                MeteorCode.WriteToConsole("Filebank changed to Packer", 3);

                    FilebankPacker packerPage = (FilebankPacker)FilebankPackerFrame.Content;
                    packerPage.Reload();
                    break;

                case "Nameplates":
                    FilebankContentArea.SelectedItem = FilebankContentArea.Items[1];
                                MeteorCode.WriteToConsole("Filebank changed to Nameplates", 3);

                    FilebankNameplates nameplatePage = (FilebankNameplates) FilebankNameplateFrame.Content;
                    nameplatePage.ReloadNameplates();
                    break;

            }
        }
    }
}
