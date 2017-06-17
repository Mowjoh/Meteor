using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;
using Meteor.content;
using Meteor.database;
using SharpCompress.Common;
using SharpCompress.Reader;
using newpath = System.IO.Path;

namespace Meteor.workers
{
    public class DownloadWorker : Worker
    {
        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;
        private int _progressStyle = 1;

        private String _downloadUrl;

        public int LastInstallCharacterId { get; set; }
        public int LastInstallSkinSlot { get; set; }

        //Constructor
        public DownloadWorker(db_handler databaseHandler) : base(databaseHandler)
        {
            _worker.WorkerReportsProgress = true;
        }

        protected internal void Launch(String url)
        {
            _downloadUrl = url;
            _worker.RunWorkerAsync();
        }

        protected override void WorkerDowork(object sender, DoWorkEventArgs e)
        {
            base.WorkerDowork(sender, e);

            if (!Directory.Exists(AppPath + "/downloads/"))
            {
                Directory.CreateDirectory(AppPath + "/downloads/");
            }
            else
            {
                try
                {
                    Directory.Delete(AppPath + "/downloads/", true);
                }
                catch
                {
                    CheckFolderDeletion(AppPath + "/downloads/");
                }
                Directory.CreateDirectory(AppPath + "/downloads/");
            }

            var link = _downloadUrl.Substring(7, _downloadUrl.Length - 7);
            var extension = _downloadUrl.Split('.')[_downloadUrl.Split('.').Length - 1];

            WebClient webby = new WebClient();
            Uri ur = new Uri(link);
            webby.DownloadProgressChanged += WebClientDownloadProgressChanged;
            webby.DownloadFileCompleted += WebClientDownloadCompleted;
            webby.DownloadFileAsync(ur, AppPath + "/downloads/archive." + extension);

            void WebClientDownloadProgressChanged(object senders, DownloadProgressChangedEventArgs es)
            {
                _worker.ReportProgress(es.ProgressPercentage);
            }

            void WebClientDownloadCompleted(object sender2, AsyncCompletedEventArgs e2)
            {
                var source = AppPath + "/downloads/archive." + extension;
                var dest = AppPath + "/downloads/archive/";
                if (extension == "rar")
                {
                    while (IsFileLocked(new FileInfo(source)))
                    {

                    }

                    //Extracting archive
                    using (Stream stream = File.OpenRead(source))
                    {
                        var reader = ReaderFactory.Open(stream);
                        while (reader.MoveToNextEntry())
                            if (!reader.Entry.IsDirectory)
                                reader.WriteEntryToDirectory(dest,
                                    ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                        reader.Dispose();
                    }

                    while (IsFileLocked(new FileInfo(source)))
                    {

                    }
                    AddExtractedContent();
                }
                else
                {
                    var arguments = "x \"" + source + "\" -o\"" + dest + "\" * -r";

                    //Extracting archive
                    var pro = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = AppPath + "/7za.exe",
                        Arguments = arguments
                    };
                    
                    var x = Process.Start(pro);
                    x?.WaitForExit();
                    while (IsFileLocked(new FileInfo(source)))
                    {

                    }


                    AddExtractedContent();
                }
            }

            while (webby.IsBusy)
            {
            }
        }

        //Download Code
        private void AddExtractedContent()
        {
            var subfolders = Directory.GetDirectories(AppPath + "/downloads/archive/");
            var mslFolders = new ArrayList();
            if (subfolders.Length > 0)
            {
                foreach (var s in subfolders)
                {
                    var name = newpath.GetFileName(s);
                    if (DbHandler.check_msl_character_name(name))
                    {
                        mslFolders.Add(s);
                    }

                }
                //MSL tagged content
                if (mslFolders.Count > 0)
                {
                    foreach (string s in mslFolders)
                    {
                        var name = newpath.GetFileName(s);
                        AddMslSkins(s, name);
                    }
                }
                else
                {
                    //Must be Meteor packed
                    foreach (var s in subfolders)
                    {

                        if (!int.TryParse(newpath.GetFileName(s), out int contentType)) continue;

                        switch (contentType)
                        {
                            //Skins
                            case 0:
                                var subs = Directory.GetDirectories(s);
                                foreach (var subf in subs)
                                {
                                    //Foreach character
                                    if (!int.TryParse(newpath.GetFileName(subf), out int charId)) continue;

                                    var skinFolders = Directory.GetDirectories(subf);
                                    foreach (var skinFolder in skinFolders)
                                    {
                                        AddMeteorSkin(charId, skinFolder);
                                    }
                                }
                                break;
                            case 1:
                                var subs1 = Directory.GetDirectories(s);
                                foreach (var subf in subs1)
                                {
                                    //Foreach character
                                    if (!int.TryParse(newpath.GetFileName(subf), out int charId1)) continue;
                                    var nameplateFolders = Directory.GetDirectories(subf);
                                    foreach (var nameplateFolder in nameplateFolders)
                                    {
                                        AddMeteorNameplate(charId1, nameplateFolder);
                                    }
                                }
                                break;
                            default:

                                break;
                        }
                    }
                }

            }

        }

