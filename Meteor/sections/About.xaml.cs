using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Xml;
using Meteor.database;

namespace Meteor.sections
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Page
    {
        private readonly db_handler _dbHandler;
        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;

        public About()
        {
            InitializeComponent();

            _dbHandler = new db_handler();
            LoadLocalVersionNumber();
        }

        private void LoadLocalVersionNumber()
        {
            //Loading local manifest
            var xml2 = new XmlDocument();
            if (File.Exists(AppPath + "/Meteor.exe.manifest"))
            {
                xml2.Load(AppPath + "/Meteor.exe.manifest");

                var nodes2 = xml2.SelectSingleNode("//*[local-name()='assembly']/*[local-name()='assemblyIdentity']");

                if (nodes2?.Attributes == null) return;

                var version2 = nodes2.Attributes[1].Value;
                AppVersionLabel.Content = "Application Version : " + version2;
            }
            else
            {
            }

        }

        //Answer to the thanks button
        private void thanks_button(object sender, RoutedEventArgs e)
        {
            WriteToConsole("You're Welcome !", 0);
        }

        //Launch the wiki web page
        private void goto_wiki(object sender, RoutedEventArgs e)
        {
            Process.Start("https://mowjoh.com");
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
