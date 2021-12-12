using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;

namespace HoodieSuite.Properties
{
    public static class ResourcesRW
    {
        public static string resFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Properties", "Resources.resx");
        public static void AddOrUpdateResource(string key, string value)
        {
            var resx = new List<DictionaryEntry>();
            if (File.Exists(resFilePath))
            {
                using (var reader = new ResXResourceReader(resFilePath))
                {
                    resx = reader.Cast<DictionaryEntry>().ToList();
                    var existingResource = resx.Where(r => r.Key.ToString() == key).FirstOrDefault();
                    if (existingResource.Key == null && existingResource.Value == null) // NEW!
                    {
                        resx.Add(new DictionaryEntry() { Key = key, Value = value });
                    }
                    else // MODIFIED RESOURCE!
                    {
                        var modifiedResx = new DictionaryEntry() { Key = existingResource.Key, Value = value };
                        resx.Remove(existingResource);  // REMOVING RESOURCE!
                        resx.Add(modifiedResx);  // AND THEN ADDING RESOURCE!
                    }
                }
            }
            else 
            {
                resx.Add(new DictionaryEntry() { Key = key, Value = value });
            }
            using (var writer = new ResXResourceWriter(resFilePath))
            {
                resx.ForEach(r =>
                {
                    // Again Adding all resource to generate with final items
                    writer.AddResource(r.Key.ToString(), r.Value.ToString());
                });
                writer.Generate();
            }
        }

        private static void CreateResFileIfNotPresent()
        {

            if (!File.Exists(resFilePath))
            {
                using (var writer = new ResXResourceWriter(resFilePath))
                {
                    writer.AddResource("root", "");
                    writer.Generate();
                }
            }
        }

        public static string ReadKeyFromResourceFile(string key, string defaultVal = "")
        {
            if (File.Exists(resFilePath))
            {
                using (var reader = new ResXResourceReader(resFilePath))
                {
                    var resx = reader.Cast<DictionaryEntry>().ToList();
                    var a = resx.Where(r => r.Key.ToString() == key);
                    if (a.Any())
                    {
                        var existingResource = a.First();
                        return existingResource.Value.ToString();
                    }
                    return defaultVal;
                }
            }
            return defaultVal;
        }
    }
}
