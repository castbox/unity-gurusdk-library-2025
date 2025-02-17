

using UnityEngine;

namespace Guru.Test
{
    using I2.Loc;
    using NUnit.Framework;
    public class GuruI2LocAPITest
    {

        [Test]
        public static void TestReverseText()
        {
            var str = "=> 123 {0} 456 <-";
            Debug.Log($"raw:{str}");
            
            var result = I2Utils.ReverseText(str);
            Debug.Log($"res1:{result}");
            
            var res2 = LocalizationManager.ApplyRTLfix(str);
            Debug.Log($"res2:{res2}");
        }

    }
}