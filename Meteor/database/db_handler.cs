using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Meteor.database
{
    class db_handler
    {
        #region Class Variables
        SQLiteConnection db_connection;
        #endregion

        #region Constructors
        public db_handler()
        {
            
            db_connection = new SQLiteConnection("Data Source=Library.sqlite;Version=3;");
            db_connection.Open();
        }
        #endregion

        #region Get

        #region Characters
        //Gets the list of characters by game order or alphabetical order
        public ArrayList get_characters(int mode)
        {
            ArrayList characters = new ArrayList();
            string sql = "";

            switch (mode)
            {
                case 1:
                    sql = "select name from characters";
                    break;
                case 0:
                    sql = "select name from characters order by name asc";
                    break;
            }


            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                characters.Add(reader.GetString(0));
            }

            return characters;
        }

        //Gets character id
        public int get_character_id(String char_name)
        {
            int id = 0;
            String sql = "Select id from characters where name = @name";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", char_name);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            return id;
        }

        public int get_character_id_msl(String msl_name)
        {
            int id = 0;
            String sql = "Select id from characters where msl_name = @name";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", msl_name);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            return id;
        }

        public int get_character_uichar_position(int char_id)
        {
            int id = 0;
            String sql = "Select ui_char_db_id from characters where id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", char_id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            return id;
        }

        public Boolean check_msl_character_name(String foldername)
        {
            Boolean result = false;

            String sql = "Select id from characters where msl_name = @foldername";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("foldername", foldername);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result = true;
            }

            return result;
        }

        public int get_character_skin_count(int character_id,int workspace_id)
        {
            int id = 0;
            String sql = "Select count(*) from skin_library where character_id = @character_id and workspace_id = @workspace_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            return id;
        }
        #endregion

        #region Skins
        //gets the skin list for a specific character name
        public ArrayList get_character_skins(String char_name, String workspace_id)
        {
            ArrayList skins = new ArrayList();

            String sql = "select skins.name from skin_library Join skins on(skins.id = skin_library.skin_id) Join characters on(characters.id = skin_library.character_id) Where characters.name = @char_name and skin_library.workspace_id = @workspace_id Order by skin_library.Slot";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_name", char_name);
            command.Parameters.AddWithValue("workspace_id", Int32.Parse(workspace_id));
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                skins.Add(reader.GetString(0));
            }

            return skins;
        }

        //gets the skin list for a specific character name
        public Boolean get_character_dlc(int char_id)
        {
            Boolean test = false;

            String sql = "select dlc from characters where id = @char_id";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_id", char_id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int val = reader.GetInt32(0);
                if (val == 1)
                {
                    test = true;
                }
            }

            return test;
        }

        //gets the custom skin list for a specific character name
        public ArrayList get_character_custom_skins(String char_name, String workspace_id)
        {
            ArrayList skins = new ArrayList();

            String sql = "select skins.name from skins Join characters on(characters.id = skins.character_id) Where characters.name = @char_name and skins.locked = 0 Order by skins.id";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_name", char_name);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                skins.Add(reader.GetString(0));
            }

            return skins;
        }

        //gets the custom skin list for a specific character name
        public ArrayList get_character_custom_skins_id(String char_name, String workspace_id)
        {
            ArrayList skins = new ArrayList();

            String sql = "select skins.id from skins Join characters on(characters.id = skins.character_id) Where characters.name = @char_name and skins.locked = 0 Order by skins.id";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_name", char_name);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                skins.Add(reader.GetInt32(0));
            }

            return skins;
        }

        //gets the custom skin list for a specific character name
        public ArrayList get_character_builder_skins_id(int workspace_id)
        {
            ArrayList skins = new ArrayList();

            String sql = "select skins.id, skin_library.slot,skins.character_id from skins Join skin_library on(skin_library.skin_id = skins.id) Where skins.locked = 0 and skin_library.workspace_id = @workspace_id Order by skins.id";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int[] val = new int[] { reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2) };
                skins.Add(val);
            }

            return skins;
        }

        //Get's a skin id based on char id and slot
        public int get_skin_id(int char_id, int slot,int workspace_id)
        {
            String sql = "Select skin_id from skin_library where character_id = @char_id and slot = @slot and workspace_id = @workspace_id";
            int id = 0;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("char_id", char_id);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            return id;
        }

        //Gets a skin gb_uid based on char id and slot
        public int get_skin_gb_id(int skin_id)
        {
            String sql = "Select gb_id from skins where id = @skin_id";
            int id = 0;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            return id;
        }

        //Gets a skin gb_uid based on char id and slot
        public int get_skin_gb_uid(int skin_id)
        {
            String sql = "Select gb_uid from skins where id = @skin_id";
            int id = 0;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            return id;
        }

        //Gets the skin info
        public String[] get_skin_info(int skin_id)
        {
            String[] info;

            String sql = "select name, author, models, csps from skins where id = @skin_id";
            String name = "";
            String author = "";
            String models = "";
            String csps = "";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                name = reader.GetString(0);
                author = reader.GetString(1);
                if (!reader.IsDBNull(2))
                {
                    models = reader.GetString(2);
                }

                if (!reader.IsDBNull(3))
                {
                    csps = reader.GetString(3);
                }

            }

            info = new String[] { name, author, models, csps };
            return info;
        }

        public Boolean skin_locked(int skin_id)
        {
            Boolean test = false;

            String sql = "Select locked from skins where id = @skin_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                test = reader.GetInt32(0) == 1? true: false;
            }

            return test;
        }

        public Boolean skin_locked(int slot,int character_id)
        {
            Boolean test = false;

            String sql = "Select locked from skin_library where slot = @slot and character_id = @character_id and workspace_id = @workspace_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("workspace_id", get_property("workspace"));
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                test = reader.GetInt32(0) == 1 ? true : false;
            }

            return test;
        }

        public DataSet get_custom_skins()
        {
            DataSet ds = new DataSet();

            String sql = "Select name as 'Skin Name', author as 'Skin Author',models as 'Model Files',csps as 'Csp files',gb_uid as 'Gamebanana User Id', id as 'Skin ID' from skins where locked = 0";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();

            SQLiteDataAdapter sqlda = new SQLiteDataAdapter(sql,db_connection);
            sqlda.Fill(ds);

            return ds;
        }

        public String[] get_skin_models(int skin_id)
        {
            String[] skin_models;
            String sql = "select models from skins where id = @skin_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            SQLiteDataReader reader = command.ExecuteReader();
            String var = "";
            while (reader.Read())
            {
                var = reader.GetString(0);
            }
            skin_models = var.Split(';');
            return skin_models;
        }

        public String[] get_skin_csps(int skin_id)
        {
            String[] skin_csps;
            String sql = "select csps from skins where id = @skin_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            SQLiteDataReader reader = command.ExecuteReader();
            String var = "";
            while (reader.Read())
            {
                var = reader.GetString(0);
            }
            skin_csps = var.Split(';');
            return skin_csps;
        }

        public Boolean check_skin_in_library(int skin_id)
        {
            Boolean test = false;

            String sql = "Select slot from skin_library where skin_id = @skin_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                test = reader.GetInt32(0) > 0 ? true : false;
            }

            return test;
        }

        public String get_skin_modelfolder(int skin_id)
        {
            String folder = "";


            String sql = "Select characters.model_folder from characters join skins on(skins.character_id = characters.id) where skins.id = @skin_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", skin_id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                folder = reader.GetString(0);
            }

            return folder;
        }

        public int get_skin_id_hash(String skin_hash)
        {
            String sql = "Select id from skins where model_hash_1 = @skin_hash or model_hash_2 = @skin_hash or csp_hash_1 = @skin_hash or csp_hash_2 = @skin_hash or csp_hash_3 = @skin_hash or csp_hash_4 = @skin_hash ";
            int id = 0;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_hash", skin_hash);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            return id;

        }


        #endregion

        #region Workspaces
        public ArrayList get_workspaces()
        {
            ArrayList workspaces = new ArrayList();
            string sql = "select name from workspaces where slot != 1";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                workspaces.Add(reader.GetString(0));
            }

            return workspaces;
        }
        public Boolean workspace_default(int slot)
        {
            Boolean result = false;
            string sql = "select locked from workspaces where slot = @slot";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result = reader.GetInt32(0) == 1 ? true : false;
            }

            return result;
        }
        public int[] get_workspace_stats(int slot)
        {
            int[] stats;
            int skin_count =0;
            string sql = "select count(*) from skin_library join workspaces on (workspaces.id = skin_library.workspace_id) where skin_library.locked = 0 and  workspaces.slot = @slot";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                skin_count = reader.GetInt32(0);
            }
            stats = new int[] { skin_count };
            return stats;
        }
        public int get_workspace_id(int slot)
        {
            int id =0;
            string sql = "select id from workspaces where slot = @slot";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }
            return id;
        }

        public int get_workspace_slot(int id)
        {
            int slot = 0;
            string sql = "select slot from workspaces where id = @id";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                slot = reader.GetInt32(0);
            }
            return slot;
        }
        public String get_workspace_name(int id)
        {
            String name = "";
            string sql = "select name from workspaces where id = @id";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", id);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                name = reader.GetString(0);
            }
            return name;
        }

        #endregion

        #region Config
        //Gets a property value
        public String get_property(String property)
        {
            String value = "";
            String sql = "select value from configuration where property = @property";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("property", property);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                value = reader.GetString(0);
            }

            return value;
        }
        #endregion

        #endregion

        #region Set
        public void set_skin_name(String name,int id)
        {
            String sql = "update skins set name = @name where id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void set_skin_author(String author, int id)
        {
            String sql = "update skins set author = @author where id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("author", author);
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void set_skin_gb_uid(int val, int skin_id)
        {
            String sql = "update skins set gb_uid = @gb_uid  where id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("gb_uid", val);
            command.Parameters.AddWithValue("id", skin_id);
            command.ExecuteNonQuery();
        }

        public void set_property_value(String value,String property)
        {
            String sql = "update configuration set value = @value where property =  @property";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("value", value);
            command.Parameters.AddWithValue("property", property);
            command.ExecuteNonQuery();
        }

        public void set_skin_hash(int id,int hash_id, String value)
        {
            String hashname = "";
            switch (hash_id)
            {
                case 0:
                    hashname = "model_hash_1";
                    break;
                case 1:
                    hashname = "model_hash_2";
                    break;
                case 2:
                    hashname = "csp_hash_1";
                    break;
                case 3:
                    hashname = "csp_hash_2";
                    break;
                case 4:
                    hashname = "csp_hash_3";
                    break;
                case 5:
                    hashname = "csp_hash_4";
                    break;

            }
            String sql = "update skins set @hashname = @value where id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("hashname", hashname);
            command.Parameters.AddWithValue("value", value);
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void set_workspace_name(String name, int slot)
        {
            String sql = "update workspaces set name = @name where slot = @slot";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("slot", slot);
            command.ExecuteNonQuery();
        }
        #endregion
        
        #region Insert

        #region Skins
        public int add_skin(String name, String author, String models, String csps, int character_id, int slot)
        {
            String sql = "insert into skins (name,author,models,csps,character_id,locked,gb_uid) values(@name,@author,@models,@csps,@character_id,0,0)";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("author", author);
            command.Parameters.AddWithValue("models", models);
            command.Parameters.AddWithValue("csps", csps);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("slot", slot);
            command.ExecuteNonQuery();

            long id = db_connection.LastInsertRowId;

            sql = "insert into skin_library (workspace_id,character_id,skin_id,slot,locked) values (" + get_property("workspace") + "," + character_id + "," + id + "," + slot + ",0)";
            command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();

            return Convert.ToInt32(id);
        }

        public void insert_skin(int skin_id,int workspace_id,int char_id,int slot)
        {
            String sql = "insert into skin_library (workspace_id,character_id,skin_id,slot,locked) values (@workspace_id,@char_id,@skin_id,@slot,0)";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            command.Parameters.AddWithValue("skin_id", skin_id);
            command.Parameters.AddWithValue("char_id", char_id);
            command.Parameters.AddWithValue("slot", slot);
            command.ExecuteNonQuery();
        }

        public void convert_skin(String name, String author, String models, String csps, int character_id, int slot)
        {
            String sql = "insert into skins (name,author,models,csps,character_id,locked,gb_uid) values(@name,@author,@models,@csps,@character_id,0,0)";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("author", author);
            command.Parameters.AddWithValue("models", models);
            command.Parameters.AddWithValue("csps", csps);
            command.Parameters.AddWithValue("character_id", character_id);
            command.ExecuteNonQuery();

            long id = db_connection.LastInsertRowId;

            sql = "update skin_library set skin_id = @skin_id where slot = @slot and workspace_id = @workspace_id and character_id = @character_id";
            command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", id);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("workspace_id", get_property("workspace"));

            command.ExecuteNonQuery();
        }

        public void add_default_skins(long id)
        {

            ArrayList lines = new ArrayList();
            String sql = "select character_id,skin_id,slot from skin_library Where skin_library.workspace_id = 1";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int[] line = new int[] { reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2) };
                lines.Add(line);
            }

            foreach (int[] line in lines)
            {
                sql = "insert into skin_library (character_id,skin_id,slot,locked,workspace_id) values (@character_id,@skin_id,@slot,1,@id)";
                command = new SQLiteCommand(sql, db_connection);
                command.Parameters.AddWithValue("character_id", line[0]);
                command.Parameters.AddWithValue("skin_id", line[1]);
                command.Parameters.AddWithValue("slot", line[2]);
                command.Parameters.AddWithValue("id", id);
                command.ExecuteNonQuery();
            }
        }

        public void copy_skins(long id_source,long id_dest)
        {
            clear_workspace(Convert.ToInt32(id_dest));

            ArrayList lines = new ArrayList();
            String sql = "select character_id,skin_id,slot,locked from skin_library Where skin_library.workspace_id = "+id_source;

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int[] line = new int[] { reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3) };
                lines.Add(line);
            }

            foreach (int[] line in lines)
            {
                sql = "insert into skin_library (character_id,skin_id,slot,locked,workspace_id) values (@character_id,@skin_id,@slot,1,@id)";
                command = new SQLiteCommand(sql, db_connection);
                command.Parameters.AddWithValue("character_id", line[0]);
                command.Parameters.AddWithValue("skin_id", line[1]);
                command.Parameters.AddWithValue("slot", line[2]);
                command.Parameters.AddWithValue("locked", line[3]);
                command.Parameters.AddWithValue("id", id_dest);
                command.ExecuteNonQuery();
            }
        }

        public void add_model(int skin_id, String model_name)
        {
            String[] models = get_skin_models(skin_id);
            Boolean present = false;
            String newmodels = "";
            foreach (String m in models)
            {
                if(m != "")
                {
                    newmodels += m + ";";
                }
                
                if(m == model_name)
                {
                    present = true;
                }
            }

            if (!present)
            {
                newmodels += model_name;
                String sql = "update skins set models = @newmodels where id = @skin_id";
                SQLiteCommand command = new SQLiteCommand(sql, db_connection);
                command.Parameters.AddWithValue("newmodels", newmodels);
                command.Parameters.AddWithValue("skin_id", skin_id);
                command.ExecuteNonQuery();
            }

        }

        public void add_csp(int skin_id, String csp_type)
        {
            String[] csps = get_skin_csps(skin_id);
            Boolean present = false;
            String newcsps = "";
            foreach (String c in csps)
            {
                if (c != "")
                {
                    newcsps += c + ";";
                }

                if (c == csp_type)
                {
                    present = true;
                }
            }

            if (!present)
            {
                newcsps += csp_type;
                String sql = "update skins set csps = @newcsps where id = @skin_id";
                SQLiteCommand command = new SQLiteCommand(sql, db_connection);
                command.Parameters.AddWithValue("newcsps", newcsps);
                command.Parameters.AddWithValue("skin_id", skin_id);
                command.ExecuteNonQuery();
            }

        }

        public void restore_default(int slot,int char_id,String workspace)
        {
            String sql = "select skin_id from skin_library where slot = @slot and character_id = @char_id and workspace_id = 1";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("char_id", char_id);
            SQLiteDataReader reader = command.ExecuteReader();
            int id = 0;
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            sql = "update skin_library set skin_id = @id where slot = @slot and character_id = @char_id and workspace_id = @workspace";
            command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("char_id", char_id);
            command.Parameters.AddWithValue("workspace", workspace);
            command.ExecuteNonQuery();
        }

        public void replace_skin(int new_id,int char_id, int slot,int workspace_id)
        {
            

            String sql = "update skin_library set  skin_id = @skin_id where character_id = @character_id and slot = @slot and workspace_id = @workspace_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("skin_id", new_id);
            command.Parameters.AddWithValue("character_id", char_id);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            command.ExecuteNonQuery();
        }

       
        #endregion

        #region Workspace
        public long add_workspace(String name, int slot)
        {
            String sql = "insert into workspaces (name,slot,locked) values (@name,@slot,0)";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("slot", slot);
            command.ExecuteNonQuery();

            return db_connection.LastInsertRowId;
        }
        #endregion

        #endregion

        #region Remove
        internal void remove_skin(int slot,int character_id, string selected_workspace)
        {
            String sql = "delete from skin_library where slot = @slot and character_id = @character_id and workspace_id = @workspace_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("slot", slot);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("workspace_id", selected_workspace);
            command.ExecuteNonQuery();
        }

        internal void delete_skin(int skin_id)
        {
            String sql = "delete from skins where id = @skin_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("id", skin_id);
            command.ExecuteNonQuery();
        }

        internal void remove_csp(int skin_id, String csp_type)
        {
            String[] csps = get_skin_csps(skin_id);
            int count = 0;
            String newcsps = "";
            foreach(String s in csps)
            {
                if(s != csp_type)
                {
                    if (count == 0)
                    {
                        newcsps += s;
                    }else
                    {
                        newcsps += ";"+s;
                    }
                }
                count++;
                
            }


            String sql = "update skins set csps = @csps where id = @skin_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("csps", newcsps);
            command.Parameters.AddWithValue("skin_id", skin_id);
            command.ExecuteNonQuery();
        }

        internal void remove_model(int skin_id,String model_name)
        {
            String[] models = get_skin_models(skin_id);
            int count = 0;
            String newmodels = "";
            foreach (String s in models)
            {
                if (s != model_name)
                {
                    if (count == 0)
                    {
                        newmodels += s;
                    }
                    else
                    {
                        newmodels += ";" + s;
                    }
                }
                count++;
            }


            String sql = "update skins set models = @models where id = @skin_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("models", newmodels);
            command.Parameters.AddWithValue("skin_id", skin_id);
            command.ExecuteNonQuery();
        }

        internal void clear_workspace(int workspace_id)
        {
            String sql = "delete from skin_library where workspace_id = @workspace_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            command.ExecuteNonQuery();
        }

        internal void delete_workspace(int workspace_id)
        {
            String sql = "delete from skin_library where workspace_id = @workspace_id";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            command.ExecuteNonQuery();

            sql = "delete from workspaces where id = @workspace_id";
            command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            command.ExecuteNonQuery();
        }
        #endregion

        #region Reorder
        public void reorder_workspace()
        {
            string sql = "select slot from workspaces";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            int countslot = 1;
            while (reader.Read())
            {
                int slot = reader.GetInt32(0);
                if(countslot != slot)
                {
                    String sql2 = "update workspaces set slot = @countslot where slot = @slot";
                    command = new SQLiteCommand(sql2, db_connection);
                    command.Parameters.AddWithValue("countslot", countslot);
                    command.Parameters.AddWithValue("slot", slot);
                    command.ExecuteNonQuery();
                }
                countslot++;
            }
        }

        public void reorder_skins(int character_id, int workspace_id)
        {
            string sql = "select slot from skin_library where character_id = @character_id and workspace_id = @workspace_id";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.Parameters.AddWithValue("character_id", character_id);
            command.Parameters.AddWithValue("workspace_id", workspace_id);
            SQLiteDataReader reader = command.ExecuteReader();
            int countslot = 1;
            while (reader.Read())
            {
                int slot = reader.GetInt32(0);
                if (countslot != slot)
                {
                    String sql2 = "update skin_library set slot = @countslot where slot = @slot and character_id = @character_id and workspace_id = @workspace_id";
                    command = new SQLiteCommand(sql2, db_connection);
                    command.Parameters.AddWithValue("countslot", countslot);
                    command.Parameters.AddWithValue("slot", slot);
                    command.Parameters.AddWithValue("character_id", character_id);
                    command.Parameters.AddWithValue("workspace_id", workspace_id);
                    command.ExecuteNonQuery();
                }
                countslot++;
            }
        }
        #endregion
    }
}
