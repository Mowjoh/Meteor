using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Meteor.database;

namespace Meteor.content
{
    public class ContentBuilder
    {
        //Setup variables
        private int workspace_id;
        private int region;
        private int language;

        //Region variables
        private readonly string region_code;
        private readonly string language_code;
        private readonly string datafolder;

        //Handlers
        private readonly MeteorDatabase meteorDatabase;
        private readonly uichar_handler uichar;

        //Paths
        public string app_path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
        public string WorkspacePath;

        //Skin variables
        public long skin_bytes = 0;

        //Constructor
        public ContentBuilder(int workspace, int region, int language, MeteorDatabase db)
        {
            
            workspace_id = workspace;
            this.region = region;
            this.language = language;
            meteorDatabase = db;

            this.uichar = new uichar_handler();

            WorkspacePath = app_path + "/workspaces/workspace_" + workspace_id;

            switch (region)
            {
                case 0:
                    region_code = "eu";
                    break;
                case 1:
                    region_code = "us";
                    break;
                case 2:
                    region_code = "jp";
                    break;
            }

            switch (language)
            {
                case 0:
                    language_code = "en";
                    break;
                case 1:
                    language_code = "fr";
                    break;
                case 2:
                    language_code = "sp";
                    break;
                case 3:
                    language_code = "gr";
                    break;
                case 4:
                    language_code = "it";
                    break;
                case 5:
                    language_code = "ne";
                    break;
                case 6:
                    language_code = "po";
                    break;
                case 7:
                    language_code = "jp";
                    break;
            }

            datafolder = "data(" + region_code + "_" + language_code + ")";
        }


        //Builder code
        public void build()
        {
            build_skins();
            
            //build_nameplates();

            if (meteorDatabase.ConfigurationFiles.First(cf => cf.name == "ui_character_db.bin").present)
            {
                build_ui_char();
            }
            
        }

        private void build_skins()
        {
            
            skin_bytes = 0;

            int activeWorkspace = int.Parse(meteorDatabase.Configurations.First(c => c.property == "activeWorkspace")
                .value);

            foreach (SkinLibrary libraryEntry in meteorDatabase.SkinLibraries.Where(sl => sl.library_lock == false && sl.workspace_id == activeWorkspace ))
            {
                Skin skin = libraryEntry.Skin;
                Character character = libraryEntry.Character;
                Nameplate nameplate = libraryEntry.Nameplate;

                var current = new SkinObject(libraryEntry.slot, skin.Id, character.Id, workspace_id, meteorDatabase);
                var modelpath = current.model_path;
                var csppath = current.csp_path;
                
                var models = skin.skin_models.Split(';');
                var csps = skin.skin_csps.Split(';');

                foreach (var model in models)
                    if (model != "")
                    {
                        var source = modelpath + "/" + model;
                        var modelslotstring = (libraryEntry.slot - 1).ToString().Length == 1
                            ? "0" + (libraryEntry.slot - 1)
                            : (libraryEntry.slot - 1).ToString();

                        var model_destination_name = model.Split('/')[1] == "cXX"
                            ? model.Split('/')[0] + "/c" + modelslotstring
                            : model.Split('/')[0] + "/l" + modelslotstring;

                        var destination = app_path + "/workspaces/workspace_" + workspace_id +
                                          "/content/patch/data/fighter/" + character.model_folder +
                                          "/model/" + model_destination_name;
                        copy_folder(source, destination);

                        Console.Write("");
                    }

                foreach (var csp in csps)
                    if (csp != "")
                    {
                        var files = Directory.GetFiles(csppath, csp + "*", SearchOption.AllDirectories);
                        var filepath = files[0];
                        var filename = Path.GetFileName(filepath);
                        var source = csppath + "/" + filename;
                        var cspslotstring = libraryEntry.slot.ToString().Length == 1 ? "0" + libraryEntry.slot : libraryEntry.slot.ToString();
                        var data = character.dlc ? region == 1 ? "data" : datafolder : "data";
                        var destination = "";
                        if (character.dlc)
                        {
                            destination = app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" +
                                          data + "/ui/replace/append/chr/" + csp + "/" +
                                          filename.Substring(0, filename.Length - 6) + cspslotstring + ".nut";
                            if (!Directory.Exists(app_path + "/workspaces/workspace_" + workspace_id +
                                                  "/content/patch/" + data + "/ui/replace/append/chr/" + csp + "/"))
                                Directory.CreateDirectory(app_path + "/workspaces/workspace_" + workspace_id +
                                                          "/content/patch/" + data + "/ui/replace/append/chr/" + csp +
                                                          "/");
                        }
                        else
                        {
                            destination = app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" +
                                          data + "/ui/replace/chr/" + csp + "/" +
                                          filename.Substring(0, filename.Length - 6) + cspslotstring + ".nut";
                            if (!Directory.Exists(app_path + "/workspaces/workspace_" + workspace_id +
                                                  "/content/patch/" + data + "/ui/replace/chr/" + csp + "/"))
                                Directory.CreateDirectory(app_path + "/workspaces/workspace_" + workspace_id +
                                                          "/content/patch/" + data + "/ui/replace/chr/" + csp + "/");
                        }

                        File.Copy(source, destination, true);
                        if (csp == "chr_13")
                        {
                            skin_bytes += new FileInfo(source).Length;
                        }

                    }
            }
        }

