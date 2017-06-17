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
        private int _thanksCount;

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
            switch (_thanksCount)
            {
                default:
                    MeteorCode.WriteToConsole("I'm not saying thanks anymore.", 0);
                    break;
                case 0:
                    MeteorCode.WriteToConsole("You're Welcome !", 0);
                    break;
                case 1:
                    MeteorCode.WriteToConsole("You're Welcome !", 0);
                    break;
                case 2:
                    MeteorCode.WriteToConsole("You're Welcome !", 0);
                    break;
                case 3:
                    MeteorCode.WriteToConsole("You're Welcome !", 0);
                    break;
                case 4:
                    MeteorCode.WriteToConsole("You're Welcome !", 0);
                    break;
                case 5:
                    MeteorCode.WriteToConsole("I said YOU'RE WEL-COME !!!", 0);
                    break;
                case 6:
                    MeteorCode.WriteToConsole("Sorry. I exagerated a bit.", 0);
                    break;
                case 7:
                    MeteorCode.WriteToConsole("You're not mad are you?", 0);
                    break;
                case 8:
                    MeteorCode.WriteToConsole("...", 0);
                    break;
                case 9:
                    MeteorCode.WriteToConsole("Sure you wanna forgive me?", 0);
                    break;
                case 10:
                    MeteorCode.WriteToConsole("Okay then but I'm not saying thanks anymore.", 0);
                    break;
                case 20:
                    MeteorCode.WriteToConsole("Come on. This is no game. Alright, this is for a game, but still...", 0);
                    break;
                case 30:
                    MeteorCode.WriteToConsole("Stop.", 0);
                    break;
                case 50:
                    MeteorCode.WriteToConsole("This is bad for both of us. Please stop this madness", 0);
                    break;
                case 90:
                    MeteorCode.WriteToConsole("Man... This is getting boring.", 0);
                    break;
                case 200:
                    MeteorCode.WriteToConsole("This won't do anything you know? nothing to achieve here.", 0);
                    break;
                case 1000:
                    MeteorCode.WriteToConsole("Going for 9000?", 0);
                    break;
                case 9001:
                    MeteorCode.WriteToConsole("W-W-Wow. It's other 9000!", 0);
                    break;

            }
            _thanksCount++;
        }

        //Launch the wiki web page
        private void goto_wiki(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.mowjoh.com/Main_Page");
        }


    }
}
