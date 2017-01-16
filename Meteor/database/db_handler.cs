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
                case 2:
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
            String sql = "Select id from characters where name = '" + char_name + "'";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
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

            String sql = "select skins.name from skin_library Join skins on(skins.id = skin_library.skin_id) Join characters on(characters.id = skin_library.character_id) Where characters.name = '" + char_name + "' and skin_library.workspace_id = "+Int32.Parse(workspace_id)+" Order by skin_library.Slot";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                skins.Add(reader.GetString(0));
            }

            return skins;
        }

        //Get's a skin id based on char id and slot
        public int get_skin_id(int char_id, int slot)
        {
            String sql = "Select skin_id from skin_library where character_id = " + char_id + " and slot = " + slot;
            int id = 0;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
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
            String sql = "Select gb_id from skins where id = " + skin_id;
            int id = 0;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
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
            String sql = "Select gb_uid from skins where id = " + skin_id;
            int id = 0;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
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

            String sql = "select name, author, models, csps from skins where id = "+skin_id;
            String name = "";
            String author = "";
            String models = "";
            String csps = "";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
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

        public Boolean skin_locked(int slot, int char_id)
        {
            Boolean test = false;

            String sql = "Select locked from skin_library where slot = " + slot+" and character_id = "+char_id;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                test = reader.GetInt32(0) == 1? true: false;
            }

            return test;
        }

        public DataSet get_custom_skins()
        {
            DataSet ds = new DataSet();

            String sql = "Select name, author,models,csps,gb_uid from skins where locked = 0";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();

            SQLiteDataAdapter sqlda = new SQLiteDataAdapter(sql,db_connection);
            sqlda.Fill(ds);

            return ds;
        }
        #endregion

        #region Workspaces
        public ArrayList get_workspaces()
        {
            ArrayList workspaces = new ArrayList();
            string sql = "select name from workspaces";

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
            string sql = "select locked from workspaces where slot = "+slot;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
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
            string sql = "select count(*) from skin_library join workspaces on (workspaces.id = skin_library.workspace_id) where skin_library.locked = 0 and  workspaces.slot = " + slot;

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
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
            string sql = "select id from workspaces where slot = "+slot;

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }
            return id;
        }
        
        #endregion

        #region Config
        //Gets a property value
        public String get_property(String property)
        {
            String value = "";
            String sql = "select value from configuration where property = '" + property + "'";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
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
            String sql = "update skins set name ='"+name+"' where id = "+id;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();
        }

        public void set_skin_author(String author, int id)
        {
            String sql = "update skins set author ='" + author + "' where id = " + id;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();
        }

        public void set_property_value(String value,String property)
        {
            String sql = "update configuration set value ='" + value + "' where property = '" + property+"'";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();
        }

        

        public void set_workspace_name(String name, int slot)
        {
            String sql = "update workspaces set name ='" + name + "' where slot = " + slot;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();
        }
        #endregion

        #region Insert
        public void add_skin(String name,String author,String models, String csps,int character_id,int slot)
        {
            String sql = "insert into skins (name,author,models,csps,character_id,locked,gb_uid) values('"+name+ "','" + author + "','" + models + "','" + csps + "'," + character_id + ",0,0)";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();

            long id =  db_connection.LastInsertRowId;

            sql = "insert into skin_library (workspace_id,character_id,skin_id,slot,locked) values ("+get_property("workspace")+","+character_id+","+id+","+slot+",0)";
            command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();
        }

        public long add_workspace(String name,int slot)
        {
            String sql = "insert into workspaces (name,slot,locked) values ('"+name+"',"+slot+",0)";
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();

            return db_connection.LastInsertRowId;
        }

        public void add_default_skins(long id)
        {

            ArrayList lines = new ArrayList();
            String sql = "select character_id,skin_id,slot from skin_library Where skin_library.workspace_id = 1";

            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int[] line = new int[] { reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)};
                lines.Add(line);
            }

            foreach (int[] line in lines)
            {
                sql = "insert into skin_library (character_id,skin_id,slot,locked,workspace_id) values ("+line[0]+","+line[1]+","+line[2]+",1,"+id+")";
                command = new SQLiteCommand(sql, db_connection);
                command.ExecuteNonQuery();
            }
        }
        #endregion

        #region Remove
        internal void remove_skin(int slot,int char_id, string selected_workspace)
        {
            String sql = "delete from skin_library where slot = "+slot+" and character_id = "+char_id+" and workspace_id = "+selected_workspace;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();
        }

        internal void clear_workspace(int workspace_id)
        {
            String sql = "delete from skin_library where workspace_id = " + workspace_id;
            SQLiteCommand command = new SQLiteCommand(sql, db_connection);
            command.ExecuteNonQuery();
        }
        #endregion
    }
}
