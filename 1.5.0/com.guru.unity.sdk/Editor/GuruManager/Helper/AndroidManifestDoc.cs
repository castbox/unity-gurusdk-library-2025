
namespace Guru.Editor
{
    using System.Xml;
    using System.IO;
    using UnityEngine;
    using System.Collections.Generic;
    
    /// <summary>
    /// Android 配置修改器
    /// </summary>
    public class AndroidManifestDoc
    {
        private const string TargetPath = "Plugins/Android/AndroidManifest.xml";
        private const string XmlnsAndroid = "xmlns:android";
        private const string NamespaceAndroid = "http://schemas.android.com/apk/res/android";
        private const string XmlnsTools= "xmlns:tools";
        private const string NamespaceTools = "http://schemas.android.com/tools";
        
        private const string UserPermission = "uses-permission";
        private const string MetaData = "meta-data";
        private const string KName = "name";

        private XmlDocument _doc;
        public XmlDocument Doc => _doc;

        private string _docPath;
        private bool _isReady = false;
        
        private XmlElement _manifestNode;
        private XmlElement _applicationNode;
        private XmlElement _queriesNode;

        
        
        
        
        #region Initiallize
        
        /// <summary>
        /// 加载文件
        /// </summary>
        /// <param name="docPath"></param>
        /// <returns></returns>
        public static AndroidManifestDoc Load(string docPath = "")
        {
            if (string.IsNullOrEmpty(docPath))
            {
                docPath = Path.GetFullPath(Path.Combine(Application.dataPath, TargetPath));
            }

            if (!File.Exists(docPath))
            {
                Debug.LogError($"--- File not found: {docPath}");
                return null;
            }

            var mod = new AndroidManifestDoc();
            mod.ReadFromPath(docPath);
            return mod;
        }

        public static AndroidManifestDoc Read(string xmlStr, string docPath = "")
        {
            var mod = new AndroidManifestDoc();
            mod.ReadFromXml(xmlStr, docPath);
            return mod;
        }
        

        /// <summary>
        /// 从文件路径读取
        /// </summary>
        /// <param name="docPath"></param>
        public void ReadFromPath(string docPath)
        {
            _isReady = false;
            if (File.Exists(docPath))
            {
                var xmlStr = File.ReadAllText(docPath);
                ReadFromXml(xmlStr, docPath);
            }
            else
            {
                Debug.LogError($"--- File not found: {docPath}");
            }
        }


        public void ReadFromXml(string xmlStr, string docPath = "")
        {
            _doc = new XmlDocument();
            _doc.LoadXml(xmlStr);
            if(!string.IsNullOrEmpty(docPath)) _docPath = docPath;
            Init();
        }


        /// <summary>
        /// Initializes the Doc
        /// </summary>
        private void Init()
        {
            // --- Root Nodes ---
            _manifestNode = _doc.SelectSingleNode("manifest") as XmlElement;
            _applicationNode = _doc.SelectSingleNode("manifest/application") as XmlElement;
            _queriesNode = _doc.SelectSingleNode("manifest/queries") as XmlElement;
            
            AddXmlnsAndroid();
            AddXmlnsTools();
            _isReady = true;
        }

        /// <summary>
        /// Save Doc
        /// </summary>
        public void Save(string docPath = "")
        {
            if (_isReady)
            {
                if (!string.IsNullOrEmpty(docPath)) _docPath = docPath;
                if (!string.IsNullOrEmpty(_docPath))
                {
                    var dir = Directory.GetParent(_docPath);
                    if(!dir.Exists) dir.Create();
                    _doc.Save(_docPath);

                }

            }
        }
        
        /// <summary>
        /// 寻找主 Activity 节点
        /// </summary>
        public XmlElement GetMainActivityNode()
        {
            var activities = _applicationNode.SelectNodes("activity");

            if (activities == null || activities.Count == 0)
            {
                return null; // 找不到主 activity 
            }

            // 扫描节点， 获取主 Activity 的 Node
            foreach (XmlElement a in activities)
            {
                if(a == null) continue;
                
                if (a.InnerXml.Contains("android.intent.action.MAIN") ||
                    a.InnerXml.Contains("android.intent.category.LAUNCHER"))
                {
                    return a;
                }
            }
            
            return activities[0] as XmlElement;
        }


        #endregion

        #region Node Opreation

