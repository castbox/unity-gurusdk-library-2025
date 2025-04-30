

namespace Guru
{
    using System;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using Firebase.Analytics;

#if GURU_ADJUST
    using AdjustSdk;
#endif
    
#if GURU_APPSFLYER
    using AppsFlyerSDK;
#endif
    
    using UnityEngine;
    
    /// <summary>
    /// DMA Helper for Google Ads Consent Mode DMA
    /// details: https://docs.google.com/document/d/1p7ad-W6XnqPjMgFkvoVf1Yylsogm_PykD9_nauInVoI/edit#heading=h.p5yfuyv0ds3k
    /// google doc: https://developers.google.com/tag-platform/security/guides/app-consent?platform=android&consentmode=advanced&hl=zh-cn
    /// The helper generates the corresponding result by means of an 11-bit Consent purpose.
    /// </summary>
    public class GoogleDMAHelper
    {
        private static string Tag => GuruConsent.Tag;
        
        public static readonly bool UsingDelayAppMeasurement = false;

        public static List<string> PurposesRules = new List<string>
        {
            "0",   // 1
            "",    // *
            "2,3",  // 3&4
            "0,6"   // 1&7
        };


        private static readonly string DefaultMapRules = "1,0,3&4,1&7";  // default rule by LiHao
        
        
        private static readonly List<string> EEARegionCodes = new List<string>
        {
            "at", "be", "bg", "hr", "cy", "cz", "dk", "ee", "fi", "fr", "de", "el", "hu", "ie", "it", "lv", "lt", "lu", "mt", "nl", "pl", "pt", "ro", "sk", "si", "es", "se", "no", "is", "li"
        };
        
        private static string DmaResult
        {
            get => PlayerPrefs.GetString(nameof(DmaResult), "");
            set => PlayerPrefs.SetString(nameof(DmaResult), value);
        }

        /// <summary>
        /// Set DMA status
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mapRule">map rules </param>
        /// <param name="enableCountryCheck"></param>
        public static void SetDMAStatus(string value, string mapRule = "", bool enableCountryCheck = false)
        {
            if (!IsUserInEEACountry(value, enableCountryCheck))
            {
                // No EEA countries skip reporting the data
                Debug.Log($"{Tag} --- GoogleDMAHelper:: User is not at EEA countries, purposes: {value}");
                if (string.IsNullOrEmpty(DmaResult) || DmaResult != "not_eea" )
                {
                    DmaResult = "not_eea";
                    ReportAdjustDMAValue(false);
                    ReportAppsflyerDMAValue(false);
                    return;
                }
                return;
            }
            
            // build an array<bool> for record result from native callback
            // sometimes it will feedback with '0' so we need to fill the array with 'false' by 11 times.
            string purposeStr = "";
            int len = 11;
            bool[] purposes = new bool[len];
            for(int i = 0; i < len; i++)
            {
                if (i < value.Length)
                {
                    purposes[i] = value[i] == '1';
                    purposeStr += value[i].ToString();
                }
                else
                {
                    purposes[i] = false;
                    purposeStr += "0";
                }
            }
            
            // build an dict<type, status> for record consent data
            Dictionary<ConsentType, ConsentStatus> consentData = ApplyPurposesRules(purposes, mapRule);
            
            // build result data for guru analytics
            string result = ApplyGoogleMapRule(consentData);  // Google rules
            Debug.Log($"{Tag} --- GoogleDMAHelper::SetDMAStatus - status:{purposeStr}  result: {result}  rule:{mapRule}");
            
            //---------- Firebase report --------------
            FirebaseAnalytics.SetConsent(consentData);
            
            //---------- Adjust report --------------
            // AdjustThirdPartySharing adjustThirdPartySharing = new AdjustThirdPartySharing(null);
            // adjustThirdPartySharing.addGranularOption("google_dma", "eea", "1");
            // adjustThirdPartySharing.addGranularOption("google_dma", "ad_personalization", $"{result[2]}");
            // // adjustThirdPartySharing.addGranularOption("google_dma", "ad_user_data", $"{result[3]}");  // From Haoyi's advice we don't give the value so Adjust will still receive the data as a trick.
            // Adjust.trackThirdPartySharing(adjustThirdPartySharing);

            ReportAdjustDMAValue(true, result[2].ToString(), result[3].ToString());
            ReportAppsflyerDMAValue(true, consentData);
            //----------- Guru DMA report ---------------
            ReportResult(purposeStr, result);
        }


        private static void ReportAdjustDMAValue(bool isEEAUser, string personalization = "1", string userData = "1")
        {
#if GURU_ADJUST
            AdjustThirdPartySharing adjustThirdPartySharing = new AdjustThirdPartySharing(null);
            
            if (isEEAUser)
            {
                // From Haoyi's advice we don't give the value so Adjust will still receive the data as a trick.
                adjustThirdPartySharing.AddGranularOption("google_dma", "eea", "1");
                adjustThirdPartySharing.AddGranularOption("google_dma", "ad_personalization", personalization);
                // adjustThirdPartySharing.addGranularOption("google_dma", "ad_user_data", userData);  
            }
            else
            {
                // No eea user, all set to granted
                adjustThirdPartySharing.AddGranularOption("google_dma", "eea", "0");
                adjustThirdPartySharing.AddGranularOption("google_dma", "ad_personalization", "1");
                adjustThirdPartySharing.AddGranularOption("google_dma", "ad_user_data", $"1");
            }
            
            Adjust.TrackThirdPartySharing(adjustThirdPartySharing);
#endif
        }

