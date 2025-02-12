using I2.Loc;
using UnityEngine.UI;

namespace Guru
{
    public static class TextExtension
    {
        public static void FixHindiText(this Text text)
        {
            if (text == null)
            {
                return;
            }

            string val = text.text;
            if (LocalizationManager.CurrentLanguageCode == "hi" && !string.IsNullOrEmpty(val))
            {
                val = I2Supporter.HindiRichTextFixer.BeforeFix(val);
                val = HindiCorrector.GetCorrectedHindiText(val);
                val = I2Supporter.HindiRichTextFixer.Fix(val);
                text.text = val;
            }
        }
        
#if TextMeshPro
        public static void FixHindiTmp(this TMPro.TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            string val = text.text;
            if (LocalizationManager.CurrentLanguageCode == "hi" && !string.IsNullOrEmpty(val))
            {
                val = I2Supporter.HindiRichTextFixer.BeforeFix(val);
                val = HindiCorrector.GetCorrectedHindiText(val);
                val = I2Supporter.HindiRichTextFixer.Fix(val);
                text.text = val;
            }
        }
#endif
    }
}