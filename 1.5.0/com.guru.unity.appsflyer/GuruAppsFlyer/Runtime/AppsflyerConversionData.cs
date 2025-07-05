#nullable enable
using System;
using System.Collections.Generic;
using AppsFlyerSDK;
using Newtonsoft.Json;

namespace Guru
{
    // https://support.appsflyer.com/hc/en-us/articles/360000726098-Conversion-data-payloads-and-scenarios
    public class AppsflyerConversionData
    {
        internal readonly string RawConversionData;
        private readonly Dictionary<string, object> _conversionData;

        public bool IsValid => !string.IsNullOrEmpty(RawConversionData);

        public AppsflyerConversionData(string? conversionData = null)
        {
            if (string.IsNullOrEmpty(conversionData))
            {
                RawConversionData = "";
                _conversionData = new Dictionary<string, object>();
            }
            else
            {
                RawConversionData = conversionData!;
                var conversionDataDictionary = new Dictionary<string, object>();
                try
                {
                    conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData) ??
                                               new Dictionary<string, object>();
                }
                catch (Exception exception)
                {
                    Log.W($"Failed to parse conversion data: {exception.Message}");
                    Foundation.SafeRun(() =>
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(RawConversionData);
                        if (result != null)
                        {
                            conversionDataDictionary = result;
                        }
                    }, ex => Log.E($"Fallback JSON parsing also failed: {ex.Message}"));
                }

                _conversionData = conversionDataDictionary;
            }
        }

        /// <summary>
        /// Check if conversion data indicates organic install
        /// </summary>
        public bool IsOrganicInstall()
        {
            if (_conversionData.Count == 0) return true;

            if (_conversionData.TryGetValue("af_status", out var status))
            {
                return status?.ToString() == "Organic";
            }

            return true;
        }

        /// <summary>
        /// Get media source from conversion data
        /// </summary>
        public string? GetMediaSource()
        {
            if (_conversionData.Count == 0) return null;

            _conversionData.TryGetValue("media_source", out var mediaSource);
            return mediaSource?.ToString();
        }

        /// <summary>
        /// Get campaign name from conversion data
        /// </summary>
        public string? GetCampaign()
        {
            if (_conversionData.Count == 0) return null;

            _conversionData.TryGetValue("campaign", out var campaign);
            return campaign?.ToString();
        }

        /// <summary>
        /// Get install time from conversion data
        /// </summary>
        public DateTime? GetInstallTime()
        {
            if (_conversionData.Count == 0) return null;

            if (!_conversionData.TryGetValue("install_time", out var installTime)) return null;
            if (DateTime.TryParse(installTime?.ToString(), out var dateTime))
            {
                return dateTime;
            }

            return null;
        }

        public void Dump()
        {
            Log.I("AppsFlyer", $"[APPSFLYER] Raw conversion data: {RawConversionData}");
            // Log all parameters for debugging
            foreach (var kvp in _conversionData)
            {
                Log.I($"  - {kvp.Key}: {kvp.Value}");
            }
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AppsflyerConversionData)obj);
        }

        protected bool Equals(AppsflyerConversionData other)
        {
            return RawConversionData == other.RawConversionData;
        }

        public override int GetHashCode()
        {
            return RawConversionData?.GetHashCode() ?? 0;
        }
    }
}