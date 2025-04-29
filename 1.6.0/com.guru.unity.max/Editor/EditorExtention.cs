using System.IO;

namespace Guru.Editor
{
    public static class GuruEditorExtentions
    {
        
        
        
        public static bool FileExists(this IFileIO _, string filePath) => File.Exists(filePath);

        public static bool DirectoryExists(this IFileIO _, string directoryPath) => Directory.Exists(directoryPath);

        public static void EnsureDir(this IFileIO _, string dirPath)
        {
            var dir = new DirectoryInfo(dirPath);
            if(!dir.Exists) dir.Create();
        }

        public static void EnsureRootDir(this IFileIO _, string filePath) =>
            EnsureDir(_, Directory.GetParent(filePath)?.FullName);
        
        public static bool DeleteFile(this IFileIO _, string filePath)
        {
            if (FileExists(_, filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        public static bool MoveFile(this IFileIO _,string from, string to)
        {
            if (FileExists(_, from))
            {
                EnsureRootDir(_, to);
                DeleteFile(_, to);
                File.Move(from, to);
                return true;
            }
            return false;
        }

        public static bool CopyFile(this IFileIO _,string from, string to)
        {
            if (FileExists(_, from))
            {
                EnsureRootDir(_, to);
                File.Copy(from, to);
                return true;
            }
            return false;
        }
        
        
        
    }
}