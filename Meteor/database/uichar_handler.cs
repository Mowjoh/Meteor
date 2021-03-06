﻿using System;
using System.IO;
using System.Reflection;

namespace Meteor.database
{
    public class uichar_handler
    {
        //returns a String arraylist of ui_char_db values for that character
        public string fileRead(long characterposition, int position)
        {
            Console.WriteLine(characterposition);
            var val = "";

            Stream stream = File.Open(filepath, FileMode.Open);
            using (var br = new BinaryReader(stream))
            {
                var pose = 13 + 127 * characterposition + offsets[position];
                stream.Seek(pose, SeekOrigin.Begin);
                int bit = br.ReadByte();
                switch (bit)
                {
                    case 2:
                        val = br.ReadByte().ToString();
                        break;
                    case 5:
                        var bytes = br.ReadBytes(4);
                        Array.Reverse(bytes);
                        val = BitConverter.ToUInt32(bytes, 0).ToString();
                        break;
                    case 6:
                        var bytes2 = br.ReadBytes(4);
                        Array.Reverse(bytes2);
                        val = BitConverter.ToUInt32(bytes2, 0).ToString();
                        break;
                }
            }

            return val;
        }

        //Sets a value in the file
        public void setFile(int characterposition, int slot, int value)
        {
            Stream stream = File.Open(filepath, FileMode.Open);
            using (var br = new BinaryReader(stream))
            {
                try
                {
                    long pose = 13 + 127 * characterposition + offsets[slot];
                    Console.WriteLine(characterposition);
                    stream.Seek(pose, SeekOrigin.Begin);
                    int bit = br.ReadByte();
                    switch (bit)
                    {
                        case 2:
                            stream.WriteByte(BitConverter.GetBytes(value)[0]);
                            break;

                        case 5:
                            var bytes = BitConverter.GetBytes(value);
                            Array.Reverse(bytes);
                            stream.Write(bytes, 0, 4);
                            break;
                        case 6:
                            var bytes2 = BitConverter.GetBytes(value);
                            Array.Reverse(bytes2);
                            stream.Write(bytes2, 0, 4);
                            break;
                    }
                }
                catch
                {
                }
            }
        }

        #region Class Variables

        public string app_path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
        public string filepath = "";

        private readonly int[] offsets =
        {
            0, 5, 7, 9, 11, 16, 21, 26, 31, 36, 38, 40, 42, 44, 46, 48, 50, 55, 57, 59, 61, 63, 65, 67, 69, 71, 73, 75,
            77, 79, 81, 83, 85, 87, 89, 91, 93, 95, 97, 99, 101, 103, 105, 107, 109, 111, 113, 115, 117, 119, 121, 123,
            125
        };

        public uichar_handler()
        {
            filepath = app_path + "/filebank/configuration/uichar/ui_character_db.bin";

            #endregion
        }
    }
}