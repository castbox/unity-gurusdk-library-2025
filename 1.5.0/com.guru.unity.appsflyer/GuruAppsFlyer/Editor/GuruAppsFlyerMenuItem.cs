using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Guru;

namespace Guru.AppsFlyer.Editor
{
    
    public static class GuruAppsFlyerMenuItem
    {
        private const string InstanceName = "GuruAppsFlyerManager";
        
        [MenuItem("Guru/AppsFlyer/Add [GuruAppsFlyerManager] to Scene")]
        private static void AddGuruAppsFlyerManagerToScene()
        {
            // 在编辑器下当前打开的场景中，添加一个空的 GameObject 对象，并附着 GuruAppsflyerManagerV1 脚本
            // 之后保存当前的场景

            // 检查场景中是否已经存在 GuruAppsFlyerManager
            var existingObject = GameObject.Find(InstanceName);
            if (existingObject != null)
            {
                Debug.LogWarning("GuruAppsFlyerManager already exists in the scene!");
                Object.DestroyImmediate(existingObject);
            }

            // 创建新的 GameObject 并添加组件
            var go = new GameObject(InstanceName);
            go.AddComponent<GuruAppsflyerManagerV1>();
            
            // 获取当前活动场景
            var scene = SceneManager.GetActiveScene();
            
            // 标记场景为脏状态，表示场景已被修改
            EditorSceneManager.MarkSceneDirty(scene);
            
            // 保存当前场景
            EditorSceneManager.SaveScene(scene);
            
            Debug.Log($"GuruAppsFlyerManager has been added to scene '{scene.name}' and saved successfully!");
        }



    }
}