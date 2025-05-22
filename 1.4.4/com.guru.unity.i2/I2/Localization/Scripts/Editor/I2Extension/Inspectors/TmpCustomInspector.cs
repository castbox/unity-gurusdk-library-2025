#if TextMeshPro && ENABLE_I2_AUTO_FIX
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Guru
{
    [CustomEditor(typeof(TextMeshProUGUI))]
    public class TmpCustomInspector: TMPro.EditorUtilities.TMP_EditorPanelUI
    {
        private bool componentAdded;
        private TextMeshProUGUI tmpTarget;
        protected override void OnEnable()
        {
            base.OnEnable();
            tmpTarget = (TextMeshProUGUI)target;
            componentAdded = tmpTarget.GetComponent<TextFontHelper>() != null;
        }

        protected override void DrawExtraSettings()
        {
            base.DrawExtraSettings();
            if (!componentAdded && Application.isPlaying == false)
            {
                // 添加自定义组件
                TextFontHelper textFontHelper = tmpTarget.gameObject.AddComponent<TextFontHelper>();
                if (textFontHelper != null)
                {
                    textFontHelper.tmpText = tmpTarget;
                    EditorUtility.SetDirty(target);
                }

                componentAdded = true;
            }
        }
    }
}
#endif