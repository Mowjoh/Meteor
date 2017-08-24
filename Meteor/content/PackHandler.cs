using Meteor.database;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace Meteor.content
{
    class PackHandler
    {
        //Class Variables
        public string app_path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
        String packfolder;
        String destinationfolder;
        int workspace;
        private MeteorDatabase meteorDatabase;
        String manual_readme_path = "";

        //Character skin track
        int[] skin_track = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] nameplate_track = new int[] {1,1,1,1,1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        //Consrtuctor
        public PackHandler(MeteorDatabase meteorDatabase, int current_worspace)
        {
            this.meteorDatabase = meteorDatabase;
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
                meteorDatabase.ResetPacker();
                return true;
            }
            else
            {
                return false;
            }
                
        }

        public void pack_skins()
        {
            foreach (Packer packerItem in meteorDatabase.Packers)
            {
                int type = packerItem.content_type;
                int id = packerItem.content_id;
                

                switch (type)
                {
                    case 0:
                        Skin skin = meteorDatabase.Skins.First(s => s.Id == id);
                        Character character = skin.Character;
                        //Values setup
                        int skin_slot = find_subfolder(packfolder + "0/" + character.Id);
                        string skin_slot_text = skin_slot > 99
                            ? skin_slot.ToString()
                            : skin_slot > 9
                                ? "0" + skin_slot
                                : "00" + skin_slot;
                        SkinObject currentSkinObject = new SkinObject(0, id,character.Id , workspace, meteorDatabase);
                        String modelpath = currentSkinObject.model_path;
                        String csppath = currentSkinObject.csp_path;
                        String destination = packfolder + "0/"+ character.Id + "/" + skin_slot_text + "/";
                        String manual_destination = packfolder + "S4E Installation/";

                        String destination_modelpath = destination + "model/";
                        String destination_csppath = destination + "csp/";
                        String destination_metapath = destination + "meta/";

                        String manual_destination_model = manual_destination + "fighter/";
                        String manual_destination_csp = manual_destination + "chr/";
                        String manual_destination_dlccsp = manual_destination + "replace/chr/";


                        //Manual copy
                        copy_manual_model(manual_destination, currentSkinObject);
                        if (currentSkinObject.dlc)
                        {
                            copy_manual_csp(manual_destination_dlccsp, currentSkinObject);
                        }
                        else
                        {
                            copy_manual_csp(manual_destination_csp, currentSkinObject);
                        }

                        //Copy
                        copy_folder(modelpath, destination_modelpath);
                        copy_folder(csppath, destination_csppath);
                        
                        
                        

                        //Meta setup
                        String name = skin.name;
                        String author = skin.author;
                        String gb_uid = "";
                        

                        //Meta create
                        create_meta(destination_metapath,author,name,gb_uid);

                        write_readme(skin.Character.name + " - \"" + name + "\" by "+author+" on model slot "+skin_track[currentSkinObject.char_id]+" and UI slot "+ (skin_track[currentSkinObject.char_id]+1));
                        skin_track[currentSkinObject.char_id]++;

                        break;
                }
                

            }
        }

        public void pack_nameplates()
        {
            foreach (Packer packerItem in meteorDatabase.Packers)
            {
                int type = packerItem.content_type;
                int id = packerItem.content_id;


                switch (type)
                {
                    case 1:
                        Nameplate nameplate = meteorDatabase.Nameplates.First(c => c.Id == id);
                        Character character = nameplate.Character;

                        NameplateObject currentNameplateObject = new NameplateObject(id, character.Id, workspace);
                        int nameplate_slot = find_subfolder(packfolder + "0/" + character.Id);
                        String destination = packfolder + "1/" + character.Id + "/" + nameplate_slot + "/file/";

                        if (!Directory.Exists(destination))
                        {
                            Directory.CreateDirectory(destination);
                        }

                        File.Copy(currentNameplateObject.full_path, destination + currentNameplateObject.filename);

                        String manual_nameplate_destination = packfolder + "S4E Installation/";
                        if (character.dlc)
                        {
                            manual_nameplate_destination += "replace/chr/chrn_11";
                        }
                        else
                        {
                            manual_nameplate_destination += "chr/chrn_11";
                        }
                        copy_manual_nameplate(manual_nameplate_destination, currentNameplateObject);
                        

                        create_meta(packfolder + "1/" + character.Id + "/" + nameplate_slot + "/meta/", nameplate.author, nameplate.name, "");

                        if (nameplate.author == "")
                        {
                            write_readme(character.name + " - \"" + nameplate.name + "\" on UI slot " + nameplate_track[currentNameplateObject.character_id]);

                        }
                        else
                        {
                            write_readme(character.name + " - \"" + nameplate.name + "\" by " + nameplate.author + " on UI slot " + nameplate_track[currentNameplateObject.character_id]);

                        }
                        nameplate_track[currentNameplateObject.character_id]++;
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
        private void copy_manual_model(String destination,SkinObject skinObject)
        {
            Skin skin = meteorDatabase.Skins.First(s => s.Id == skinObject.skin_id);
            String modelname = skin.Character.model_folder;
            String path = destination + modelname + "/model/";
            copy_folder(skinObject.model_path, path);
            String modelslot = skin_track[skinObject.char_id] < 10? "0"+ skin_track[skinObject.char_id].ToString() : skin_track[skinObject.char_id].ToString();
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

        private void copy_manual_csp(String destination, SkinObject skinObject)
        {
            String[] files = Directory.GetFiles(skinObject.csp_path,"*.nut",SearchOption.AllDirectories);
            foreach(String file in files)
            {
                String filename = new FileInfo(file).Name;
                String format = filename.Split('_')[0];
                String number = filename.Split('_')[1];
                String chara = filename.Split('_')[2];

                String subdir = format + "_" + number;

                String slot = skin_track[skinObject.char_id] + 1 <10 ? "0"+(skin_track[skinObject.char_id] + 1).ToString() : (skin_track[skinObject.char_id] + 1).ToString();
                if (!Directory.Exists(destination + subdir))
                {
                    Directory.CreateDirectory(destination + subdir);
                }

                File.Copy(file, destination + subdir + "/" + format + "_" + number + "_" + chara + "_" + slot + ".nut");

            }
        }

        private void copy_manual_nameplate(String destination, NameplateObject nameplateObject)
        {
            String slot = nameplate_track[nameplateObject.character_id] < 10 ? "0" + nameplate_track[nameplateObject.character_id].ToString() : nameplate_track[nameplateObject.character_id].ToString();
            String filename = new FileInfo(nameplateObject.full_path).Name;
            String[] names = filename.Split('_');
            names[3] = slot;
            String fullpath = destination + "/" + String.Join("_", names);
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }
            File.Copy(nameplateObject.full_path,fullpath+".nut");
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
