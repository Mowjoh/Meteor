using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Meteor.content;
using Meteor.database;

namespace Meteor.sections.filebank
{
    
    public partial class FilebankPacker 
    {
        private readonly MeteorDatabase meteorDatabase;
        private int ActiveWorkspace { get; }

        public FilebankPacker()
        {
            InitializeComponent();

            meteorDatabase = new MeteorDatabase();

            ActiveWorkspace = int.Parse(meteorDatabase.Configurations.First(c => c.property == "activeWorkspace")
                .value);

            PackerDataGrid.Columns.Add(new DataGridTextColumn()
            {
                Binding = new Binding("type"),
                Header = "Content Type",
                Width = 100
            });
            PackerDataGrid.Columns.Add(new DataGridTextColumn()
            {
                Binding = new Binding("parent"),
                Header = "Parent",
                Width = 100
            });
            PackerDataGrid.Columns.Add(new DataGridTextColumn()
            {
                Binding = new Binding("name"),
                Header = "Name",
                Width = 450
            });
        }

        private void packer_clear_button_Click(object sender, RoutedEventArgs e)
        {
            meteorDatabase.ResetPacker();
            Reload();
        }

        private void PackButtonClick(object sender, RoutedEventArgs e)
        {
            var pacman = new PackHandler(meteorDatabase, ActiveWorkspace);
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
            PackerDataGrid.Items.Clear();

            foreach (Packer packItem in meteorDatabase.Packers)
            {
                switch (packItem.content_type)
                {
                    case 0:
                        Skin skin = meteorDatabase.Skins.First(s => s.Id == packItem.content_id);
                        PackerDataGrid.Items.Add(new PackerDataItem()
                        {
                            name = skin.name,
                            parent = skin.Character.name,
                            type = "Skin"
                        });
                        break;
                    case 1:
                        Nameplate nameplate = meteorDatabase.Nameplates.First(n => n.Id == packItem.content_id);
                        PackerDataGrid.Items.Add(new PackerDataItem()
                        {
                            name = nameplate.name,
                            parent = nameplate.Character.name,
                            type = "Nameplate"
                        });
                        break;
                    case 3:

                        break;
                }
            }
            
        }

    }

    public struct PackerDataItem
    {
        public string type { get; set; }
        public string parent { get; set; }
        public string name { get; set; }
    }
}
