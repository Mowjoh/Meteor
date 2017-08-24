using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using Meteor.content;
using SharpCompress.Archives;
using SharpCompress.Readers;
using newpath = System.IO.Path;

namespace Meteor.workers
{
    public class DownloadWorker : Worker
    {
        private string AppPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;

        private String _downloadUrl;
        private bool downloaded;

        public int LastInstallCharacterId { get; set; }
        public int LastInstallSkinSlot { get; set; }

        private WebClient webby;

        //Constructor
        public DownloadWorker(MeteorDatabase meteorDatabase) : base(meteorDatabase)
        {
            _worker.WorkerReportsProgress = true;
            Name = "DownloadWorker";
            downloaded = false;

            webby = new WebClient();
            webby.DownloadProgressChanged += WebClientDownloadProgressChanged;
            webby.DownloadFileCompleted += WebClientDownloadCompleted;
        }

        protected internal void Launch(String url)
        {
            _downloadUrl = url;
            _worker.RunWorkerAsync();
            Status = 1;
            Style = 1;
            Message = "Downloading file";
        }

        protected override void WorkerDowork(object sender, DoWorkEventArgs e)
        {
            base.WorkerDowork(sender, e);

            RecreateDownloadFolder();

            var link = _downloadUrl.Substring(7, _downloadUrl.Length - 7);
            var extension = _downloadUrl.Split('.')[_downloadUrl.Split('.').Length - 1];

            Uri ur = new Uri(link);

            webby.DownloadFileAsync(ur, AppPath + "/downloads/archive." + extension);

            while (!downloaded)
            {

            }
            downloaded = false;
        }

        //Downloader Functions
        private void WebClientDownloadProgressChanged(object senders, DownloadProgressChangedEventArgs es)
        {
            try
            {
                Status = 2;
                _worker.ReportProgress(es.ProgressPercentage);
            }
            catch
            {
            }
        }

        private void WebClientDownloadCompleted(object sender2, AsyncCompletedEventArgs e2)
        {
            ExtractArchive();
            AddExtractedContent();
            downloaded = true;
            Status = 3;
        }