        /// <summary>
        /// 添加属性
        /// </summary>
        /// <param name="node"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="namespaceUri"></param>
        /// <returns></returns>
        public bool AddAttribute(XmlElement node, string key, string value, string namespaceUri = "")
        {
            if (node != null)
            {
                if (string.IsNullOrEmpty(namespaceUri))
                {
                    node.SetAttribute(key, value);
                    return true;
                    
                }
                
                node.SetAttribute(key, namespaceUri, value);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 添加 Android Tools 工具节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddAndroidAttribute(XmlElement node, string key, string value)
        {
            return AddAttribute(node, key, value, NamespaceAndroid);
        }
        

        #endregion
        
        #region API

        public bool AddXmlnsAndroid()
        {
            return AddAttribute(_manifestNode, XmlnsAndroid, NamespaceAndroid);
        }
        
        public bool AddXmlnsTools()
        {
            return AddAttribute(_manifestNode, XmlnsTools, NamespaceTools);
        }
        
        /// <summary>
        /// Add Replace Item
        /// </summary>
        /// <param name="item"></param>
        public void AddApplicationReplaceItem(string item)
        {
            if (_applicationNode != null)
            {
                List<string> items = new List<string>(5);
                if (_applicationNode.HasAttribute("replace", NamespaceTools))
                {
                    var arr = _applicationNode.GetAttribute("replace",NamespaceTools).Split(',');
                    if(arr != null && arr.Length > 0)
                    {
                        items.AddRange(arr);
                    }
                }

                if (!items.Contains(item)) items.Add(item);
                
                _applicationNode.SetAttribute("replace",  NamespaceTools, string.Join(",", items));
            }
        }
        
        public void SetApplicationAttribute(string key, string value)
        {
            if (_applicationNode != null)
            {
                _applicationNode.SetAttribute(key, NamespaceAndroid, value);
            }
        }
        
        /// <summary>
        /// Set metadata
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="valueName"></param>
        /// <param name="keyName"></param>
        public void SetMetadata(string key, string value, string valueName = "value", string keyName = KName)
        {
            if (_doc == null || !_isReady) return;
            
            XmlElement node = null;
            if (!TryGetMetadata(key, out node, keyName))
            {
                node = AddChildNode(MetaData, _applicationNode);
            }

            node.SetAttribute(keyName, NamespaceAndroid, key);
            node.SetAttribute(valueName, NamespaceAndroid, value);
        }

        /// <summary>
        /// 添加元素点
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public XmlElement AddChildNode(string name, XmlNode parent = null)
        {
            var e = _doc.CreateElement(name);
            parent?.AppendChild(e);
            return e;
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool RemoveChildNode(XmlNode node, XmlNode parent = null)
        {
            if(node == null) return false;
            if (parent != null)
            {
                parent.RemoveChild(node);
                return true;
            }
            _doc.RemoveChild(node);
            return true;
        }


        /// <summary>
        /// 添加权限
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyName"></param>
        public void AddPermission(string key, string keyName = KName)
        {
            if (_doc == null || !_isReady) return;

            XmlElement node = null;
            if(!TryGetPermission(key, out node, keyName))
            {
                node = AddChildNode(UserPermission, _manifestNode);
                // _manifestNode?.AppendChild(node);
            }
            node.SetAttribute(keyName, NamespaceAndroid, key);
        }
        
        /// <summary>
        /// 删除 Permission
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="keyName"></param>
        /// <param name="valueName"></param>
        public void RemovePermission(string key, string value = "remove", string keyName = "name",  string valueName = "node")
        {
            if (_doc == null || !_isReady) return;
            
            XmlElement node = null;
            if(!TryGetPermission(key, out node, keyName))
            {
                node = AddChildNode(UserPermission, _manifestNode);
            }
            node.SetAttribute(keyName, NamespaceAndroid, key);
            node.SetAttribute(valueName, NamespaceTools, value);
        }
        
        public bool SetPackageName(string packageName)
        {
            if (_manifestNode != null)
            {
                _manifestNode.Attributes["package"].Value = packageName;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 添加 Queries Intent
        /// </summary>
        /// <param name="value"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public void AddQueriesIntent(string value, string keyName = "name")
        {
            if (_queriesNode == null)
            {
                _queriesNode = AddChildNode("queries", _manifestNode);
            }

            var intentList = _queriesNode.SelectNodes("intent");
            if (intentList != null)
            {
                foreach (XmlElement intent in intentList)
                {
                    var action = intent?.SelectSingleNode("action") as XmlElement;
                    
                    if (action != null 
                        && action.GetAttribute(keyName, NamespaceAndroid) == value)
                    {
                        return; // Has injected，skip ...
                    }
                }
            }
            
            // Inject new intent node
            XmlElement intentNode = _doc.CreateElement("intent");
            _queriesNode.AppendChild(intentNode);
            XmlElement actionNode = _doc.CreateElement("action");
            intentNode.AppendChild(actionNode);
            actionNode.SetAttribute(keyName, NamespaceAndroid, value);
        }

        #endregion

        #region Data Opration

        public bool TryGetMetadata(string name, out XmlElement node, string keyName = KName)
        {
            node = null;

            if(_applicationNode != null)
            {
                var list = _applicationNode.SelectNodes(MetaData);
                if (list != null)
                {
                    foreach (XmlElement e in list)
                    {
                        if (e.GetAttribute(keyName, NamespaceAndroid) == name)
                        {
                            node = e;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        
        public bool TryGetPermission(string name, out XmlElement node, string keyName = KName)
        {
            node = null;

            if(_manifestNode != null)
            {
                var list = _manifestNode.SelectNodes(UserPermission);
                if (list != null)
                {
                    foreach (XmlElement e in list)
                    {
                        if (e.GetAttribute(keyName, NamespaceAndroid) == name)
                        {
                            node = e;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        public bool TryRootNode(string nodeName, out XmlElement node)
        {
            node = null;
            if (_doc != null)
            {
                node = _doc.SelectSingleNode(nodeName) as XmlElement;
                return node != null;
            }
            return false;
        }
        
        #endregion
        
        #region Output
        
        public override string ToString()
        {
            if (_doc != null) return _doc.InnerXml;
            return this.ToString();
        }


        #endregion
        
    }
}