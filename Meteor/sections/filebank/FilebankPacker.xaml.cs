using System.Windows;
using Meteor.content;
using Meteor.database;

namespace Meteor.sections.filebank
{
    
    public partial class FilebankPacker 
    {
        private readonly db_handler _dbHandler;
        private int ActiveWorkspace { get; }

        public FilebankPacker()
        {
            InitializeComponent();

            _dbHandler = new db_handler();

            ActiveWorkspace = int.Parse(_dbHandler.get_property("workspace"));
        }

        private void packer_clear_button_Click(object sender, RoutedEventArgs e)
        {
            _dbHandler.clear_packer_content();
            Reload();
        }

        private void PackButtonClick(object sender, RoutedEventArgs e)
        {
            var pacman = new packer(_dbHandler, ActiveWorkspace);
            if (pacman.pack())
            {
                            MeteorCode.WriteToConsole("Archive Packed", 0);
            }
            else
            {
                            MeteorCode.WriteToConsole("Archive wasn't packed", 2);
            }
        }

        public void Reload()
        {
            PackerDataGrid.ItemsSource = _dbHandler.get_packer_content().Tables[0].DefaultView;

        }

    }
}
