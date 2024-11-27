

namespace Guru
{
    using UnityEngine;
    using System;
    
    [Serializable]
    public class AdsModel
    {
        internal AdsModelStorage _storage;
        
        public bool hasFirstRadsReward = false;
        public double tchAd001Value = 0;
        public double tchAd02Value = 0;
        public bool buyNoAds = false;
        
        
        public bool HasFirstRadsReward
        {
            get => hasFirstRadsReward;
            set
            {
                hasFirstRadsReward = value;
                Save();
            }
        }

        public double TchAD001RevValue
        {
            get => tchAd001Value;
            set
            {
                tchAd001Value = value;
                Save();
            }
        }
        
        public double TchAD02RevValue
        {
            get => tchAd02Value;
            set
            {
                tchAd02Value = value;
                Save();
            }
        }
        
        public bool BuyNoAds
        {
            get => buyNoAds;
            set
            {
                buyNoAds = value;
                Save();
            }
        }
        
        
        private void Save() => _storage.Save();

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
        
        public static AdsModel Create()
        {
            AdsModelStorage.Create(out var model, out var storage);
            model._storage = storage;
            return model;
        }
    }


    internal class AdsModelStorage : MonoBehaviour
    {
        private const string INSTANCE_NAME = "GuruSDK";
        private bool _needToSave = false;
        private bool _forceSave = false;
        private DateTime _lastSavedTime = new DateTime(1970,1,1);
        private float _saveInterval = 2;
        private AdsModel _model;

        public static void Create(out AdsModel model, out AdsModelStorage storage)
        {
            model = null;
            storage = null;
            
            var go = GameObject.Find(INSTANCE_NAME);
            if (go == null) go = new GameObject(INSTANCE_NAME);
            
            AdsModelStorage _ins = null;
            if (!go.TryGetComponent<AdsModelStorage>(out storage))
            {
                storage = go.AddComponent<AdsModelStorage>();
            }
            
            string json = PlayerPrefs.GetString(nameof(AdsModel), "");
            if (!string.IsNullOrEmpty(json))
            {
                model = JsonUtility.FromJson<AdsModel>(json);
            }
            else
            {
                model = new AdsModel();
            }
            
            model._storage = storage;
            storage._model  = model;
        }



        public void Save(bool forceSave = false)
        {
            _forceSave = forceSave;
            _needToSave = (DateTime.UtcNow - _lastSavedTime).TotalSeconds > _saveInterval;
        }


        private async void AsyncSaveData()
        {
            
        }


        #region 生命周期

        
        // 主线程进行写入操作
        void Update()
        {
            if (_needToSave)
            {
                var json = _model?.ToJson() ?? "";
                if (!string.IsNullOrEmpty(json))
                {
                    PlayerPrefs.SetString(nameof(AdsModel), json);
                    _needToSave = false;
                    _lastSavedTime = DateTime.UtcNow;
                }
            }

            if (_forceSave)
            {
                PlayerPrefs.Save();     
                _forceSave = false;
            }
        }

        // 监听特殊事件
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save(true);
            }
        }

        // 监听特殊事件
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Save(true);
            }
        }

        #endregion
    }


}