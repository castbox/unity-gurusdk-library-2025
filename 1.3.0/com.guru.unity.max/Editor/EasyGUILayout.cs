using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Guru
{
    public static class EasyGUILayout
    {
        private const string K_BOX_STYPE = "Box";
        public static GUIStyle BoxStyle => new GUIStyle(K_BOX_STYPE);

        private static ulong _genId = ulong.MinValue;
        private static ulong GenId
        {
            get
            {
                if (_genId == ulong.MaxValue) _genId = ulong.MinValue;
                _genId++;
                return _genId;
            }
        }

        public static void Label(string value, float width = 0, int fontSize = 0, FontStyle fontStyle = FontStyle.Normal,  TextAnchor anchor = TextAnchor.MiddleLeft, params GUILayoutOption[] options)
        {
            GUILayoutOption w = null;
            if (width > 0)
            {
                w = GUILayout.Width(width);
            }
            
            GUIStyle s = null;
            if (fontSize > 0)
            {
                s = new GUIStyle("Label");
                s.fontSize = fontSize;
                s.alignment = anchor;
                s.fontStyle = fontStyle;
            }

            List<GUILayoutOption> opts = new List<GUILayoutOption>();
            if (w != null) opts.Add(w);
            if (options != null && options.Length > 0) opts.AddRange(options);
            
            if (s == null)
            {
                GUILayout.Label(value, opts.ToArray());
                return;
            }

            GUILayout.Label(value, s, opts.ToArray());
        }

        /// <summary>
        /// 水平Box布局
        /// </summary>
        /// <param name="content"></param>
        public static void HBox(Action content)
        {
            GUILayout.BeginHorizontal(BoxStyle);
            content?.Invoke();
            GUILayout.EndHorizontal(); 
        }
        
        /// <summary>
        /// 水平Box布局
        /// </summary>
        /// <param name="content"></param>
        public static void VBox(Action content)
        {
            GUILayout.BeginVertical(BoxStyle);
            content?.Invoke();
            GUILayout.EndVertical(); 
        }


        /// <summary>
        /// BoxLineItem 风格图框
        /// </summary>
        /// <param name="label"></param>
        /// <param name="contents"></param>
        /// <param name="width"></param>
        /// <param name="fontSize"></param>
        public static void BoxLineItem(string label, float width = 60, int fontSize = 0, params Action[] contents)
        {
            HBox(
                () =>
                {
                    Label(label, width, fontSize);
                    if (contents != null && contents.Length > 0)
                    {
                        for (int i = 0; i < contents.Length; i++)
                        {
                            contents[i]?.Invoke();
                        }
                    }
                }
            );
        }

        /// <summary>
        /// Text 文本框
        /// </summary>
        /// <param name="value"></param>
        /// <param name="onValueChanged"></param>
        public static string Text(string value, Action<string> onValueChanged = null, params GUILayoutOption[] options)
        {
            return Text(value, "", null, onValueChanged, options);;
        }
        
        /// <summary>
        /// Text 文本框
        /// </summary>
        /// <param name="value"></param>
        /// <param name="onValueChanged"></param>
        public static string Text(string value, string label, Action<string> onValueChanged = null, params GUILayoutOption[] options)
        {
            return Text(value, label, null, onValueChanged, options);
        }
        
        /// <summary>
        /// Text 文本框
        /// </summary>
        /// <param name="value"></param>
        /// <param name="onValueChanged"></param>
        public static string Text(string value, GUIStyle style, Action<string> onValueChanged = null, params GUILayoutOption[] options)
        {
            return Text(value, "", style, onValueChanged, options);
        }
        
        
        /// <summary>
        /// Text 文本框
        /// </summary>
        /// <param name="value"></param>
        /// <param name="onValueChanged"></param>
        public static string Text(string text , string label, GUIStyle style, Action<string> onValueChanged = null, params GUILayoutOption[] options)
        {
            string _cname = label;
            if (string.IsNullOrEmpty(_cname)) _cname = $"lab_{GenId}";
            GUI.SetNextControlName(_cname);

            string newValue = "";
            if (string.IsNullOrEmpty(label) && style == null)
            {
                newValue = EditorGUILayout.TextField(text, options);
            }
            else if (string.IsNullOrEmpty(label))
            {
                newValue = EditorGUILayout.TextField(text, style, options);
            }
            else if (style == null)
            {
                newValue = EditorGUILayout.TextField(label, text, options);
            }
            else
            {
                newValue = EditorGUILayout.TextField(label, text, style, options);
            }
            
            if ( newValue != text )
                // && GUI.GetNameOfFocusedControl() != _cname)
            {
                if (null != onValueChanged)
                {
                    onValueChanged?.Invoke(newValue);
                }
            }
            return newValue;
        }

        /// <summary>
        /// Text 文本框
        /// </summary>
        /// <param name="value"></param>
        /// <param name="onValueChanged"></param>
        public static bool Toggle(bool value, Action<bool> onValueChanged = null)
        {
            bool newValue = GUILayout.Toggle(value, "");
            if (newValue != value)
            {
                if (null != onValueChanged)
                {
                    onValueChanged?.Invoke(newValue);
                }
            }
            return newValue;
        }

    }
}