

namespace Guru
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;

    
    [Serializable]
    class PurchasedProduct
    {
        public string productName;
        public string productId;
        public string receipt;
        public bool appleProductIsRestored;
    }

    [Serializable]
    class GuruSDKSerializedModel
    {
        //-------------- data ---------------
        
        public string uid = "";
        public int b_level = 0;
        public int b_play = 0;
        public bool no_ads = false;
        public bool debug_mode = false;
        public long last_active_ts = 0; // 用户上次登录时间戳
        public int lt = 1; // 添加登录次数字段,初始值为1
        public List<PurchasedProduct> purchased = new List<PurchasedProduct>(10);
        
        //-------------- data ---------------
    }

    [Serializable]
    internal class GuruSDKModel: ILTPropertyDataHolder
    {
        private const float SAVE_INTERVAL = 3;
        private const string SAVE_KEY = "com.guru.sdk.model.save";
        
        private DateTime _lastSavedTime = DateTime.UnixEpoch;
        private DateTime _lastActiveTime;

        private bool _noAds = false;
        private int _bLevel;
        private int _bPlay;
        private string _uid;
        private bool _debugMode = false;
        private int _lt;
        private List<PurchasedProduct> _purchased;


        private static GuruSDKModel _instance;

        public static GuruSDKModel Instance
        {
            get
            {
                if (null == _instance) _instance = new GuruSDKModel();
                return _instance;
            }
        }

        public GuruSDKModel()
        {
            // 读取内存值
            GuruSDKSerializedModel model = LoadModel();
            _uid = model.uid;
            _noAds = model.no_ads;
            _bLevel = model.b_level;
            _bPlay = model.b_play;
            _purchased = model.purchased;
            _debugMode = model.debug_mode;
            _lt = model.lt;
            _lastActiveTime = TimeUtil.ConvertTimeSpanToDateTime(model.last_active_ts);
        }

        
        public int BLevel
        {
            get => _bLevel;
            set
            {
                if (value < _bLevel)
                {
                    // b_level 必须比上一次的值大
                    UnityEngine.Debug.LogWarning($"[SDK] :: Set b_level [{value}] should not be less than original value [{_bLevel}]");
                    return;
                }

                _bLevel = value;
                Save();
            }
        }

        public int BPlay
        {
            get => _bPlay;
            set
            {
                _bPlay = value;
                Save();
            }
        }

        public string UserId
        {
            get => _uid;
            set
            {
                _uid = value;
                Save();
            }
        }


        public bool IsIapUser => _purchased.Count > 0;

        public bool IsNoAds
        {
            get => _noAds;
            set
            {
                _noAds = value;
                Save();
            }
        }

        public bool IsDebugMode
        {
            get => _debugMode;
            set
            {
                _debugMode = value;
                Save();
            }
        }

        public int LT
        {
            get => _lt;
            set => _lt = value;
        }

        public DateTime LastActiveTime
        {
            get => _lastActiveTime;
            set => _lastActiveTime = value;
        }


        #region 初始化


        private GuruSDKSerializedModel LoadModel()
        {
            GuruSDKSerializedModel model = null;
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                var json = PlayerPrefs.GetString(SAVE_KEY, "");
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        model = JsonUtility.FromJson<GuruSDKSerializedModel>(json);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }
                }
            }

            if (model == null) model = new GuruSDKSerializedModel();
            return model;
        }

        /// <summary>
        /// 保存至 PlayerPrefs 数据
        /// </summary>
        private void SetToMemory()
        {
            var model = new GuruSDKSerializedModel()
            {
                uid = _uid,
                b_level = _bLevel,
                b_play = _bPlay,
                no_ads = _noAds,
                purchased = _purchased,
                debug_mode = _debugMode,
                last_active_ts = TimeUtil.GetTimeStamp(_lastActiveTime),
                lt = _lt
            };

            var json = JsonUtility.ToJson(model);
            if (!string.IsNullOrEmpty(json))
            {
                PlayerPrefs.SetString(SAVE_KEY, json);
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="forceSave"></param>
        public void Save(bool forceSave = false)
        {
            SafeSaveInMainThread(forceSave).Forget();
        }

        /// <summary>
        /// 安全存储到主线程
        /// </summary>
        /// <param name="forceSave"></param>
        private async UniTask SafeSaveInMainThread(bool forceSave = false)
        {
            await UniTask.SwitchToMainThread();
            
            SetToMemory(); // 每次保存都要设置到 PlayerPrefs 内

            bool shouldWriteToDisk = forceSave || (DateTime.Now - _lastSavedTime) >= TimeSpan.FromSeconds(SAVE_INTERVAL);
            if (!shouldWriteToDisk) return;
            _lastSavedTime = DateTime.Now; // 更新最后保存时间
            PlayerPrefs.Save(); // 写入到磁盘
        }

        #endregion
        
        #region 订单记录
        
        /// <summary>
        /// 订单是否存在
        /// </summary>
        /// <param name="receipt">收据凭证字段</param>
        /// <returns></returns>
        public bool HasPurchasedProduct(string receipt)
        {
            if(_purchased.Count == 0) return false;
            return _purchased.Exists(p => p.receipt == receipt);
        }

        /// <summary>
        /// 添加已支付订单
        /// </summary>
        /// <param name="receipt"></param>
        /// <param name="productName"></param>
        /// <param name="productId"></param>
        /// <param name="appleProductIsRestored"></param>
        public void AddReceipt(string receipt, string productName, string productId, bool appleProductIsRestored = false)
        {
            if (!HasPurchasedProduct(receipt))
            {
                _purchased.Add(new PurchasedProduct()
                {
                    receipt = receipt,
                    productName = productName,
                    productId = productId,
                    appleProductIsRestored = appleProductIsRestored
                });
               Save();
            }
        }

        public string[] GetReceipts(string productName)
        {
            var receipts = new List<string>();
            receipts.AddRange(from purchasedProduct in _purchased where purchasedProduct.productName == productName select purchasedProduct.receipt);
            return receipts.ToArray();
        }
        
        public string[] GetReceiptsById(string productId)
        {
            var receipts = new List<string>();
            receipts.AddRange(from purchasedProduct in _purchased where purchasedProduct.productId == productId select purchasedProduct.receipt);
            return receipts.ToArray();
        }
        
        #endregion
        
        #region 清除数据

        public void ClearData()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
        }

        #endregion
        
    }








}