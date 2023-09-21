using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Utils
{
    public static class JsonFileManager
    {
        public static T LoadFile<T>(string path, bool createIfNotExist = false)
            where T : new()
        {
            if(!File.Exists(path) && createIfNotExist)
            {
                SaveFile(new T(), path);
            }

            var data = JObject.Parse(File.ReadAllText(path));
            return data.ToObject<T>();
        }

        public static async Task<T> LoadFileAsync<T>(string path, bool createIfNotExist = false)
            where T : new()
        {
            if (!File.Exists(path) && createIfNotExist)
            {
                SaveFile(new T(), path);
            }

            var data = JObject.Parse(await File.ReadAllTextAsync(path));
            return data.ToObject<T>();
        }

        public static void SaveFile<T>(T data, string path)
            where T : new()
        {
            var jObject = JObject.FromObject(data);
            File.WriteAllText(path, jObject.ToString());
        }

        public static async Task SaveFileAsync<T>(T data, string path)
            where T : new()
        {
            var jObject = JObject.FromObject(data);
            await File.WriteAllTextAsync(path, jObject.ToString());
        }
    }
}
