﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Windows;
using Vic3ModManager.Essentials;
using System.Text.Json.Serialization;

namespace Vic3ModManager
{
    public static class ModManager
    {
        public static EventHandler? OnModSwitched;

        public static void AddMod(Mod mod)
        {
            AllMods.Add(mod);
        }

        public static void RemoveMod(Mod mod)
        {
            AllMods.Remove(mod);
        }

        public static void SwitchMod(Mod mod)
        {
            if (AllMods.Contains(mod))
            {
                CurrentMod = mod;
                OnModSwitched?.Invoke(null, null);
            }
        }

        public static string[] GetAvailableMods()
        {
            if (!Directory.Exists("./Mods"))
            {
                return [];
            }

            string[] mods = Directory.GetFiles("./Mods", "*.json");

            if (mods.Length < 1) return [];

            for (int i = 0; i < mods.Length; i++)
            {
                mods[i] = Path.GetFileNameWithoutExtension(mods[i]);
            }

            return mods;
        }

        public static void SaveCurrentMod()
        {
            if (CurrentMod == null) return;

            SaveMod(CurrentMod);
        }

        public static void UpdateCurrentMod(string name, string description, string version, GameLanguages.DefaultLanguages defLanguage)
        {
            if (CurrentMod == null) return;

            string oldName = CurrentMod.Name;

            CurrentMod.Name = name;
            CurrentMod.Description = description;
            CurrentMod.Version = version;
            CurrentMod.DefaultLanguage = defLanguage;

            if (oldName != name)
            {
                string oldFileName = StringHelpers.FormatString(oldName);
                string oldFilePath = Path.Combine("./Mods", $"{oldFileName}.json");

                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            SaveCurrentMod();
        }

        private static void SaveMod(Mod mod)
        {
            if (!Directory.Exists("./Mods"))
            {
                Directory.CreateDirectory("./Mods");
            }

            string fileName = StringHelpers.FormatString(mod.Name);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(mod, settings);
            File.WriteAllText($"./Mods/{fileName}.json", json, Encoding.UTF8);
        }

        public static bool LoadMod(string file, bool isExternalFile = false)
        {
            bool success = false;
            try
            {
                string filePath = isExternalFile? file : Path.Combine("./Mods", $"{file}.json");
                string fileContent = File.ReadAllText(filePath);

                Mod? loadedMod = JsonConvert.DeserializeObject<Mod>(fileContent);

                if (loadedMod != null)
                {
                    if (loadedMod.ModStructureIteration != Mod.MOD_STUCTURE_ITERATION)
                    {
                        File.Copy(filePath, filePath.Replace(".json", $"_backup_{DateTime.Now:dd_MM_yy_hh_mm}.json"));

                        loadedMod = MigrateToCurrentVersion(loadedMod);

                        SaveMod(loadedMod);
                    }

                    AddMod(loadedMod);
                    SwitchMod(loadedMod);
                    success = true;
                }
            }
            catch (JsonSerializationException ex)
            {
                MessageBox.Show($"JSON Serialization Exception: {ex.Message}");
            }
            catch (Exception ex) { MessageBox.Show($"Unable to load mod project, file is possibly corrupted! Error: {ex.Message}"); }

            return success;
        }   

        public static Mod? CurrentMod { get; private set; }

        public static List<Mod> AllMods { get; private set; } = new();

        public static Mod MigrateToCurrentVersion(Mod oldMod)
        {
            Mod newMod = new(oldMod.Name, oldMod.Description, oldMod.Version, [.. oldMod.MusicAlbums], oldMod.DefaultLanguage);

            return newMod;
        }

    }

}
