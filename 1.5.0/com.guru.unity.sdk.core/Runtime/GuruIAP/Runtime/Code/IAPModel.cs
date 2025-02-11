
using System.Collections.Generic;
using UnityEngine;

namespace Guru.IAP
{
    using System;
    using Newtonsoft.Json;
    
    [Serializable]
    public class IAPModel
    {
        public static readonly float SaveInterval = 3;
        public static readonly int MaxReceiptCount = 30;

        public static readonly string PlatformAndroid = "android";
        public static readonly string PlatformIOS = "ios";

        public bool isIapUser = false;
        public int buyCount = 0;
        public List<string> androidTokens;
        public List<string> iosReceipts;
        public List<GoogleOrderData> googleOrders;
        public List<AppleOrderData> appleOrders;
        
        /// <summary>
        /// 是否还有未上报的 Google Order
        /// </summary>
        public bool HasUnreportedGoogleOrder => (googleOrders?.Count ?? 0) > 0;
        
        /// <summary>
        /// 是否还有未上报的 Apple Order
        /// </summary>
        public bool HasUnreportedAppleOrder => (appleOrders?.Count ?? 0) > 0;


        #region Initialize

        public IAPModel()
        {
            androidTokens = new List<string>(MaxReceiptCount);
            iosReceipts = new List<string>(MaxReceiptCount);
            googleOrders = new List<GoogleOrderData>(20);
            appleOrders = new List<AppleOrderData>(20);
        }

        public static IAPModel Load()
        {
            IAPModel model = null;
            var json = LoadModelData();
            if (!string.IsNullOrEmpty(json))
            {
                model = JsonConvert.DeserializeObject<IAPModel>(json);
            }
            if(null != model) return model;
            return new IAPModel();
        }


        public void Save()
        {
            SaveModelData(this);
        }


        private static string LoadModelData()
        {
            return PlayerPrefs.GetString(nameof(IAPModel), "");
        }

        private static void SaveModelData(IAPModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            PlayerPrefs.SetString(nameof(IAPModel), json);
        }

        #endregion

        #region Google Token
        
        /// <summary>
        /// 添加 Google 的收据数据
        /// </summary>
        /// <param name="token"></param>
        public void AddToken(string token)
        {
            if (androidTokens == null) androidTokens = new List<string>(MaxReceiptCount);
            if(string.IsNullOrEmpty(token)) return;
            if(androidTokens.Count >= MaxReceiptCount) androidTokens.RemoveAt(0);
            androidTokens.Add(token);
            Save();
        }
        
        public bool IsTokenExists(string token)
        {
            if (androidTokens == null) return false;
            return androidTokens.Contains(token);
        }
        #endregion
        
        #region iOS Receipt
        /// <summary>
        /// 添加收据
        /// </summary>
        /// <param name="receipt"></param>
        public void AddReceipt(string receipt)
        {
            if (iosReceipts == null) iosReceipts = new List<string>(MaxReceiptCount);
            if(string.IsNullOrEmpty(receipt)) return;
            if(iosReceipts.Count >= MaxReceiptCount) iosReceipts.RemoveAt(0);
            iosReceipts.Add(receipt);
            Save();
        }

        
        
        public bool IsReceiptExist(string receipt)
        {
            if (iosReceipts == null) return false;
            return iosReceipts.Contains(receipt);
        }
    
        #endregion

        #region Google Orders

        public void AddGoogleOrder(GoogleOrderData order)
        {
            if (HasGoogleOrder(order)) return;
            googleOrders.Add(order);
            Save();
        }

        public bool HasGoogleOrder(GoogleOrderData order)
        {
            if(googleOrders == null || googleOrders.Count == 0) return false;
            for (int i = 0; i < googleOrders.Count; i++)
            {
                var o = googleOrders[i];
                if (o.Equals(order))
                {
                    return true;
                }
            }
            return false;
        }

        public bool RemoveGoogleOrder(GoogleOrderData order)
        {
            for (int i = 0; i < googleOrders.Count; i++)
            {
                var o = googleOrders[i];
                if (o.Equals(order))
                {
                    googleOrders.RemoveAt(i);
                    Save();
                    return true;
                }
            }
            return false;
        }
        public void ClearGoogleOrders()
        {
            googleOrders.Clear();
            Save();
        }

        #endregion
        
        #region Apple Orders
        
        public void AddAppleOrder(AppleOrderData order)
        {
            if (HasAppleOrder(order)) return;
            appleOrders.Add(order);
            Save();
        }
        
        public bool HasAppleOrder(AppleOrderData order) 
        {
            if(appleOrders == null || appleOrders.Count == 0) return false;
            for (int i = 0; i < appleOrders.Count; i++)
            {
                var o = appleOrders[i];
                if (o.Equals(order))
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool RemoveAppleOrder(AppleOrderData order)
        {
            for (int i = 0; i < appleOrders.Count; i++)
            {
                var o = appleOrders[i];
                if (o.Equals(order))
                {
                    appleOrders.RemoveAt(i);
                    Save();
                    return true;
                }
            }
            return false;
        }
        
        public void ClearAppleOrders()
        {
            appleOrders.Clear();
            Save();
        }
        #endregion

        #region Params


        public int PurchaseCount
        {
            get => buyCount;
            set
            {
                buyCount = value;
                if (buyCount > 0 && isIapUser == false) isIapUser = true;
                Save();
            }
        }

       
        public void SetIsIapUser(bool isIap)
        {
            isIapUser = isIap;
            Save();
        }

        #endregion

        #region ClearData

        public void ClearData()
        {
            googleOrders.Clear();
            appleOrders.Clear();
            androidTokens.Clear();
            iosReceipts.Clear();
            buyCount = 0;
            isIapUser = false;
            Save();
        }

        #endregion
    }


    


}