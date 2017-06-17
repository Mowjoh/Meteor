using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Meteor.database;
using Meteor.workers;

namespace Meteor
{
    static class MeteorCode
    {
        private static readonly db_handler DbHandler = new db_handler();
        private static readonly Pastebin.Pastebin Pastebin = new Pastebin.Pastebin("f165a49418f0ed6c5f61e9e233889d91");

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
                ((MainWindow)Application.Current.MainWindow).Console.Text = date + " | " + typeText + " | " + s + "\n" + ((MainWindow)Application.Current.MainWindow).Console.Text;
            }
            else
            {
                if (DbHandler.get_property("dev_logs") == "1")
                    ((MainWindow)Application.Current.MainWindow).Console.Text = date + " | " + typeText + " | " + s + "\n" + ((MainWindow)Application.Current.MainWindow).Console.Text;
            }
        }

        public static void Paste(string message, string stack)
        {
            if (DbHandler.get_property("pastebin") == "1")
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
            }
        }

    }
}
