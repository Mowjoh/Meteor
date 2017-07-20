using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;

namespace Meteor.sections
{
    //About class used as Page.
    public partial class About
    {
        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;
        private int _thanksCount;

        public About()
        {
            InitializeComponent();
            LoadLocalVersionNumber();
        }

        private void LoadLocalVersionNumber()
        {
            try
            {
                //Loading local manifest
                var xml2 = new XmlDocument();
                if (File.Exists(AppPath + "/Meteor.exe.manifest"))
                {
                    //Getting the value
                    xml2.Load(AppPath + "/Meteor.exe.manifest");
                    var node = xml2.SelectSingleNode("//*[local-name()='assembly']/*[local-name()='assemblyIdentity']");
                    if (node?.Attributes == null) return;
                    var versionNumber = node.Attributes[1].Value;

                    //Setting the label to the versionNumber
                    AppVersionLabel.Content = "Application Version : " + versionNumber;
                }
            }
            catch(ManifestLoadError manifestLoadError)
            {
                //Pasting the error
                MeteorCode.Paste(manifestLoadError.Message,manifestLoadError.StackTrace);
            }
            

        }

        //Answer to the thanks button
        private void thanks_button(object sender, RoutedEventArgs e)
        {
            //Trigger diffferent messages based on the count
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
            //Launching a webpage
            Process.Start("http://www.mowjoh.com/Main_Page");
        }


    }

    internal abstract class ManifestLoadError : Exception
    {

    }
}
