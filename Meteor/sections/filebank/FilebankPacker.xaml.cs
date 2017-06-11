using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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
                WriteToConsole("Archive Packed", 0);
            }
            else
            {
                WriteToConsole("Archive wasn't packed", 2);
            }
        }

        public void Reload()
        {
            PackerDataGrid.ItemsSource = _dbHandler.get_packer_content().Tables[0].DefaultView;

        }

        //Console Writing
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