        private void AddMslSkins(string charaPath, string charaName)
        {
            var meteor_skins = Directory.GetDirectories(charaPath);
            foreach (var skin in meteor_skins)
            {
                var skin_folder = newpath.GetFileName(skin);

                try
                {
                    var ActiveWorkspace = int.Parse(DbHandler.get_property("workspace"));
                    var skin_name = skin.Split('_')[2];
                    if (skin_folder.Split('_')[0] == "meteor" && skin_folder.Split('_')[1] == "xx" &&
                        skin_folder.Split('_').Length == 3)
                    {
                        var character_id = DbHandler.get_character_id_msl(charaName);
                        var skin_count = DbHandler.get_character_skin_count(character_id, ActiveWorkspace);
                        var last_id = DbHandler.add_skin(skin_name, "", "", "", character_id, skin_count + 1);

                        var created_skin = new Skin(skin_count + 1, last_id, character_id, ActiveWorkspace, DbHandler);
                        created_skin.get_models(skin + "/model/");
                        created_skin.get_csps(skin + "/csp/");

                        var xmlpath = skin + "/meta/meta.xml";
                        if (File.Exists(xmlpath))
                        {
                            var xml = new XmlDocument();
                            xml.Load(xmlpath);
                            var nodes = xml.SelectSingleNode("/metadata/meta[attribute::name='author']");
                            var author = nodes.InnerText;
                            var names = xml.SelectSingleNode("/metadata/meta[attribute::name='name']");
                            var name = names.InnerText;

                            DbHandler.set_skin_author(author, last_id);
                            DbHandler.set_skin_gb_uid(-1, last_id);
                            DbHandler.set_skin_name(name, last_id);

                            LastInstallCharacterId = character_id;
                            LastInstallSkinSlot = skin_count + 1;
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void AddMeteorSkin(int charId, string path)
        {
            try
            {
                var ActiveWorkspace = int.Parse(DbHandler.get_property("workspace"));
                var skin_name = "";
                var skin_count = DbHandler.get_character_skin_count(charId, ActiveWorkspace);
                var last_id = DbHandler.add_skin(skin_name, "", "", "", charId, skin_count + 1);

                var created_skin = new Skin(skin_count + 1, last_id, charId, ActiveWorkspace, DbHandler);
                created_skin.get_models(path + "/model/");
                created_skin.get_csps(path + "/csp/");

                var xmlpath = path + "/meta/metadata.xml";
                if (File.Exists(xmlpath))
                {
                    var xml = new XmlDocument();
                    xml.Load(xmlpath);
                    var nodes = xml.SelectSingleNode("/metadata/meta[attribute::val='author']");
                    var author = nodes.InnerText;
                    var names = xml.SelectSingleNode("/metadata/meta[attribute::val='name']");
                    var name = names.InnerText;
                    var gb_uids = xml.SelectSingleNode("/metadata/meta[attribute::val='gb_uid']");
                    var gb_uid = names.InnerText;

                    int gbuid;
                    DbHandler.set_skin_author(author, last_id);
                    DbHandler.set_skin_gb_uid(-1, last_id);
                    DbHandler.set_skin_name(name, last_id);
                    if (int.TryParse(gb_uid, out gbuid))
                    {
                        DbHandler.set_skin_gb_uid(gbuid, last_id);
                    }


                    LastInstallCharacterId = charId;
                    LastInstallSkinSlot = skin_count + 1;
                }
                else
                {
                    DbHandler.set_skin_author("Error parsing Author", last_id);
                    DbHandler.set_skin_name("No Name", last_id);
                }

            }
            catch
            {
            }
        }

        private void AddMeteorNameplate(int charId, string path)
        {
            try
            {
                var activeWorkspace = int.Parse(DbHandler.get_property("workspace"));
                var fileFolder = path + "/file/";
                var filePath = Directory.GetFiles(fileFolder)[0];
                var createdNameplate = new nameplate(filePath, charId, activeWorkspace, DbHandler);

                var xmlpath = path + "/meta/metadata.xml";
                if (File.Exists(xmlpath))
                {
                    var xml = new XmlDocument();
                    xml.Load(xmlpath);
                    var nodes = xml.SelectSingleNode("/metadata/meta[attribute::val='author']");
                    var author = nodes.InnerText;
                    var names = xml.SelectSingleNode("/metadata/meta[attribute::val='name']");
                    var name = names.InnerText;
                    var gb_uid = "";

                    DbHandler.set_nameplate_author(author, createdNameplate.nameplate_id);
                    DbHandler.set_nameplate_name(name, createdNameplate.nameplate_id);

                }
            }
            catch
            {
            }
        }

        //Folder Code
        private void CheckFolderDeletion(String path)
        {
            if (Directory.Exists(path))
            {
                setAttributesNormal(new DirectoryInfo(path));
                try
                {
                    Directory.Delete(path, true);
                }
                catch
                {
                    // ignored
                }
            }
            else
            {

            }
        }

        private void setAttributesNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                setAttributesNormal(subDir);
                subDir.Attributes = FileAttributes.Normal;
            }
            foreach (var file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                stream?.Close();
            }

            //file is not locked
            return false;
        }

    }
}
