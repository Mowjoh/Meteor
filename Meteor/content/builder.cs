using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Meteor.database;

namespace Meteor.content
{
    public class builder
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
        private readonly db_handler db;
        private readonly uichar_handler uichar;

        //Paths
        public string app_path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
        public string workspace_path;

        //Skin variables
        private ArrayList skins = new ArrayList();
        public long skin_bytes = 0;

        //Constructor
        public builder(int workspace, int region, int language, db_handler db)
        {
            
            workspace_id = workspace;
            this.region = region;
            this.language = language;
            this.db = db;

            this.uichar = new uichar_handler();

            workspace_path = app_path + "/workspaces/workspace_" + workspace_id;

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
            
            build_nameplates();

            if (db.get_property("uichar") == "1")
            {
                build_ui_char();
            }
            
        }

        private void build_skins()
        {
            skin_bytes = 0;
            get_skins();
            foreach (int[] skin in skins)
            {
                var current = new Skin(skin[1], skin[0], skin[2], workspace_id, db);
                var modelpath = current.model_path;
                var csppath = current.csp_path;
                var models = db.get_skin_models(skin[0]);
                var csps = db.get_skin_csps(skin[0]);

                foreach (var model in models)
                    if (model != "")
                    {
                        var source = modelpath + "/" + model;
                        var modelslotstring = (skin[1] - 1).ToString().Length == 1
                            ? "0" + (skin[1] - 1)
                            : (skin[1] - 1).ToString();
                        var model_destination_name = model.Split('/')[1] == "cXX"
                            ? model.Split('/')[0] + "/c" + modelslotstring
                            : model.Split('/')[0] + "/l" + modelslotstring;

                        var destination = app_path + "/workspaces/workspace_" + workspace_id +
                                          "/content/patch/data/fighter/" + db.get_skin_modelfolder(skin[0]) +
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
                        var cspslotstring = skin[1].ToString().Length == 1 ? "0" + skin[1] : skin[1].ToString();
                        var data = db.get_character_dlc(skin[2]) ? region == 1 ? "data" : datafolder : "data";
                        var destination = "";
                        if (db.get_character_dlc(skin[2]))
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
            var characters = db.get_characters(0);
            foreach (string character in characters)
            {
                var id = db.get_character_id(character);
                var ui_id = db.get_character_uichar_position(id);
                var count = db.get_character_skin_count(id, workspace_id);
                if (count >= 8)
                {
                    uichar.setFile(ui_id, 7, count);
                }

            }


            if (db.get_property("ui_char") == "1")
            {
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
        }

        private void build_nameplates()
        {
            int[] chara_track = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int last_id = -1;
            if (db.get_property("uichar") == "1")
            {
                reset_nameplates();
            }
            
            foreach(int[] nameplate in db.get_custom_nameplates())
            {
                int id = nameplate[0];
                int char_id = nameplate[1];
                int slot = nameplate[2];
                int nameplate_slot = 0;
                int ui_char_id = db.get_character_uichar_position(char_id);
                String characterfolder = db.get_character_cspfolder(char_id);
                int basecount = db.get_character_nameplate_count(char_id);
                nameplate_slot = basecount + chara_track[char_id];

                nameplate current_nameplate = new nameplate(id, char_id, workspace_id, db);

                if(last_id != id)
                {
                    last_id = id;
                    chara_track[char_id]++;
                    nameplate_slot = basecount + chara_track[char_id];

                    String slot_text = nameplate_slot < 10 ? "0" + nameplate_slot.ToString() : nameplate_slot.ToString();
                    String filename = new FileInfo(current_nameplate.full_path).Name;

                    var data = db.get_character_dlc(char_id) ? region == 1 ? "data" : datafolder : "data";

                    var destination = "";
                    if (db.get_character_dlc(char_id))
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

                    File.Copy(current_nameplate.full_path, destination,true);
                }

                if (db.get_property("uichar") == "1")
                {
                    int ui_char_slot = slot + 36;
                    if (slot != 0 && slot < 17)
                    {
                        uichar.setFile(ui_char_id, ui_char_slot, nameplate_slot);
                    }
                }
                
                
                

            }
        }

        public void clean_workspace()
        {
            var workspace_modelpath = workspace_path + "/content/patch/data/fighter/";
            var workspace_csppath = workspace_path + "/content/patch/data/ui/replace/chr/";
            var workspace_dlc_csppath = workspace_path + "/content/patch/" + datafolder + "/ui/replace/append/chr/";

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

        private void get_skins()
        {
            skins = db.get_character_builder_skins_id(workspace_id);
        }

        
        //Config functions
        public void set_workspace_id(int new_workspace_id)
        {
            workspace_path = app_path + "/workspaces/workspace_" + new_workspace_id;
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
            for (int char_id =1; char_id < 56; char_id++)
            {
                int ui_char_id = db.get_character_uichar_position(char_id);
                switch (char_id)
                {
                    default:
                        for(int i= 1;i < 17; i++)
                        {
                            int count = base_count + i;
                            if(i < 9)
                            {
                                uichar.setFile(ui_char_id, count, 1);
                            }
                            else
                            {
                                uichar.setFile(ui_char_id, count, 0);
                            }
                        }
                        break;
                    case 7:
                        uichar.setFile(ui_char_id, 37, 1);
                        uichar.setFile(ui_char_id, 38, 2);
                        uichar.setFile(ui_char_id, 39, 3);
                        uichar.setFile(ui_char_id, 40, 4);
                        uichar.setFile(ui_char_id, 41, 5);
                        uichar.setFile(ui_char_id, 42, 6);
                        uichar.setFile(ui_char_id, 43, 7);
                        uichar.setFile(ui_char_id, 44, 8);
                        break;
                    case 12:
                        for (int i = 1; i < 17; i++)
                        {
                            int count = base_count + i;
                            uichar.setFile(ui_char_id, count, 1);
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
                                    uichar.setFile(ui_char_id, count, 1);
                                }
                                else
                                {
                                    uichar.setFile(ui_char_id, count, 2);
                                }
                            }
                            else
                            {
                                uichar.setFile(ui_char_id, count, 0);
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
                                    uichar.setFile(ui_char_id, count, 1);
                                }
                                else
                                {
                                    uichar.setFile(ui_char_id, count, 5);
                                }
                            }
                            else
                            {
                                uichar.setFile(ui_char_id, count, 0);
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
                                    uichar.setFile(ui_char_id, count, 1);
                                }
                                else
                                {
                                    uichar.setFile(ui_char_id, count, 2);
                                }
                            }
                            else
                            {
                                uichar.setFile(ui_char_id, count, 0);
                            }
                        }
                        break;
                }
            }
        }
    }
}