using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Meteor.updates
{
    class Updater
    {
        private string AppPath { get; set; }

        public Updater(string appPath)
        {
            AppPath = appPath;

            CheckUpdate();
        }

        //New updater
        private void CheckUpdate()
        {
            try
            {
                //Loading local manifest
                var xml2 = new XmlDocument();
                var local_version = "";
                if (File.Exists(AppPath + "/Meteor.exe.manifest"))
                {
                    xml2.Load(AppPath + "/Meteor.exe.manifest");
                    var nodes2 =
                        xml2.SelectSingleNode("//*[local-name()='assembly']/*[local-name()='assemblyIdentity']");
                    var version2 = nodes2.Attributes[1].Value;
                    local_version = version2.Replace('.', '_');
                }
                else
                {
                    local_version = "0_0_0_0";
                }


                //Searching for last update
                var last_version = GetLastVersion();

                //Loading remote manifest
                var xml = new XmlDocument();
                xml.Load("http://mowjoh.com/meteor/Application Files/Meteor_" + last_version + "/Meteor.exe.manifest");
                var nodes = xml.SelectSingleNode("//*[local-name()='assembly']/*[local-name()='assemblyIdentity']");
                var version = nodes.Attributes[1].Value;
                var remote_version = version.Replace('.', '_');

                if (CheckIfNewer(local_version, remote_version))
                    Update();
            }
            catch
            {
            }
        }

        private bool CheckIfNewer(string localversion, string remoteversion)
        {
            try
            {
                var l_major = int.Parse(localversion.Split('_')[0]);
                var l_minor = int.Parse(localversion.Split('_')[1]);
                var l_build = int.Parse(localversion.Split('_')[2]);
                var l_revision = int.Parse(localversion.Split('_')[3]);

                var r_major = int.Parse(remoteversion.Split('_')[0]);
                var r_minor = int.Parse(remoteversion.Split('_')[1]);
                var r_build = int.Parse(remoteversion.Split('_')[2]);
                var r_revision = int.Parse(remoteversion.Split('_')[3]);

                //remote major is superior
                if (r_major > l_major)
                    return true;
                if (r_minor > l_minor)
                    return true;
                if (r_build > l_build)
                    return true;
                if (r_revision > l_revision)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetLastVersion()
        {
            try
            {
                //Getting remote info
                var remote_path = "http://mowjoh.com/meteor/Application Files/patchnotes.xml";
                var xml = new XmlDocument();
                xml.Load(remote_path);
                var nodes = xml.SelectSingleNode("package");
                var version = nodes.Attributes[0].Value;
                version = version.Replace('.', '_');
                return version;
            }
            catch (Exception)
            {
                return "";
            }
        }

        private void Update()
        {
            var result = MessageBox.Show("An update is available. Proceed with the update?", "Segtendo WARNING",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var pro = new ProcessStartInfo();
                pro.FileName = AppPath + "/Meteor Updater.exe";
                var x = Process.Start(pro);
                Application.Current.Shutdown();
            }
        }
    }
}