        private void build_ui_char()
        {
            //Reload ui_character_db
            foreach (Character character in meteorDatabase.Characters)
            {
                var count = meteorDatabase.SkinLibraries.Count(sl => sl.workspace_id == workspace_id && sl.character_id == character.Id);
                if (count >= 8)
                {
                    uichar.setFile(character.ui_character_db_id, 7, count);
                }

            }

            //Copy over file
            if (!Directory.Exists(app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" +
                                    datafolder + "/param/ui/"))
                Directory.CreateDirectory(app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" +
                                            datafolder + "/param/ui/");

            try
            {
                var path = app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" + datafolder +
                "/param/ui/ui_character_db.bin";
                File.Copy(uichar.filepath, path, true);
            }
            catch (Exception e)
            {
                var mess = e.Message;
                var stack = e.StackTrace;
                var x = "";
            }

            
        }

        private void build_nameplates()
        {
            int[] chara_track = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int last_id = -1;
            if (meteorDatabase.Configurations.First(c => c.property == "ui_character_db").value == "1")
            {
                reset_nameplates();
            }
            

            foreach(SkinLibrary entry in meteorDatabase.SkinLibraries.Where(sl => sl.workspace_id == workspace_id))
            {
                Nameplate nameplate = entry.Nameplate;
                Character character = entry.Character;

                int nameplateSlot = 0;
                nameplateSlot = character.nameplate_count + chara_track[character.Id];

                NameplateObject currentNameplateObject = new NameplateObject(nameplate.Id, character.Id, workspace_id);

                if(last_id != nameplate.Id)
                {
                    last_id = nameplate.Id;
                    chara_track[character.Id]++;
                    nameplateSlot = character.nameplate_count + chara_track[character.Id];

                    String slot_text = nameplateSlot < 10 ? "0" + nameplateSlot.ToString() : nameplateSlot.ToString();
                    String filename = new FileInfo(currentNameplateObject.full_path).Name;

                    var data = character.dlc ? region == 1 ? "data" : datafolder : "data";

                    var destination = "";
                    if (character.dlc)
                    {
                        destination = app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" +
                                      data + "/ui/replace/append/chr/chrn_11/" +
                                      filename.Substring(0, filename.Length - 6) + slot_text + ".nut";
                        if (!Directory.Exists(app_path + "/workspaces/workspace_" + workspace_id +
                                              "/content/patch/" + data + "/ui/replace/append/chr/chrn_11/"))
                            Directory.CreateDirectory(app_path + "/workspaces/workspace_" + workspace_id +
                                                      "/content/patch/" + data + "/ui/replace/append/chr/chrn_11/");
                    }
                    else
                    {
                        destination = app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" +
                                      data + "/ui/replace/chr/chrn_11/" +
                                      filename.Substring(0, filename.Length - 6) + slot_text + ".nut";
                        if (!Directory.Exists(app_path + "/workspaces/workspace_" + workspace_id +
                                              "/content/patch/" + data + "/ui/replace/chr/chrn_11/"))
                            Directory.CreateDirectory(app_path + "/workspaces/workspace_" + workspace_id +
                                                      "/content/patch/" + data + "/ui/replace/chr/chrn_11/");
                    }

                    File.Copy(currentNameplateObject.full_path, destination,true);
                }

                if (meteorDatabase.Configurations.First(c => c.property == "ui_character_db").value == "1")
                {
                    int uiCharSlot = entry.slot + 36;
                    if (entry.slot != 0 && entry.slot < 17)
                    {
                        uichar.setFile(character.ui_character_db_id, uiCharSlot, nameplateSlot);
                    }
                }
                
                
                

            }
        }

