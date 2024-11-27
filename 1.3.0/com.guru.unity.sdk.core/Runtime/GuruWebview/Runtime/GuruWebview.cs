namespace Guru
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// Guru 内置浏览器
    /// </summary>
    public class GuruWebview
    {
        public const string Version = "0.0.2";
        public static float WindowFadeDuration = 0.35f;
        
        private static UniWebView CreateWebView()
        {
            var go = new GameObject("guru_webview");
            Object.DontDestroyOnLoad(go);
            var view = go.AddComponent<UniWebView>();
            // view.Frame = new Rect(0,0, Screen.width, Screen.height);
            // SafeArea fit
            var x = Screen.width - Screen.safeArea.width;
            if (x < 0) x = 0;
            var y = Screen.height - Screen.safeArea.height;
            if (y < 0) y = 0;
            view.Frame = new Rect(x, y, Screen.safeArea.width, Screen.safeArea.height);
            
            view.SetUserInteractionEnabled(true);
            return view;
        }

        /// <summary>
        /// 打开页面
        /// </summary>
        /// <param name="url">页面链接</param>
        /// <param name="showToolbar">显示工具条</param>
        /// <param name="waitForReady">等待加载完成后再显示页面</param>
        /// <param name="fadeIn">淡入显示效果</param>
        public static void OpenPage(string url, bool showToolbar = true, 
            bool waitForReady = true, bool fadeIn = true)
        {
            Debug.Log($"---- Guru Open Url: {url}");
            var view = CreateWebView();
            
            if (showToolbar)
            {
                view.EmbeddedToolbar.ShowNavigationButtons();
                view.EmbeddedToolbar.SetDoneButtonText(" X ");
                view.EmbeddedToolbar.Show();
            }

            view.Load(url);
            if (waitForReady)
            {
                view.OnPageFinished += (v, code, msg) =>
                {
                    // 加载完成后展示页面
                    view.Show(fadeIn, duration:WindowFadeDuration);
                };
            }
            else
            {
                view.Show(fadeIn, duration:WindowFadeDuration);   //直接加载页面
            }
        }

    }
}