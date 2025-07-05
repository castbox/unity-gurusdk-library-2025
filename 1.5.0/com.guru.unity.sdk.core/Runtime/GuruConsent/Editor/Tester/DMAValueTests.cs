using UnityEngine;

namespace Guru.Editor
{
    using NUnit.Framework;
    using Guru;
    using System.Text.RegularExpressions;
    
    public class DMAValueTests
    {

        
        [Test]
        public static void TestDMAValue()
        {
            
            GoogleDMAHelper.UpdateDmaStatus("0");
            Debug.Log($"\n-----------\n\n");
            
            GoogleDMAHelper.UpdateDmaStatus("1011");
            Debug.Log($"\n-----------\n\n");

            GoogleDMAHelper.UpdateDmaStatus("10000010");
            Debug.Log($"\n-----------\n\n");

            GoogleDMAHelper.UpdateDmaStatus("10110010000");
            Debug.Log($"\n-----------\n\n");

        }
        
        
        
        [Test]
        public static void Test01Value()
        {
            string str = "000111";
            
            var match = Regex.Match(str, "^[01]+$");
            Debug.Log($"#1 match: {match.Value}");
            
            str = "901dasd(>P{";
            match = Regex.Match(str, "^[01]+$");
            Debug.Log($"#2 match: {match.Value}");

        }

    }
}