namespace Guru
{
    using System;
    using UnityEngine;
    using System.Linq;
    
    /// <summary>
    /// IDHelper 生成器
    /// </summary>
    public static class IDHelper
    {
        private const string UUID_PREFIX = "1b6eb1"; // 6 bytes
        private const string UUID_NAMESCPACE = "6ba7b810-9dad-11d1-80b4-00c04fd430c8";
        
        /// <summary>
        /// 通过给定字符串生成固定的 UUID
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GenUUID(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.Log($"<color=red>GenUUID: And Invalid Key: {key}</color>");
                return "";
            }

            var uid_code = key;
            if(key.Contains('-'))  uid_code = key.Split('-').Last(); 
            if (uid_code.Length > 13)
            {
                return MakeUUIDV5(key);
            }
            return MakeGuruKeyUUID(uid_code);
        }

        /// <summary>
        /// 需求详见：【[中台] 【BI】在订单数据内设置user_id (QA 无需测试)(BI 跟进)】
        /// https://www.tapd.cn/33527076/prong/stories/view/1133527076001020001
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string MakeUUIDV5(string key)
        {
            // 使用DNS命名空间的GUID
            Guid namespaceId = Guid.Parse(UUID_NAMESCPACE);
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(key);
 
            // 计算MD5散列
            byte[] hashBytes = System.Security.Cryptography.MD5.Create().ComputeHash(nameBytes);
 
            // 将命名空间GUID的二进制值与散列值的前16个字节连接起来
            byte[] guidBytes = new byte[16];
            Array.Copy(namespaceId.ToByteArray(), guidBytes, 16);
            Array.Copy(hashBytes, 0, guidBytes, 0, 16);
 
            // 设置版本号和变体以生成V5 UUID
            guidBytes[6] &= 0x0F; // 清除版本位
            guidBytes[6] |= 0x50; // 设置版本和变体位
 
            return new Guid(guidBytes).ToString("D");
        }

        private static string MakeGuruKeyUUID(string uidCode, string prefix = "")
        {
            // 使用MD5创建一个散列值
            if(string.IsNullOrEmpty(prefix)) prefix = UUID_PREFIX; // 6 bytes
            string haxString = ""; // should be less than 26 bytes
            
            for(int i =0; i < uidCode.Length; i++){
                int charCode = (int)(uidCode[i]);
                // Debug.Log($"charCode: {charCode}");
                if (charCode > 0xFF)
                {
                    return "";
                }
                haxString += $"{charCode:X2}";
            }
            var padNum = 32 - prefix.Length;
            if (haxString.Length > padNum)
            {
                return "";
            }
            // Debug.Log($"haxString: {haxString}  len: {haxString.Length}");
            string final =  prefix + haxString.PadLeft(padNum, '0');
            return new Guid(final).ToString("D");
        }

    }
}