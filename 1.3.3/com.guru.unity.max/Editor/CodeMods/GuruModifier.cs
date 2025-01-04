using System.IO;
using Guru;

namespace Guru.Editor.Max
{
    public abstract class GuruModifier: IFileIO
    {
        protected virtual string TargetPath { get; set; }

        public const string Tag = "[GuruMod]";

        protected string GetFullPath(string path = "")
        {
            if (string.IsNullOrEmpty(path)) path = TargetPath;
            return Path.GetFullPath(GetAssetPath(path));
        }

        
        protected string GetAssetPath(string path = "")
        {
            if (string.IsNullOrEmpty(path)) path = TargetPath;
            return GuruMaxCodeFixer.GetAssetPathFromPackageForExportPath(path);
        }
        


    }
}