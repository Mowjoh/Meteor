using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Xml;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace Meteor_Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {

        #region Variables
        //Setting class variables
        String version;
        String patchnotes;
        String last_version;
        ArrayList failed_files = new ArrayList();
        ArrayList success_files = new ArrayList();
        private String app_path = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName;
        ArrayList messages = new ArrayList();

        private readonly BackgroundWorker worker = new BackgroundWorker();

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            write("wolcoom to the updooter");

            //Getting last version info
            last_version = get_lastest_ver();

            //Write patch info
            write_patch();

            //Setting up the workers
            worker_setup();

            //Launching the update process
            worker.RunWorkerAsync();
        }


        #region Console
        //Writes a string to the console
        private void write(String s)
        {
            console.Text += s + "\n";
        }
        //Writes the patch contents
        private void write_patch()
        {
            //Getting remote info
            String remote_path = "http://meteor.mowjoh.com/Application Files/patchnotes.xml";
            XmlDocument xml = new XmlDocument();
            xml.Load(remote_path);
            XmlNode nodes = xml.SelectSingleNode("package");
            XmlNodeList patches = xml.SelectNodes("package/patchnote");
            version = nodes.Attributes[0].Value;
            patchnotes = nodes.InnerText;


            write("This will update to version " + version + "\n");



            foreach (XmlElement patch in patches)
            {
                write("Patch " + patch.Attributes["version"].Value);
                XmlNodeList patchnodes = xml.SelectNodes("package/patchnote[attribute::version='" + patch.Attributes["version"].Value + "']/patch");
                foreach (XmlElement xe in patchnodes)
                {
                    write("- " + xe.InnerText + "\n");
                }
            }

        }
        #endregion

        #region Worker Functions
        private void worker_setup()
        {
            worker.DoWork += update_worker_DoWork;
            worker.ProgressChanged += update_worker_ProgressChanged;
            worker.RunWorkerCompleted += update_worker_RunWorkerCompleted;
        }

        private void update_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            update();
        }

        private void update_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressbar.Value = e.ProgressPercentage;
            if (progressbar.Value == 50)
            {
                write("- Download Completed");
            }
            if (progressbar.Value == 75)
            {
                write("- Archive Extracted");
            }
        }

        private void update_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            foreach (String s in success_files)
            {
                write("Updated following file: " + s);

            }
            foreach (String s in failed_files)
            {
                write("Failed to update following file: " + s);
            }

            if (failed_files.Count == 0)
            {
                replace_manifest();
                status_text.Content = "Update completed";
            }else
            {
                status_text.Content = "Update failed, please retry";
                console.Text = "";
                foreach(String s in messages)
                {
                    write(s);
                }
            }

            

            launch_button.IsEnabled = true;
        }
        #endregion

        #region Updater
        //Gets patchnotes lastest version
        private string get_lastest_ver()
        {
            //Getting remote info
            String remote_path = "http://meteor.mowjoh.com/Application Files/patchnotes.xml";
            XmlDocument xml = new XmlDocument();
            xml.Load(remote_path);
            XmlNode nodes = xml.SelectSingleNode("package");
            String version = nodes.Attributes[0].Value.ToString();
            version = version.Replace('.', '_');
            return version;

        }

        //True if newer
        private Boolean compare_version(String localversion, String remoteversion)
        {
            int l_major = int.Parse(localversion.Split('_')[0]);
            int l_minor = int.Parse(localversion.Split('_')[1]);
            int l_build = int.Parse(localversion.Split('_')[2]);
            int l_revision = int.Parse(localversion.Split('_')[3]);

            int r_major = int.Parse(remoteversion.Split('_')[0]);
            int r_minor = int.Parse(remoteversion.Split('_')[1]);
            int r_build = int.Parse(remoteversion.Split('_')[2]);
            int r_revision = int.Parse(remoteversion.Split('_')[3]);

            //remote major is superior
            if (r_major > l_major)
            {
                return true;
            }
            else
            {
                if (r_minor > l_minor)
                {
                    return true;
                }
                else
                {
                    if (r_build > l_build)
                    {
                        return true;
                    }
                    else
                    {
                        if (r_revision > l_revision)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }

        //Launches the update process
        private void update()
        {
            try
            {
                //Getting local version info
                XmlDocument local_xml = new XmlDocument();
                String local_version = "";

                if (File.Exists(app_path + "/Meteor.exe.manifest"))
                {
                    local_xml.Load(app_path + "/Meteor.exe.manifest");
                    XmlNode nodes = local_xml.SelectSingleNode("//*[local-name()='assembly']/*[local-name()='assemblyIdentity']");
                    local_version = nodes.Attributes[1].Value;
                    local_version = local_version.Replace('.', '_');
                }
                else
                {
                    local_version = "0_0_0_0";
                }

                //Getting remote info
                String remote_path = "http://meteor.mowjoh.com/Application Files/Meteor_" + last_version +
                                     "/updatefiles.xml";
                XmlDocument files_xml = new XmlDocument();
                files_xml.Load(remote_path);
                XmlNodeList file_list = files_xml.SelectNodes("package/file");

                foreach (XmlElement xe in file_list)
                {
                    String filepath = xe.InnerText;
                    String fileversion = xe.Attributes[0].Value.ToString();
                    String downloadpath = "http://mowjoh.com/meteor/Application Files/Meteor_" + last_version + "/" +
                                          filepath;
                    String destinationpath = System.IO.Path.GetDirectoryName(app_path + "/" + filepath);
                    if (!Directory.Exists(destinationpath))
                    {
                        Directory.CreateDirectory(destinationpath);
                    }

                    if (compare_version(local_version, fileversion))
                    {
                        //Getting file
                        using (WebClient webClient = new WebClient())
                        {
                            try
                            {
                                webClient.DownloadFile(new Uri(downloadpath), app_path + "/" + filepath);
                                success_files.Add(filepath);
                            }
                            catch (Exception e)
                            {
                                messages.Add(e.Message);
                                messages.Add(e.StackTrace);
                                failed_files.Add(filepath);
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                messages.Add(e.Message);
                messages.Add(e.StackTrace);
            }
        }

        //Saves the new manifest
        private void replace_manifest()
        {
            String remote_path = "http://meteor.mowjoh.com/Application Files/Meteor_" + last_version + "/Meteor.exe.manifest";
            XmlDocument files_xml = new XmlDocument();
            files_xml.Load(remote_path);
            files_xml.Save(app_path + "/Meteor.exe.manifest");
        }
        #endregion

        #region Actions

        //Launch MMSL
        private void launch_meteor(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.FileName = app_path + "/Meteor.exe";
                Process.Start(pro);
                Application.Current.Shutdown();
            }
            catch
            {
                write("The updater couldn't launch Meteor Skin Library");
            }

        }


        #endregion

    }
}
