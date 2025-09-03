
namespace Guru
{
    using System;
    using UnityEngine.Serialization;
    using System.Collections.Generic;
    using UnityEngine;
    
    [Serializable]
    public class GuruServicesConfig
    {
        public long version = 0;
        public GuruAppSettings app_settings;
        public GuruParameters parameters;
        public GuruAdjustSettings adjust_settings;
        public GuruFbSettings fb_settings;
        public GuruAdSettings ad_settings;
        public string[] products;
        
        //-------------------------------- 配置检测 --------------------------------
        public bool IsAmazonAndroidEnabled() => ad_settings != null && 
                                                ad_settings.amazon_ids_android != null &&
                                                ad_settings.amazon_ids_android.Length > 0;
        public bool IsAmazonIOSEnabled() => ad_settings != null && 
                                                ad_settings.amazon_ids_ios != null &&
                                                ad_settings.amazon_ids_ios.Length > 0;
        public bool IsPubmaticAndroidEnabled() => ad_settings != null && 
                                            ad_settings.pubmatic_ids_android != null &&
                                            ad_settings.pubmatic_ids_android.Length > 0;
        public bool IsPubmaticIOSEnabled() => ad_settings != null && 
                                                  ad_settings.pubmatic_ids_ios != null &&
                                                  ad_settings.pubmatic_ids_ios.Length > 0;
        public bool IsMolocoAndroidEnabled() => ad_settings != null && 
                                                  ad_settings.moloco_ids_android != null &&
                                                  ad_settings.moloco_ids_android.Length > 0;
        public bool IsMolocoIOSEnabled() => ad_settings != null && 
                                                ad_settings.moloco_ids_ios != null &&
                                                ad_settings.moloco_ids_ios.Length > 0;
        public bool IsTradplusAndroidEnabled() => ad_settings != null && 
                                            ad_settings.tradplus_ids_android != null &&
                                            ad_settings.tradplus_ids_android.Length > 0;
        public bool IsTradplusIOSEnabled() => ad_settings != null && 
                                              ad_settings.tradplus_ids_ios != null &&
                                              ad_settings.tradplus_ids_ios.Length > 0;
        public bool IsIAPEnabled() => app_settings != null && app_settings.enable_iap 
                                                           && products != null && products.Length > 0;
        public bool UseCustomKeystore() => app_settings?.custom_keystore ?? false;
        
        public bool IsFirebaseEnabled() => app_settings?.enable_firebase ?? true;
        public bool IsFacebookEnabled() => app_settings?.enable_facebook ?? true;
        public bool IsAdjustEnabled() => app_settings?.enable_adjust ?? true;
        public bool IsAppsflyerEnabled() => app_settings?.enable_appsflyer ?? false;
        public bool IsThinkingDataEnabled() => app_settings?.enable_thinkingdata ?? false;
        
        public string AppBundleId() => app_settings?.bundle_id ?? "";
        
        public string GetStoreUrl()
        {
            string storeUrl = "";
            if (app_settings != null)
            {
                storeUrl = app_settings.android_store;
#if UNITY_IOS
                storeUrl = app_settings.ios_store;   
#endif
            }
            return storeUrl;
        }
        
        //-------------------------------- 配置检测 -------------------------------

        
        
        //-------------------------------- Parameters --------------------------------
        public TchFbModeEnum GetTchFacebookMode()
        {
            if (parameters == null) return TchFbModeEnum.Mode001;

            if (parameters.tch_020 > 0) return TchFbModeEnum.Mode02;
            
            var value = parameters?.tch_fb_mode ?? 0;
            if (Enum.IsDefined(typeof(TchFbModeEnum), value))
            {
                return (TchFbModeEnum)value;
            }
            return TchFbModeEnum.Mode001;
        }

        public bool GetIsAppReview() => parameters?.apple_review ?? false;
        public bool GetEnableAnaErrorLog() => parameters?.enable_errorlog ?? false;
        public bool GetIsAdsCompliance() => parameters?.ads_compliance ?? false;
        public bool GetDMACountryCheck() => parameters?.dma_country_check ?? false;
        public string GetDMAMapRule() => parameters?.dma_map_rule ?? "";
        public bool GetUseUUID() => parameters?.using_uuid ?? false;
        [Obsolete("不再使用 Keywords 功能， 此开关即将被废弃，请不要再调用")]
        public bool GetKeywordsEnabled() => parameters?.enable_keywords ?? false; 
        public int GetTokenValidTime() => parameters?.token_valid_time ?? 604800;
        public int GetLevelEndSuccessNum() => parameters?.level_end_success_num ?? 50;
        public string GetCdnHost() => parameters?.cdn_host ?? "";
        public bool GetUsingUUID() => parameters?.using_uuid ?? true;
        public string[] GetUrlSchemaList() => parameters?.url_schema ?? null;
        //-------------------------------- Parameters --------------------------------
        
    }
    
