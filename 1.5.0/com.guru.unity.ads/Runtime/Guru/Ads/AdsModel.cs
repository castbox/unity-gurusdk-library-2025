using Cysharp.Threading.Tasks;

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
        public string prevFbAdDate;
        
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

        public DateTime PreviousFBAdRevenueDate
        {
            get
            {
                if (!string.IsNullOrEmpty(prevFbAdDate) 
                    && DateTime.TryParse(prevFbAdDate, out DateTime date))
                {
                    return date;
                }
                return new DateTime(1970, 1, 1);
            }
            
            set
            {
                prevFbAdDate = value.ToString("g");
                Save();
            }
        }



        private void Save() => _storage.Save().Forget();

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
        // private bool _needToSave = false;
        // private bool _forceSave = false;
        private DateTime _lastSavedTime = DateTime.UnixEpoch;
        private readonly float _saveInterval = 2;
        private AdsModel _model;

        public static void Create(out AdsModel model, out AdsModelStorage storage)
        {
            model = null;
            storage = null;
            
            var go = GameObject.Find(INSTANCE_NAME);
            if (go == null) go = new GameObject(INSTANCE_NAME);
            
            if (!go.TryGetComponent(out storage))
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



        public async UniTaskVoid Save(bool forceSave = false)
        {
            var canSave = forceSave;
            if (!forceSave)
            {
                canSave = (DateTime.UtcNow - _lastSavedTime).TotalSeconds > _saveInterval;
            }

            if (canSave)
            {
                await UniTask.SwitchToMainThread();
                var json = _model?.ToJson() ?? "";
                if (!string.IsNullOrEmpty(json))
                {
                    PlayerPrefs.SetString(nameof(AdsModel), json);
                    _lastSavedTime = DateTime.UtcNow;
                }
                PlayerPrefs.Save(); 
            }
        }


        #region 生命周期

        
        // 主线程进行写入操作
        // void Update()
        // {
        //     if (_needToSave)
        //     {
        //         var json = _model?.ToJson() ?? "";
        //         if (!string.IsNullOrEmpty(json))
        //         {
        //             PlayerPrefs.SetString(nameof(AdsModel), json);
        //             _needToSave = false;
        //             _lastSavedTime = DateTime.UtcNow;
        //         }
        //     }
        //
        //     if (_forceSave)
        //     {
        //         PlayerPrefs.Save();     
        //         _forceSave = false;
        //     }
        // }

        // 监听特殊事件
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save(true).Forget();
            }
        }

        // 监听特殊事件
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Save(true).Forget();
            }
        }

        #endregion
    }


}