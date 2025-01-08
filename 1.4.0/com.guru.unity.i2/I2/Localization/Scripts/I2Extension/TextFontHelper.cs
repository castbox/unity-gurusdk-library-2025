#if TextMeshPro
using TMPro;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Guru
{
    /// <summary>
    /// 字体修复助手
    /// 后期可扩展, 当用户切换字体时进行修复
    /// </summary>
    [DisallowMultipleComponent]
    public class TextFontHelper : MonoBehaviour
    {
        public Text text;
        [HideInInspector]
        public Font originFont;
#if TextMeshPro
        public TMP_Text tmpText;
        [HideInInspector]
        public TMP_FontAsset originFontAsset;
#endif

        public const int TEXT_TYPE_NONE = 0;
        public const int TEXT_TYPE_UGUI = 1;
        public const int TEXT_TYPE_TMP = 2;

        private bool _fixSuccess; // 修复成功标志位

        #region 生命周期

        void Awake()
        {
            if (text == null)
            {
                text = GetComponent<Text>();
            }
            if (text != null)
            {
                originFont = text.font;
            }
#if TextMeshPro
            if (tmpText == null)
            {
                tmpText = GetComponent<TMP_Text>();
            }
            if(tmpText != null)
            {
                originFontAsset = tmpText.font;
            }
#endif

            // 字体修复
            FixFont(); 

            // 印地语修复
            if (text != null)
            {
                text.RegisterDirtyLayoutCallback(text.FixHindiText);
            }
#if TextMeshPro
            if (tmpText != null)
            {
                tmpText.RegisterDirtyLayoutCallback(tmpText.FixHindiTmp);
            }
#endif
        }


        private void OnEnable()
        {
            FixFont(); // 激活后复查
        }

        private void OnDestroy()
        {
            if (text != null)
            {
                text.UnregisterDirtyLayoutCallback(text.FixHindiText);
            }
            
#if TextMeshPro
            if (tmpText != null)
            {
                tmpText.UnregisterDirtyLayoutCallback(tmpText.FixHindiTmp);
            }
#endif
        }

        #endregion

        #region 字体适配接口

        /// <summary>
        /// 文本组件类型
        /// </summary>
        public int TextType
        {
            get
            {
                int type = TEXT_TYPE_NONE;
                if (text != null)
                {
                    type = TEXT_TYPE_UGUI;
                }
#if TextMeshPro
                else if (tmpText != null)
                {
                    type = TEXT_TYPE_TMP;
                }
#endif

                return type;
            }
        }


        /// <summary>
        /// 适配UGUI字体文件
        /// </summary>
        /// <param name="font"></param>
        public bool ApplyFont(Font font)
        {
            if (font == null)
            {
                return false;
            }
            
            if (text != null)
            {
                text.font = font;
                return true;
            }

            return false;
        }

#if TextMeshPro
        /// <summary>
        /// 适配 TextMeshPro 字体文件
        /// </summary>
        /// <param name="fontAsset"></param>
        public bool ApplyFontAsset(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null)
            {
                return false;
            }
            
            if (tmpText != null)
            {
                tmpText.font = fontAsset;
                return true;
            }

            return false;
        }
#endif

        #endregion

        #region 修复字体

        /// <summary>
        /// 修复字体
        /// </summary>
        public void FixFont(bool force = false)
        {
            if (!force && _fixSuccess)
            {
                return;
            }
            _fixSuccess = Guru.I2Supporter.Fix(this, force);
        }

        #endregion

        #region 安装组件

#if UNITY_EDITOR
        /// <summary>
        /// 安装UGUI字体
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static TextFontHelper Setup(Text text)
        {
            TextFontHelper cop = text.gameObject.GetComponent<TextFontHelper>();
            if (cop != null)
            {
                return null;
            }

            cop = text.gameObject.AddComponent<TextFontHelper>();
            cop.text = text;
            return cop;
        }

#if TextMeshPro
        /// <summary>
        /// 安装TMP字体
        /// </summary>
        /// <param name="tmp"></param>
        /// <returns></returns>
        public static TextFontHelper Setup(TMP_Text tmp)
        {
            TextFontHelper cop = tmp.gameObject.GetComponent<TextFontHelper>();
            if (cop != null) return null;

            cop = tmp.gameObject.AddComponent<TextFontHelper>();
            cop.tmpText = tmp;
            return cop;
        }
#endif

#endif
        #endregion
    }
}
