using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Guru;

public class FileUtil
{
    /// <summary>
    /// 递归删除目录
    /// </summary>
    /// <param name="str">String.</param>
    public static void DeleteDir(string str)
    {
        DirectoryInfo dir = new DirectoryInfo (str);
        if (!dir.Exists) 
            return;

        foreach (var file in dir.GetFiles()) 
        {
            if (file.FullName.EndsWith(".meta", StringComparison.Ordinal)) 
                continue;
            file.Delete ();
        }

        foreach (var d in dir.GetDirectories()) 
            DeleteDir (d.FullName);

        dir.Delete (true);
    }
    
    /// <summary>
    /// 递归创建目录
    /// </summary>
    /// <param name="str">String.</param>
    public static void CreateDir(string str, bool clean = false) 
    {
        if (clean)
            DeleteDir(str);

        DirectoryInfo dir = new DirectoryInfo (str);
        if (!dir.Parent.Exists) 
            CreateDir (dir.Parent.FullName);
        
        if(!dir.Exists)
            dir.Create ();
    }
    
    /// <summary>
    /// 拷贝目录
    /// </summary>
    /// <param name="from">From.</param>
    /// <param name="to">To.</param>
    public static void CopyDir(string from, string to) 
    {
        var dir = new DirectoryInfo (from);
        if (!dir.Exists) 
            return;
        
        CreateDir (to);
        foreach (var f in dir.GetFiles()) 
        {
            var name = f.Name;
            if (name.StartsWith (".", StringComparison.Ordinal) || name.StartsWith ("~", StringComparison.Ordinal)) 
                continue;

            var to_file = Path.Combine (to, name);
            if (File.Exists (to_file)) 
                File.Delete (to_file);
            File.Copy (f.FullName, to_file);
        }

        foreach (var d in dir.GetDirectories()) 
        {
            string sub = Path.Combine (to, d.Name);
            CreateDir (sub);
            CopyDir(d.FullName, sub);
        }
    }
    
    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <returns>The file.</returns>
    /// <param name="file">File.</param>
    public static byte[] ReadFile(string file) 
    {
        if (!File.Exists(file)) 
            return null;

        return File.ReadAllBytes(file);
    }

    public static async Task<byte[]> ReadFileAsync(string file)
    {
        if (!File.Exists(file)) 
            return null;
        FileStream fileStream = new FileStream(file,FileMode.Open);
        byte[] bytes = new byte[fileStream.Length]; 
        await fileStream.ReadAsync(bytes,0,(int)fileStream.Length);
        fileStream.Dispose();
        return bytes;
    }

    /// <summary>
    /// 写文件, 如果目录不存在自动创建
    /// </summary>
    /// <param name="file">File.</param>
    /// <param name="data">Data.</param>
    public static void WriteFile(string file, byte[] data) 
    {
        try 
        {
            var parent = Directory.GetParent(file);
            CreateDir(parent.FullName);
            File.WriteAllBytes(file, data);
        } 
        catch(Exception e) 
        {
            Log.E("FileUtil", e);
        }
    }
    
    /// <summary>
    /// 写文件, 如果目录不存在自动创建
    /// </summary>
    /// <param name="file">File.</param>
    /// <param name="data">Data.</param>
    public static void WriteFile(string file, string data) 
    {
        try 
        {
            var parent = Directory.GetParent(file);
            CreateDir(parent.FullName);
            File.WriteAllText(file, data);
        } 
        catch(Exception e) 
        {
            Log.E("FileUtil", e);
        }
    }

    /// <summary>
    /// 写文件, 如果目录不存在自动创建
    /// data 必须是utf-8的格式
    /// </summary>
    /// <param name="file">文件路径</param>
    /// <param name="data">数据</param>
    public static async Task WriteFileAsync(string file,string data)
    {
        try 
        {
            var parent = Directory.GetParent(file);
            CreateDir(parent.FullName);
            FileStream fileStream = new FileStream(file ,FileMode.OpenOrCreate);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            await fileStream.WriteAsync(bytes, 0, bytes.Length);
            fileStream.Dispose();
        } 
        catch(Exception e) 
        {
            Log.E("FileUtil", e);
        }
    }

    public static async Task WriteFileAsync(string file,byte[] data)
    {
        try 
        {
            var parent = Directory.GetParent(file);
            CreateDir(parent.FullName);
            FileStream fileStream = new FileStream(file ,FileMode.OpenOrCreate);
            await fileStream.WriteAsync(data, 0, data.Length);
            fileStream.Dispose();
        } 
        catch(Exception e) 
        {
            Log.E("FileUtil", e);
        }
    }

    /// <summary>
    /// 判断文件是否存在
    /// </summary>
    /// <param name="file">File.</param>
    public static bool Exists(string file)
    {
        return File.Exists(file);
    }
}