    /// <summary>
    /// GuruApp 基础配置参数
    /// </summary>
    [Serializable]    
    public class GuruAppSettings
    {
        public string app_id;
        public string product_name;
        public string bundle_id;
        public string support_email;
        public string privacy_url;
        public string terms_url;
        public string android_store;
        public string ios_store;
        public bool enable_firebase = true;
        public bool enable_facebook = true;
        public bool enable_iap = false;
        public bool custom_keystore = false;
        public bool enable_adjust = true;
        public bool enable_appsflyer = false;
        public bool enable_thinkingdata = false;
    }
    
    /// <summary>
    /// GuruApp 可修改配置参数
    /// 详细注释参考： https://docs.google.com/spreadsheets/d/1WEYm52VAWGuqhXA2_4X0zafmm5r4asAUcvbsu4qklng/edit?gid=0#gid=0 
    /// </summary>
    [Serializable]
    public class GuruParameters
    {
        public int token_valid_time = 604800;
        public int level_end_success_num = 50;
        public bool enable_keywords = false;
        public float tch_020 = 0;   // 老版本的 tch_02 开关， 会在下个版本中移除 
        public int tch_fb_mode = 0;
        public bool using_uuid = false;
        public string dma_map_rule = "";
        public bool dma_country_check = false;
        public bool apple_review = false; // 苹果审核标志位
        public bool enable_errorlog = false;
        public bool ads_compliance = false;
        public string cdn_host = "";
        public string[] url_schema = null; // 设置 UrlSchemas
    }

    [Serializable]
    public class GuruAdjustSettings
    {
        public string[] app_token;
        public string[] events;

        public string AndroidToken() => app_token != null && app_token.Length > 0 ? app_token[0] : "";
        public string iOSToken() => app_token != null && app_token.Length > 1 ? app_token[1] : "";
    }
    
    [Serializable]
    public class GuruFbSettings
    {
        public string fb_app_id;
        public string fb_client_token;
    }

    [Serializable]
    public class GuruAdSettings
    {
        public string sdk_key;
        public string[] admob_app_id;
        public string[] max_ids_android;
        public string[] max_ids_ios;
        public string[] amazon_ids_android;
        public string[] amazon_ids_ios;
        public string[] pubmatic_ids_android;
        public string[] pubmatic_ids_ios;
        public string[] moloco_ids_android;
        public string[] moloco_ids_ios;
        public string[] tradplus_ids_android;
        public string[] tradplus_ids_ios;


        
        private bool TryGetAdUnitIds(string[] ids_array, out string bannerId, out string interstitialId, out string rewardedId , int startIndex = 0)
        {
            bannerId = "";
            interstitialId = "";
            rewardedId = "";
            
            if (ids_array == null) return false;
            if (ids_array.Length < startIndex + 3) return false;
            
            bannerId = ids_array[startIndex];
            interstitialId = ids_array[startIndex + 1];
            rewardedId = ids_array[startIndex + 2];
            
            return true;
        }
        
        
        /// <summary>
        /// 获取 MAX 广告 ID
        /// </summary>
        /// <param name="bannerId"></param>
        /// <param name="interstitialId"></param>
        /// <param name="rewardedId"></param>
        /// <returns></returns>
        public bool TryGetMaxUnitIds(out string bannerId, out string interstitialId, out string rewardedId)
        {
            string[] ids = max_ids_android;
#if UNITY_IOS
            ids = max_ids_ios;
#endif
            return TryGetAdUnitIds(ids, out bannerId, out interstitialId, out rewardedId);
        }
        
        /// <summary>
        /// 获取 Amazon ID
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="bannerId"></param>
        /// <param name="interstitialId"></param>
        /// <param name="rewardedId"></param>
        /// <returns></returns>
        public bool TryGetAmazonUnitIds(out string appId, out string bannerId, out string interstitialId, out string rewardedId)
        {
            appId = bannerId = interstitialId = rewardedId = "";
            
            string[] ids = amazon_ids_android;
#if UNITY_IOS
            ids = amazon_ids_ios;
#endif
            
            if (ids == null) return false;
            if (ids.Length < 4) return false;

            appId = ids[0];
            bannerId = ids[1];
            interstitialId = ids[2];
            rewardedId = ids[3];

            return true;
        }
        
        public bool TryGetPubmaticUnitIds(out string bannerId, out string interstitialId, out string rewardedId)
        {
            string[] ids = pubmatic_ids_android;
#if UNITY_IOS
            ids = pubmatic_ids_ios;
#endif
            return TryGetAdUnitIds(ids, out bannerId, out interstitialId, out rewardedId);
        }
        
        public bool TryGetTradPlusUnitIds(out string bannerId, out string interstitialId, out string rewardedId)
        {
            string[] ids = tradplus_ids_android;
#if UNITY_IOS
            ids = tradplus_ids_ios;
#endif
            return TryGetAdUnitIds(ids, out bannerId, out interstitialId, out rewardedId);
        }
        
        public bool TryGetMolocoUnitIds(out string bannerId, out string interstitialId, out string rewardedId)
        {
            string[] ids = moloco_ids_android;
#if UNITY_IOS
            ids = moloco_ids_ios;
#endif
            return TryGetAdUnitIds(ids, out bannerId, out interstitialId, out rewardedId);
        }
    }
    
}