using System;
#if TextMeshPro
using TMPro;
#endif
using UnityEngine;

namespace Guru
{
    [Serializable]
    [CreateAssetMenu(fileName = "FixFontAsset.asset", menuName = "Guru/Create FixFontAsset ...", order = 1)]
    public class FixFontAsset: ScriptableObject
    {
        public FixFontInfo[] items;
        
        public FixFontInfo Get(string code)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].code == code)
                {
                    return items[i];
                }
            }

            return null;
        }

        public static FixFontAsset Load()
        {
            var inst = Resources.Load<FixFontAsset>(nameof(FixFontAsset));
            return inst;
        }
    } 

    [Serializable]
    public class FixFontInfo
    {
        public string code;
        public Font font;
#if TextMeshPro
        public TMP_FontAsset fontAsset;
#endif
    }
}