        private static void ReportAppsflyerDMAValue(bool isEEAUser, Dictionary<ConsentType, ConsentStatus> consentData = null)
        {
#if GURU_APPSFLYER
            bool? isUserSubjectToGdpr;
            bool? hasConsentForDataUsage;
            bool? hasConsentForAdsPersonalization;
            
            Log.I($"Adjust SetConsent {isEEAUser}");
            if (isEEAUser)
            {
                isUserSubjectToGdpr = true;
                hasConsentForDataUsage = true;
                hasConsentForAdsPersonalization = 
                    consentData.TryGetValue(ConsentType.AdPersonalization, out var adPersonalization)
                        ? adPersonalization == ConsentStatus.Granted
                        : null;
            }
            else
            {
                isUserSubjectToGdpr = false;
                hasConsentForDataUsage = true;
                hasConsentForAdsPersonalization = true;
            }

            var appsFlyerConsent = new AppsFlyerConsent(isUserSubjectToGdpr, hasConsentForDataUsage, hasConsentForAdsPersonalization);
            AppsFlyer.setConsentData(appsFlyerConsent);
#endif
        }
        
        private static void ReportResult(string purposeStr, string result)
        {
            if (!string.IsNullOrEmpty(DmaResult) && DmaResult == result)
            {
                // result nochange will not report the event;
                return;
            }

            DmaResult = result;
            //----------- Guru Analytics report ---------------
            Analytics.TrackEvent(new DmaEvent(purposeStr, result));
        }


        /// <summary>
        /// using Guru map rules to generate the result
        /// </summary>
        /// <param name="purposes"></param>
        /// <returns></returns>
        private static string ApplyGuruMapRule(bool[] purposes)
        {
            // purpose 1, 3, 4 => guru analytics granted
            //---------------- Guru Rules ---------------------
            if (purposes[0] && purposes[2] && purposes[3])
            {
                return "1111";
            }
            return "0000";
        }

        /// <summary>
        /// using Google map rules to generate the result
        /// </summary>
        /// <param name="consentData"></param>
        /// <returns></returns>
        private static string ApplyGoogleMapRule(Dictionary<ConsentType, ConsentStatus> consentData)
        {
            string result = "";
            result += consentData[ConsentType.AdStorage] == ConsentStatus.Granted ? "1" : "0";
            result += consentData[ConsentType.AnalyticsStorage] == ConsentStatus.Granted ? "1" : "0";
            result += consentData[ConsentType.AdPersonalization] == ConsentStatus.Granted ? "1" : "0";
            result += consentData[ConsentType.AdUserData] == ConsentStatus.Granted ? "1" : "0";
            return result;
        }


        private static Dictionary<ConsentType, ConsentStatus> ApplyPurposesRules(bool[] purposes, string mapRules = "")
        {
            if (string.IsNullOrEmpty(mapRules)) mapRules = DefaultMapRules;
            
            
            Dictionary<ConsentType, ConsentStatus> consentData = new Dictionary<ConsentType, ConsentStatus>()
            {
                { ConsentType.AdStorage, ConsentStatus.Denied },
                { ConsentType.AnalyticsStorage, ConsentStatus.Denied },
                { ConsentType.AdPersonalization, ConsentStatus.Denied },
                { ConsentType.AdUserData, ConsentStatus.Denied },
            };

            string[] map = mapRules.Replace(" ", "").Split(',');
            if (map.Length != 4) return consentData;
            

            for (int i = 0; i < map.Length; i++)
            {
                ConsentType type = ConsentType.AdStorage;
                switch (i)
                {
                    default: type = ConsentType.AdStorage; break;
                    case 1: type = ConsentType.AnalyticsStorage; break;
                    case 2: type = ConsentType.AdPersonalization; break;
                    case 3: type = ConsentType.AdUserData; break;
                }

                bool granted = true;
                if (string.IsNullOrEmpty(map[i]) || map[i] == "0")
                {
                    // null pass directly.
                }
                else
                {
                    // parse all the single rule   
                    int[] rule = Array.ConvertAll(map[i].Split('&'), s =>
                    {
                        if(string.IsNullOrEmpty(s) || s == "0") return 0;
                        int.TryParse(s, out var v);
                        return v;
                    });
                    // for each bit to judge the result of granted
                    for (int j = 0; j < rule.Length; j++)
                    {
                        var idx = rule[j] - 1; // 1 -> 0, convert id to index;
                        if (idx < 0 || idx >= purposes.Length || !purposes[idx])
                        {
                            granted = false;
                            break;
                        }
                    }
                }
                consentData[type] = granted ? ConsentStatus.Granted : ConsentStatus.Denied;
            }
            
            return consentData;
        }


        private static bool IsUserInEEACountry(string value, bool enableCountryCheck = true)
        {
            //#1. Empty string
            if(string.IsNullOrEmpty(value)) return false;
            
            //#2. string not "0" or "1".
            var match = Regex.Match(value, "^[01]+$");
            if(string.IsNullOrEmpty(match.Value)) return false;

#if UNITY_IOS 
            //#3. country code is not in list
            Debug.Log($"{Tag} --- regionCode: [{RegionCode}]");
            if(enableCountryCheck && IsUserNotInEEACountry()) return false;
#endif
            
            return true;
        }
        

#if UNITY_IOS
        private static bool IsUserNotInEEACountry()
        {
            return !EEARegionCodes.Contains(RegionCode.ToLower());
        }
        
        private static string RegionCode => ConsentAgentIOS.GetRegionCode().ToLower();
#endif
    }
}