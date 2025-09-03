using System.IO;
using UnityEngine;

namespace Guru.Editor
{
    public class Guru16KbHelper
    {
        /// <summary>
        /// 为 App 生成 16kb 配置
        /// </summary>
        private static void InjectExtraArgsFor16KbPageSize()
        {
            var fileName = "IL2CPP_extra_args.txt";
            var filePath = Path.GetFullPath($"{Application.dataPath}/Plugins/Android/{fileName}");
            var fileConent = "--page-size=16k\n--max-page-size=16384";
            
            if (File.Exists(filePath))
            {
                return;
            }
            
            File.WriteAllText(filePath, fileConent);
        }


        public static void Apply()
        {
#if GURU_16KB
          InjectExtraArgsFor16KbPageSize();
#endif
        }

    }
}