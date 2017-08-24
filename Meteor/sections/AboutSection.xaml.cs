using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;

namespace Meteor.sections
{
    //About class used as Page.
    public partial class AboutSection
    {
        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;
        private int _thanksCount;

        public AboutSection()
        {
            InitializeComponent();
            LoadLocalVersionNumber();
        }

        //Getting local version number.
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
                    MeteorCode.Message("I'm not saying thanks anymore.");
                    break;
                case 0:
                    MeteorCode.Message("You're Welcome !");
                    break;
                case 1:
                    MeteorCode.Message("You're Welcome !");
                    break;
                case 2:
                    MeteorCode.Message("You're Welcome !");
                    break;
                case 3:
                    MeteorCode.Message("You're Welcome !");
                    break;
                case 4:
                    MeteorCode.Message("You're Welcome !");
                    break;
                case 5:
                    MeteorCode.Message("I said YOU'RE WEL-COME !!!");
                    break;
                case 6:
                    MeteorCode.Message("Sorry. I exagerated a bit.");
                    break;
                case 7:
                    MeteorCode.Message("You're not mad are you?");
                    break;
                case 8:
                    MeteorCode.Message("...");
                    break;
                case 9:
                    MeteorCode.Message("Sure you wanna forgive me?");
                    break;
                case 10:
                    MeteorCode.Message("Okay then but I'm not saying thanks anymore.");
                    break;
                case 20:
                    MeteorCode.Message("Come on. This is no game. Alright, this is for a game, but still...");
                    break;
                case 30:
                    MeteorCode.Message("Stop.");
                    break;
                case 50:
                    MeteorCode.Message("This is bad for both of us. Please stop this madness");
                    break;
                case 90:
                    MeteorCode.Message("Man... This is getting boring.");
                    break;
                case 200:
                    MeteorCode.Message("This won't do anything you know? nothing to achieve here.");
                    break;
                case 1000:
                    MeteorCode.Message("Going for 9000?");
                    break;
                case 9001:
                    MeteorCode.Message("W-W-Wow. It's other 9000!");
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
