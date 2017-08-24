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
using Meteor.database;
using Meteor.workers;
using System.Windows.Controls;
using System.Xml;

namespace Meteor
{
    static class MeteorCode
    {
        //Public variables
        public static string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;

        //Private variables
        private static readonly Pastebin.Pastebin Pastebin = new Pastebin.Pastebin("f165a49418f0ed6c5f61e9e233889d91");


        //Writes to the console with a status 
        public static void WriteToConsole(string s, int type)
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
                //((MainWindow)Application.Current.MainWindow).Console.Text = date + " | " + typeText + " | " + s + "\n" + ((MainWindow)Application.Current.MainWindow).Console.Text;
            }
            else
            {
                //if (DbHandler.get_property("dev_logs") == "1")
                    //((MainWindow)Application.Current.MainWindow).Console.Text = date + " | " + typeText + " | " + s + "\n" + ((MainWindow)Application.Current.MainWindow).Console.Text;
            }
        }

        //Generates a pastebin and opens a webpage if user confirms
        public static void Paste(string message, string stack)
        {
            /*if (DbHandler.get_property("pastebin") == "1")
            {
                var result = MessageBox.Show("An error happened with Meteor. Open the pastebin?", "Segtendo WARNING",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    var url = Pastebin.CreatePaste("Meteor Error", "csharp", "Error Message : " + message + "\n StackTrace:" + stack, global::Pastebin.PasteExposure.Public, global::Pastebin.PasteExpiration.OneWeek);
                    Process.Start(url);
                }
                catch (Exception ee)
                {
                    MeteorCode.WriteToConsole("Pastebin error : " + ee.Message, 2);
                }
            }*/
        }

        public static void ChangeStatus(string message)
        {
            ((MainWindow) Application.Current.MainWindow).Title = message + " - Meteor";
        }

        public static void Message(string message)
        {
            ((MainWindow) Application.Current.MainWindow).StatusMessage.Text = message;
        }

        public static void SetS4EWorkspacePath(int workspaceId, string s4Epath)
        {
            //Loading local manifest
            var xml = new XmlDocument();
            var workspacePath = AppPath + "/workspaces/workspace_" + workspaceId + "/";
            if (!Directory.Exists(workspacePath + "/content/patch/"))
                Directory.CreateDirectory(workspacePath + "/content/patch/");
            if (File.Exists(s4Epath + "/sm4shmod.xml"))
            {
                xml.Load(s4Epath + "/sm4shmod.xml");
                var node = xml.SelectSingleNode("/Sm4shMod/ProjectWorkplaceFolder");
                if (node == null)
                {
                    var newnode = xml.CreateElement("ProjectWorkplaceFolder");
                    newnode.InnerText = workspacePath;
                    var root = xml.SelectSingleNode("/Sm4shMod");
                    root?.AppendChild(newnode);
                }
                else
                {
                    node.InnerText = workspacePath;
                }

                xml.Save(s4Epath + "/sm4shmod.xml");
            }
            else
            {
                MeteorCode.WriteToConsole("Could not assign Sm4sh Explorer's workspace", 1);
            }
        }
    }
}
