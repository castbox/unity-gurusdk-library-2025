using System.IO;
using UnityEngine;

namespace Guru.Editor.Max
{
    public class PubmaticMod: GuruModifier
    {
        protected override string TargetPath => "OpenWrapSDK/Editor/POBPlistProcessor.cs";


        public static void Apply()
        {
            PubmaticMod mod = new PubmaticMod();
            mod.FixPostBuildPath();
        }

        private void FixPostBuildPath()
        {
            var path = GetFullPath();

            if (File.Exists(path))
            {
                string raw = File.ReadAllText(path);
                string realDataPath = $"Packages/{GuruMaxSdkAPI.PackageName}";
                raw = raw.Replace("Application.dataPath", $"\"{realDataPath}\"");
                File.WriteAllText(path, raw);
                Debug.Log($"{Tag} <color=#88ff00>--- code has injected: {path}.</color>");
            }
            else
            {
                Debug.Log($"{Tag} <color=red>--- File not found: {path}.</color>");
            }

        }



    }
}