        private void ExtractArchive()
        {
            Style = 0;
            Message = "Extracting archive";

            var extension = _downloadUrl.Split('.')[_downloadUrl.Split('.').Length - 1];
            var source = AppPath + "/downloads/archive." + extension;
            var dest = AppPath + "/downloads/archive/";

            //Extracting archive
            if (extension != "7z")
            {
                using (Stream stream = File.OpenRead(source))
                {
                    var reader = ReaderFactory.Open(stream);
                    int count = 0;
                    
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            reader.WriteEntryToDirectory(dest,
                                new ExtractionOptions() {ExtractFullPath = true, Overwrite = true});
                        }
                    }
                }
            }
            else
            {
                var archive = ArchiveFactory.Open(source);
                var reader = archive.ExtractAllEntries();
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                        reader.WriteEntryToDirectory(dest, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                }
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

        private void RecreateDownloadFolder()
        {
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
        }

        //Content Code
        private void AddExtractedContent()
        {
            Message = "Adding Content";
            var subfolders = Directory.GetDirectories(AppPath + "/downloads/archive/");
            var mslFolders = new ArrayList();
            if (subfolders.Length > 0)
            {
                foreach (var s in subfolders)
                {
                    var name = newpath.GetFileName(s);
                    if (meteorDatabase.Characters.Any(c => c.msl_name == name))
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
            var meteorSkins = Directory.GetDirectories(charaPath);
            foreach (var path in meteorSkins)
            {
                var skinFolder = newpath.GetFileName(path);

                try
                {
                    //Getting variables
                    var activeWorkspace = int.Parse(meteorDatabase.Configurations
                        .First(c => c.property == "activeWorkspace").value);
                    var skinName = path.Split('_')[2];

                    //Checking meteor folder name
                    if (skinFolder.Split('_')[0] == "meteor" && skinFolder.Split('_')[1] == "xx" &&
                        skinFolder.Split('_').Length == 3)
                    {
                        //Setting up variables
                        Character character = meteorDatabase.Characters.First(c => c.msl_name == charaName);
                        var skinCount = character.SkinLibraries.Count(sl => sl.workspace_id == activeWorkspace);

                        //Adding skin to database
                        Skin skin = new Skin()
                        {
                            name = skinName,
                            character_id = character.Id,
                            skinLock = false,
                            skin_models = "",
                            skin_csps = "",
                            gb_uid = 0
                        };
                        meteorDatabase.Skins.Add(skin);
                        meteorDatabase.SaveChanges();

                        //Instanciating skin object
                        var createdSkin = new SkinObject(skinCount + 1, skin.Id, character.Id, activeWorkspace,
                            meteorDatabase);

                        createdSkin.AddModelsFromPath(path + "/model/");
                        createdSkin.AddCspsFromPath(path + "/csp/");

                        //Getting XML Data
                        var xmlpath = path + "/meta/meta.xml";
                        if (File.Exists(xmlpath))
                        {
                            var xml = new XmlDocument();
                            xml.Load(xmlpath);
                            var nodes = xml.SelectSingleNode("/metadata/meta[attribute::name='author']");
                            var author = nodes.InnerText;
                            var names = xml.SelectSingleNode("/metadata/meta[attribute::name='name']");
                            var name = names.InnerText;

                            skin.author = author;
                            skin.gb_uid = -1;
                            skin.name = name;


                            LastInstallCharacterId = character.Id;
                            LastInstallSkinSlot = skinCount + 1;
                        }
                    }
                    meteorDatabase.SaveChanges();
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
                var ActiveWorkspace = int.Parse(meteorDatabase.Configurations
                    .First(c => c.property == "activeWorkspace").value);
                var skin_name = "";
                var skin_count =
                    meteorDatabase.SkinLibraries.Count(
                        sl => sl.workspace_id == ActiveWorkspace && sl.character_id == charId);
                Skin skin = new Skin()
                {
                    name = "",
                    character_id = charId,
                    skinLock = false,
                    skin_models = "",
                    skin_csps = "",
                    gb_uid = 0
                };
                meteorDatabase.Skins.Add(skin);
                meteorDatabase.SaveChanges();

                SkinLibrary entry = new SkinLibrary()
                {
                    character_id = skin.character_id,
                    skin_id = skin.Id,
                    slot = skin_count + 1,
                    workspace_id = ActiveWorkspace
                };
                meteorDatabase.SkinLibraries.Add(entry);
                meteorDatabase.SaveChanges();

                var created_skin = new SkinObject(skin_count + 1, skin.Id, charId, ActiveWorkspace, meteorDatabase);
                created_skin.AddModelsFromPath(path + "/model/");
                created_skin.AddCspsFromPath(path + "/csp/");

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
                    skin.author = author;
                    skin.gb_uid = -1;
                    skin.name = name;
                    if (int.TryParse(gb_uid, out gbuid))
                    {
                        skin.gb_uid = gbuid;
                    }


                    LastInstallCharacterId = charId;
                    LastInstallSkinSlot = skin_count + 1;
                }
                else
                {
                    skin.author = "parse Error";
                    skin.name = "no name";
                }
                meteorDatabase.SaveChanges();
            }
            catch
            {
            }
        }

        private void AddMeteorNameplate(int charId, string path)
        {
            try
            {
                var ActiveWorkspace = int.Parse(meteorDatabase.Configurations
                    .First(c => c.property == "activeWorkspace").value);
                var fileFolder = path + "/file/";
                var filePath = Directory.GetFiles(fileFolder)[0];
                var createdNameplate = new NameplateObject(filePath, charId, ActiveWorkspace);
                Nameplate nameplate = new Nameplate()
                {
                    name = "new Nameplate",
                    character_id = charId
                };
                meteorDatabase.Nameplates.Add(nameplate);

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

                    nameplate.name = name;
                }
            }
            catch
            {
            }
        }
    }
}
