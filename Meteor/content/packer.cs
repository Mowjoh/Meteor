using Meteor.database;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace Meteor.content
{
    class packer
    {
        //Class Variables
        public string app_path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
        String packfolder;
        String destinationfolder;
        db_handler db;
        int workspace;
        String manual_readme_path = "";

        //Character skin track
        int[] skin_track = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] nameplate_track = new int[] {1,1,1,1,1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        //Consrtuctor
        public packer(db_handler dbs, int current_worspace)
        {
            db = dbs;
            workspace = current_worspace;
            packfolder = app_path + "/packer/";
        }
        

        //Pack functions
        public Boolean pack()
        {
            if (Directory.Exists(packfolder))
            {
                try
                {
                    Directory.Delete(packfolder, true);
                }
                catch
                {
                    CheckFolderDeletion(packfolder);
                }
            }
            Directory.CreateDirectory(packfolder);

            SaveFileDialog sfd = new SaveFileDialog();
            
            sfd.Title = "Pick the archive destination";
            sfd.AddExtension = true;
            sfd.DefaultExt = ".zip";
            sfd.Filter = "Zip Archives (*.zip)|*.zip";


           sfd.ShowDialog();

            if (sfd.FileName != "")
            {
                destinationfolder = sfd.FileName;
                manual_readme_path = packfolder + "S4E Installation/readme.txt";
                write_readme("Contents of the pack :");
                write_readme("");
                write_readme("");
                write_readme("Skins");
                pack_skins();
                write_readme("");
                write_readme("");
                write_readme("Nameplates");
                pack_nameplates();

                archive_folder(packfolder, destinationfolder);
                db.clear_packer_content();
                return true;
            }
            else
            {
                return false;
            }
                
        }

        public void pack_skins()
        {
            ArrayList content_list = db.get_packer_ids();
            foreach (int[] row in content_list)
            {
                int type = row[0];
                int id = row[1];
                

                switch (type)
                {
                    case 0:
                        //Values setup
                        int char_id = db.get_character_id(db.get_character_name(id));
                        int skin_slot = find_subfolder(packfolder + "0/" + char_id);
                        Skin Current_Skin = new Skin(0, id,char_id , workspace, db);
                        String modelpath = Current_Skin.model_path;
                        String csppath = Current_Skin.csp_path;
                        String destination = packfolder + "0/"+char_id+"/"+ skin_slot + "/";
                        String manual_destination = packfolder + "S4E Installation/";

                        String destination_modelpath = destination + "model/";
                        String destination_csppath = destination + "csp/";
                        String destination_metapath = destination + "meta/";

                        String manual_destination_model = manual_destination + "fighter/";
                        String manual_destination_csp = manual_destination + "chr/";
                        String manual_destination_dlccsp = manual_destination + "replace/chr/";


                        //Manual copy
                        copy_manual_model(manual_destination, Current_Skin);
                        if (Current_Skin.dlc)
                        {
                            copy_manual_csp(manual_destination_dlccsp, Current_Skin);
                        }
                        else
                        {
                            copy_manual_csp(manual_destination_csp, Current_Skin);
                        }

                        //Copy
                        copy_folder(modelpath, destination_modelpath);
                        copy_folder(csppath, destination_csppath);
                        
                        
                        

                        //Meta setup
                        String name = db.get_skin_info(id)[0];
                        String author = db.get_skin_info(id)[1];
                        String gb_uid = "";
                        if (author == db.get_property("username"))
                        {
                            gb_uid = db.get_property("gb_uid");
                        }
                        

                        //Meta create
                        create_meta(destination_metapath,author,name,gb_uid);

                        write_readme(db.get_character_name(Current_Skin.skin_id) + " - \"" + name + "\" by "+author+" on model slot "+skin_track[Current_Skin.char_id]+" and UI slot "+ (skin_track[Current_Skin.char_id]+1));
                        skin_track[Current_Skin.char_id]++;

                        break;
                }
                

            }
        }

        public void pack_nameplates()
        {
            ArrayList content_list = db.get_packer_ids();
            foreach (int[] row in content_list)
            {
                int type = row[0];
                int id = row[1];


                switch (type)
                {
                    case 1:
                        int char_id = db.get_character_id_nameplate(id);
                        nameplate current_nameplate = new nameplate(id, char_id, workspace, db);
                        int nameplate_slot = find_subfolder(packfolder + "0/" + char_id);
                        String destination = packfolder + "1/" + char_id + "/" + nameplate_slot + "/file/";

                        if (!Directory.Exists(destination))
                        {
                            Directory.CreateDirectory(destination);
                        }

                        File.Copy(current_nameplate.full_path, destination + current_nameplate.filename);

                        String manual_nameplate_destination = packfolder + "S4E Installation/";
                        if (db.get_character_dlc(char_id))
                        {
                            manual_nameplate_destination += "replace/chr/chrn_11";
                        }
                        else
                        {
                            manual_nameplate_destination += "chr/chrn_11";
                        }
                        copy_manual_nameplate(manual_nameplate_destination, current_nameplate);
                        

                        create_meta(packfolder + "1/" + char_id + "/" + nameplate_slot + "/meta/", db.get_nameplate_info(id)[1], db.get_nameplate_info(id)[0], "");

                        if (db.get_nameplate_info(id)[1] == "")
                        {
                            write_readme(db.get_character_name_nameplate(current_nameplate.nameplate_id) + " - \"" + db.get_nameplate_info(id)[0] + "\" on UI slot " + nameplate_track[current_nameplate.character_id]);

                        }
                        else
                        {
                            write_readme(db.get_character_name_nameplate(current_nameplate.nameplate_id) + " - \"" + db.get_nameplate_info(id)[0] + "\" by " + db.get_nameplate_info(id)[1] + " on UI slot " + nameplate_track[current_nameplate.character_id]);

                        }
                        nameplate_track[current_nameplate.character_id]++;
                        break;
                }

            }
        }

        //Metadata
        private void create_meta(String destination,String author,String name,String gb_uid)
        {
            String metapath = destination + "metadata.xml";
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("metadata");
            xmlDoc.AppendChild(rootNode);

            XmlNode metaNode = xmlDoc.CreateElement("meta");
            XmlAttribute attribute = xmlDoc.CreateAttribute("val");
            attribute.Value = "name";
            metaNode.Attributes.Append(attribute);
            metaNode.InnerText = name;
            rootNode.AppendChild(metaNode);

            metaNode = xmlDoc.CreateElement("meta");
            attribute = xmlDoc.CreateAttribute("val");
            attribute.Value = "author";
            metaNode.Attributes.Append(attribute);
            metaNode.InnerText = author;
            rootNode.AppendChild(metaNode);

            metaNode = xmlDoc.CreateElement("meta");
            attribute = xmlDoc.CreateAttribute("val");
            attribute.Value = "gb_uid";
            metaNode.Attributes.Append(attribute);
            metaNode.InnerText = gb_uid;
            rootNode.AppendChild(metaNode);

            xmlDoc.Save(metapath);
        }


        //File Management
        private void archive_folder(String source, String destination)
        {
            if (Directory.GetDirectories(source).Length > 0)
            {
                //Extracting archive
                var pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Hidden;
                pro.FileName = app_path + "/7za.exe";
                var arguments = "a -tzip \"" + destination + "\" \"" + source + "\"* -mx9";
                pro.Arguments = arguments;
                var x = Process.Start(pro);
                x.WaitForExit();
            }
        }

        private int find_subfolder(String path)
        {
            int i = 1;
            bool found = false;
            while (!found)
            {
                String check = path + "/" + i;
                if (!Directory.Exists(check))
                {
                    found = true;
                }
                else
                {
                    i++;
                }
            }

            return i;
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

        public void copy_folder(string source, string destination)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }


        //Manual operations
        private void copy_manual_model(String destination,Skin skin)
        {
            String modelname = db.get_skin_modelfolder(skin.skin_id);
            String path = destination + modelname + "/model/";
            copy_folder(skin.model_path, path);
            String modelslot = skin_track[skin.char_id] < 10? "0"+ skin_track[skin.char_id].ToString() : skin_track[skin.char_id].ToString();
            String[] dirs = Directory.GetDirectories(path, "*xx",SearchOption.AllDirectories);
            foreach(String dir in dirs)
            {
                copy_folder(dir, dir.Substring(0, dir.Length - 2) + modelslot);
                try
                {
                    Directory.Delete(dir);
                }
                catch
                {
                    CheckFolderDeletion(dir);
                }
            }


        }

        private void copy_manual_csp(String destination, Skin skin)
        {
            String[] files = Directory.GetFiles(skin.csp_path,"*.nut",SearchOption.AllDirectories);
            foreach(String file in files)
            {
                String filename = new FileInfo(file).Name;
                String format = filename.Split('_')[0];
                String number = filename.Split('_')[1];
                String chara = filename.Split('_')[2];

                String subdir = format + "_" + number;

                String slot = skin_track[skin.char_id] + 1 <10 ? "0"+(skin_track[skin.char_id] + 1).ToString() : (skin_track[skin.char_id] + 1).ToString();
                if (!Directory.Exists(destination + subdir))
                {
                    Directory.CreateDirectory(destination + subdir);
                }

                File.Copy(file, destination + subdir + "/" + format + "_" + number + "_" + chara + "_" + slot + ".nut");

            }
        }

        private void copy_manual_nameplate(String destination, nameplate nameplate)
        {
            String slot = nameplate_track[nameplate.character_id] < 10 ? "0" + nameplate_track[nameplate.character_id].ToString() : nameplate_track[nameplate.character_id].ToString();
            String filename = new FileInfo(nameplate.full_path).Name;
            String[] names = filename.Split('_');
            names[3] = slot;
            String fullpath = destination + "/" + String.Join("_", names);
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }
            File.Copy(nameplate.full_path,fullpath+".nut");
        }

        private void write_readme(String line)
        {
            if (!File.Exists(manual_readme_path))
            {
                if(!Directory.Exists(new FileInfo(manual_readme_path).Directory.FullName))
                {
                    Directory.CreateDirectory(new FileInfo(manual_readme_path).Directory.FullName);
                }

            }
            using (StreamWriter sw = File.AppendText(manual_readme_path))
            {
                sw.WriteLine(line);
            }
        }
        
    }
}
