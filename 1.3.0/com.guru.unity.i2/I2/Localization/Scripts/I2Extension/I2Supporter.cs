namespace Guru
{
    public class I2Supporter
    {
        private static FixFontAsset _fixFontAsset;

        public static FixFontAsset FixFontAsset
        {
            get
            {
                if (_fixFontAsset == null)
                {
                    _fixFontAsset = FixFontAsset.Load();
                }

                return _fixFontAsset;
            }
        }
        
        private static FixFontInfo _fixInfo;

        public static FixFontInfo GetFixInfo(bool force)
        {
            if (force || _fixInfo == null)
            {
                _fixInfo = FindFixInfoByI2();
            }

            return _fixInfo;
        }

        private static FixFontInfo FindFixInfoByI2()
        {
            string code = I2.Loc.LocalizationManager.CurrentLanguageCode;
            FixFontInfo info = FixFontAsset.Get(code);
            return info;
        }

        /// <summary>
        /// 修复文本及Font
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public static bool Fix(TextFontHelper helper, bool force)
        {
            FixFontInfo info = GetFixInfo(force);
            if (info == null && !force)
            {
                return false;   // 无需修复
            }
            
            if(info == null)
            {
                if(helper.TextType == TextFontHelper.TEXT_TYPE_UGUI)
                {
                    return helper.ApplyFont(helper.originFont); // UGUI 修复
                }
#if TextMeshPro                
                if(helper.TextType == TextFontHelper.TEXT_TYPE_TMP)
                {
                    return helper.ApplyFontAsset(helper.originFontAsset); // TMP 修复
                }
#endif
            }

            if (helper.TextType == TextFontHelper.TEXT_TYPE_UGUI)
            {
                return helper.ApplyFont(info.font); // UGUI 修复
            }
#if TextMeshPro
            if (helper.TextType == TextFontHelper.TEXT_TYPE_TMP)
            {
                return helper.ApplyFontAsset(info.fontAsset); // TMP 修复
            }
#endif

            return false;
        }
        
        /// <summary>
        /// 运行时切换语言后，修复所有Font
        /// </summary>
        public static void FixAll()
        {
            TextFontHelper[] helpers = UnityEngine.Object.FindObjectsOfType<TextFontHelper>(true);
            foreach (TextFontHelper helper in helpers)
            {
                Fix(helper, true);
            }
        }

        private static IHindiRichTextFixer _hindiRichTextFixer;
        public static IHindiRichTextFixer HindiRichTextFixer
        {
            get
            {
                if (_hindiRichTextFixer == null)
                {
                    _hindiRichTextFixer = new DefaultRichTextFix();
                }

                return _hindiRichTextFixer;
            }

            set
            {
                if (value != null)
                {
                    _hindiRichTextFixer = value;
                }
            }
        }
    }
}