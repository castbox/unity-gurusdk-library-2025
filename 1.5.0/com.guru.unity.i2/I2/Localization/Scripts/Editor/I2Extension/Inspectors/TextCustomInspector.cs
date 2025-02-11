#if ENABLE_I2_AUTO_FIX
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Guru
{
    [CustomEditor(typeof(Text))]
    public class TextCustomInspector: UnityEditor.UI.TextEditor
    {
        private bool componentAdded;
        private Text textTarget;

        protected override void OnEnable()
        {
            base.OnEnable();
            textTarget = (Text)target;
            componentAdded = textTarget.GetComponent<TextFontHelper>() != null;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!componentAdded && Application.isPlaying == false)
            {
                // 添加自定义组件
                TextFontHelper textFontHelper = textTarget.gameObject.AddComponent<TextFontHelper>();
                if (textFontHelper != null)
                {
                    textFontHelper.text = textTarget;
                    EditorUtility.SetDirty(target);
                }
                componentAdded = true;
            }
        }
    }
}
#endif