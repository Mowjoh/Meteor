using Meteor.content;
using Meteor.database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meteor.content
{
    class builder
    {
        #region Class Variables

        int workspace_id;
        int region;
        int language;

        String region_code;
        String language_code;
        String datafolder;

        db_handler db;
        uichar_handler uichar;

        public String app_path = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName;

        public String workspace_path;


        ArrayList skins = new ArrayList();
        #endregion

        #region Constructors
        public builder(int workspace, int region, int language,db_handler db,uichar_handler uichar)
        {
            this.workspace_id = workspace;
            this.region = region;
            this.language = language;
            this.db = db;

            this.uichar = uichar;

            this.workspace_path = app_path + "/workspaces/workspace_" + workspace_id;

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
        #endregion

        #region Get Content
        public void get_skins()
        {
            skins = db.get_character_builder_skins_id(workspace_id);
        }
        #endregion

        #region Build content

        #region Skins
        public void build_skins()
        {
            get_skins();
            foreach(int[] skin in skins)
            {
                Skin current = new Skin(skin[1], skin[0], skin[2],this.workspace_id,db);
                String modelpath = current.model_path;
                String csppath = current.csp_path;
                String[] models = db.get_skin_models(skin[0]);
                String[] csps = db.get_skin_csps(skin[0]);
                foreach(String model in models)
                {
                    if(model != "")
                    {
                        String source = modelpath + "/" + model;
                        String modelslotstring = (skin[1] - 1).ToString().Length == 1 ? "0" + (skin[1] - 1).ToString() : (skin[1] - 1).ToString();
                        String model_destination_name = model.Split('/')[1] == "cXX" ? model.Split('/')[0] + "/c" + modelslotstring : model.Split('/')[0] + "/l" + modelslotstring;

                        String destination = app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/data/fighter/" + db.get_skin_modelfolder(skin[0]) + "/model/" + model_destination_name;
                        copy_folder(source, destination);

                        Console.Write("");
                    }
                    
                }

                foreach(String csp in csps)
                {
                    if(csp != "")
                    {
                        String[] files = Directory.GetFiles(csppath, csp + "*", SearchOption.AllDirectories);
                        String filepath = files[0];
                        String filename = System.IO.Path.GetFileName(filepath);
                        String source = csppath + "/" + filename;
                        String cspslotstring = skin[1].ToString().Length == 1 ? "0" + skin[1].ToString() : skin[1].ToString();
                        String data = db.get_character_dlc(skin[2]) ? datafolder : "data";
                        String destination = "";
                        if (db.get_character_dlc(skin[2]))
                        {
                            destination = app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" + data + "/ui/replace/append/chr/" + csp + "/" + filename.Substring(0, filename.Length - 6) + cspslotstring + ".nut";
                            if (!Directory.Exists(app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" + data + "/ui/replace/append/chr/" + csp + "/"))
                            {
                                Directory.CreateDirectory(app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" + data + "/ui/replace/append/chr/" + csp + "/");
                            }
                        }
                        else
                        {
                            destination = app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" + data + "/ui/replace/chr/" + csp + "/" + filename.Substring(0, filename.Length - 6) + cspslotstring + ".nut";
                            if (!Directory.Exists(app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" + data + "/ui/replace/chr/" + csp + "/"))
                            {
                                Directory.CreateDirectory(app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" + data + "/ui/replace/chr/" + csp + "/");
                            }
                        }

                        File.Copy(source, destination, true);
                    }
                    

                    


                }

            }
        }


        #endregion

        #region Config
        public void build_ui_char()
        {
            //If setting to rebuild
            if (db.get_property("reload_ui") == "1")
            {
                //Reload ui_character_db
                ArrayList characters = db.get_characters(0);
                foreach(String character in characters)
                {
                    int id = db.get_character_id(character);
                    int ui_id = db.get_character_uichar_position(id);
                    int count = db.get_character_skins(character, workspace_id.ToString()).Count;

                    uichar.setFile(ui_id, 7, count);
                }
            }

            if(db.get_property("ui_char") == "1")
            {
                //Copy over file
                if (!Directory.Exists(app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" + datafolder + "/param/ui/"))
                {
                    Directory.CreateDirectory(app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" + datafolder + "/param/ui/");
                }
                File.Copy(uichar.filepath, app_path + "/workspaces/workspace_" + workspace_id + "/content/patch/" + datafolder + "/param/ui/ui_character_db.bin", true);
            }
            
            
        }
        #endregion

        #region Actions
        public void build()
        {
            build_skins();
            build_ui_char();
        }

        public void copy_folder(String source, String destination)
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }
        #endregion

        #endregion

        public void s4e_workspace_clean(int mode)
        {
            if (mode == 0)
            {
                clean_workspace();
            }
        }

        public void clean_workspace()
        {
            String workspace_modelpath = workspace_path + "/content/patch/data/fighter/";
            String workspace_csppath = workspace_path + "/content/patch/data/ui/replace/chr/";
            String workspace_dlc_csppath = workspace_path + "/content/patch/"+datafolder+"/ui/replace/append/chr/";

            //Cleaning fighter folders
            if (Directory.Exists(workspace_modelpath))
            {
                String[] fighters = Directory.GetDirectories(workspace_modelpath);
                foreach (String fighter in fighters)
                {
                    String fullpath = fighter + "/model/";
                    if (Directory.Exists(fullpath))
                    {
                        Directory.Delete(fullpath, true);
                        Directory.CreateDirectory(fullpath);
                    }
                }
            }
            

            if (Directory.Exists(workspace_csppath))
            {
                //Cleaning csp folders
                String[] cspfolders = Directory.GetDirectories(workspace_csppath);

                foreach (String cspfolder in cspfolders)
                {
                    String fullpath = cspfolder;
                    String foldername = System.IO.Path.GetFileName(fullpath);
                    if (foldername != "chrn_11" && foldername != "chr_10")
                    {
                        if (Directory.Exists(fullpath))
                        {
                            Directory.Delete(fullpath, true);
                            Directory.CreateDirectory(fullpath);
                        }

                    }
                }
            }
            

            if (Directory.Exists(workspace_dlc_csppath))
            {
                //Cleaning csp folders
                String[] dlccspfolders = Directory.GetDirectories(workspace_dlc_csppath);
                foreach (String cspfolder in dlccspfolders)
                {
                    String fullpath = cspfolder;
                    String foldername = System.IO.Path.GetFileName(fullpath);
                    if (foldername != "chrn_11" | foldername != "chr_10")
                    {
                        if (Directory.Exists(fullpath))
                        {
                            Directory.Delete(fullpath, true);
                            Directory.CreateDirectory(fullpath);
                        }

                    }
                }
            }
            

        }

        public void s4e_export_workspace()
        {
            String s4e_path = db.get_property("s4e_path") + "/content/patch/";
            copy_folder(app_path + "/workspaces/workspace_" + workspace_id, s4e_path);
        }

        public void set_workspace_id(int new_workspace_id)
        {
            this.workspace_path = app_path + "/workspaces/workspace_" + new_workspace_id;
            this.workspace_id = new_workspace_id;
        }

    }
}
