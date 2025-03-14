
using System.Collections.Generic;

namespace Guru
{
    using System;
    using UnityEngine;
    using AdjustSdk;
    
    
    
    
    /// <summary>
    /// Revenue Model
    /// </summary>
    [Serializable]
    public class AdjustModel
    {
        private const string SAVE_KEY = "gurusdk_adjust_ad_rervenue_model";

        public int impressionCount = 0;
        public double revenue = 0;
        public string reportDate = "";
        public AdjustAttribution attribution = null;
        
        public static AdjustModel LoadOrCreate()
        {
            var json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                return JsonUtility.FromJson<AdjustModel>(json);
            }
            return new AdjustModel();
        }

        private void Save()
        {
            var json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(SAVE_KEY, json);
        }

        /// <summary>
        /// 添加一次广告收益
        /// </summary>
        /// <param name="value"></param>
        public void AddImpressionRevenue(double value)
        {
            revenue += value;
            impressionCount++;
            Save();
        }

        /// <summary>
        /// 设置上次上报时间
        /// </summary>
        /// <param name="date"></param>
        public void SetReportDate(DateTime date)
        {
            reportDate = date.ToString("g");
            Save();
        }
        
        
        /// <summary>
        /// 设置上报日期且清零
        /// </summary>
        /// <param name="date"></param>
        public void SetReportDateAndClear(DateTime date)
        {
            reportDate = date.ToString("g");
            impressionCount = 0;
            revenue = 0;
            Save();
        }
        
        /// <summary>
        /// 是否已经上报过累计值了
        /// </summary>
        /// <returns></returns>
        public bool HasAccumulateRevenueReported()
        {
            return !string.IsNullOrEmpty(reportDate);
        }


        public bool HasData()
        {
            return impressionCount > 0 || revenue > 0;
        }


        public override string ToString()
        {
            return $"[{nameof(AdjustModel)}]  impressionCount:{impressionCount},  revenue:{revenue}";
        }
        
        /// <summary>
        /// 用户来源渠道（如自然流量、Google等）
        /// </summary>
        public AdjustAttribution Attribution
        {
            get => attribution;
            set {
                attribution = value;
                Save();
            }
        }
    }
}