        public void clean_workspace()
        {
            var workspace_modelpath = WorkspacePath + "/content/patch/data/fighter/";
            var workspace_csppath = WorkspacePath + "/content/patch/data/ui/replace/chr/";
            var workspace_dlc_csppath = WorkspacePath + "/content/patch/" + datafolder + "/ui/replace/append/chr/";

            //Cleaning fighter folders
            if (Directory.Exists(workspace_modelpath))
            {
                var fighters = Directory.GetDirectories(workspace_modelpath);
                foreach (var fighter in fighters)
                {
                    var fullpath = fighter + "/model/";
                    if (Directory.Exists(fullpath))
                    {
                        try
                        {
                            Directory.Delete(fullpath, true);
                        }
                        catch
                        {
                            CheckFolderDeletion(fullpath);
                        }


                        Directory.CreateDirectory(fullpath);
                    }
                }
            }


            if (Directory.Exists(workspace_csppath))
            {
                //Cleaning csp folders
                var cspfolders = Directory.GetDirectories(workspace_csppath);

                foreach (var cspfolder in cspfolders)
                {
                    var fullpath = cspfolder;
                    var foldername = Path.GetFileName(fullpath);
                    if (foldername != "chr_10")
                        if (Directory.Exists(fullpath))
                        {
                            try
                            {
                                Directory.Delete(fullpath, true);
                            }
                            catch
                            {
                                CheckFolderDeletion(fullpath);
                            }

                            Directory.CreateDirectory(fullpath);
                        }
                }
            }


            if (Directory.Exists(workspace_dlc_csppath))
            {
                //Cleaning csp folders
                var dlccspfolders = Directory.GetDirectories(workspace_dlc_csppath);
                foreach (var cspfolder in dlccspfolders)
                {
                    var fullpath = cspfolder;
                    var foldername = Path.GetFileName(fullpath);
                    if ((foldername != "chr_10"))
                        if (Directory.Exists(fullpath))
                        {
                            try
                            {
                                Directory.Delete(fullpath, true);
                            }
                            catch
                            {
                                CheckFolderDeletion(fullpath);
                            }
                            Directory.CreateDirectory(fullpath);
                        }
                }
            }
        }

        
        //Config functions
        public void set_workspace_id(int new_workspace_id)
        {
            WorkspacePath = app_path + "/workspaces/workspace_" + new_workspace_id;
            workspace_id = new_workspace_id;
        }

        //File management
        private void copy_folder(string source, string destination)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }

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

        //Nameplates ui_char
        private void reset_nameplates()
        {
            int base_count = 36;
            for (int charId =1; charId < 56; charId++)
            {
                int uiCharId = meteorDatabase.Characters.First(c => c.Id == charId).ui_character_db_id;
                switch (charId)
                {
                    default:
                        for(int i= 1;i < 17; i++)
                        {
                            int count = base_count + i;
                            if(i < 9)
                            {
                                uichar.setFile(uiCharId, count, 1);
                            }
                            else
                            {
                                uichar.setFile(uiCharId, count, 0);
                            }
                        }
                        break;
                    case 7:
                        uichar.setFile(uiCharId, 37, 1);
                        uichar.setFile(uiCharId, 38, 2);
                        uichar.setFile(uiCharId, 39, 3);
                        uichar.setFile(uiCharId, 40, 4);
                        uichar.setFile(uiCharId, 41, 5);
                        uichar.setFile(uiCharId, 42, 6);
                        uichar.setFile(uiCharId, 43, 7);
                        uichar.setFile(uiCharId, 44, 8);
                        break;
                    case 12:
                        for (int i = 1; i < 17; i++)
                        {
                            int count = base_count + i;
                            uichar.setFile(uiCharId, count, 1);
                        }
                        break;
                    case 39:
                        for (int i = 1; i < 17; i++)
                        {
                            int count = base_count + i;
                            if (i < 9)
                            {
                                if (i % 2 != 0)
                                {
                                    uichar.setFile(uiCharId, count, 1);
                                }
                                else
                                {
                                    uichar.setFile(uiCharId, count, 2);
                                }
                            }
                            else
                            {
                                uichar.setFile(uiCharId, count, 0);
                            }
                        }
                        break;
                    case 40:
                        for (int i = 1; i < 17; i++)
                        {
                            int count = base_count + i;
                            if (i < 9)
                            {
                                
                                if(i < 5)
                                {
                                    uichar.setFile(uiCharId, count, 1);
                                }
                                else
                                {
                                    uichar.setFile(uiCharId, count, 5);
                                }
                            }
                            else
                            {
                                uichar.setFile(uiCharId, count, 0);
                            }
                        }
                        break;
                    case 41:
                        for (int i = 1; i < 17; i++)
                        {
                            int count = base_count + i;
                            if (i < 9)
                            {
                                if (i % 2 != 0)
                                {
                                    uichar.setFile(uiCharId, count, 1);
                                }
                                else
                                {
                                    uichar.setFile(uiCharId, count, 2);
                                }
                            }
                            else
                            {
                                uichar.setFile(uiCharId, count, 0);
                            }
                        }
                        break;
                }
            }
        }
    }
}