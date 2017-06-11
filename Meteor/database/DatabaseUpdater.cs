using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace Meteor.database
{
    class DatabaseUpdater
    {
        private static readonly string ApplicationPath = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
        private static string CommandFilePath;
        private static readonly string BackupDirectoryPath = ApplicationPath + "/Backups/Library/";
        private static readonly string LibraryPath = ApplicationPath+ "/Library.sqlite";
        private string BackupPath;
        private static int BackupId;
        private db_handler db;

        public DatabaseUpdater()
        {
            BackupId = FindAvailableID();
            BackupPath = BackupDirectoryPath + "Backup_Library_" + BackupId+".sqlite";
            db = new db_handler();
            findlastcommand();
        }

        public int FindAvailableID()
        {
            int i = 1;
            Boolean found = false;

            while (!found)
            {
                if (File.Exists(BackupDirectoryPath + "Backup_Library_" + i+".sqlite"))
                {
                    i++;
                }
                else
                {
                    found = true;
                }
            }
            
            return i;
        }

        public Boolean UpdootDatabase()
        {
            if (CommandFilePath != "")
            {
                if (File.Exists(CommandFilePath))
                {
                    try
                    {
                        if (!Directory.Exists(BackupDirectoryPath))
                        {
                            Directory.CreateDirectory(BackupDirectoryPath);
                        }

                        File.Copy(LibraryPath, BackupPath);

                        if (File.Exists(LibraryPath))
                        {
                            string[] lines = System.IO.File.ReadAllLines(CommandFilePath);
                            Boolean UpdateHasErrors = false;
                            foreach (string line in lines)
                            {
                                if (!db.UpdateDatabase(line))
                                {
                                    UpdateHasErrors = true;
                                }
                            }
                            if (UpdateHasErrors)
                            {

                                File.Copy(BackupPath, LibraryPath);
                                return false;
                            }
                            else
                            {
                                File.Delete(CommandFilePath);
                                return true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        String exception = e.Message;
                        String stack = e.StackTrace;
                        String test = "";
                        return false;
                    }
                }
            }
            


            return false;

        }

        public void findlastcommand()
        {
            if(File.Exists(ApplicationPath + "/command.txt"))
            {
                CommandFilePath = ApplicationPath + "/command.txt";
            }
            else
            {
                Boolean found = false;
                String TemporaryPath = ApplicationPath + "/commands/command_";
                int i = 1;
                while (!found && i < 100)
                {
                    if (File.Exists(TemporaryPath + i + ".txt"))
                    {
                        found = true;
                        CommandFilePath = TemporaryPath + i + ".txt";
                    }
                    i++;
                }
            }

            


        }

    }
}
