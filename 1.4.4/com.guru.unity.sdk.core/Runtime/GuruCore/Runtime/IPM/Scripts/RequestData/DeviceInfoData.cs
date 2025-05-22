namespace Guru
{
    public class DeviceInfoData
    {
        public string appIdentifier;//APP包名，如android的packageName或者ios的bundleId
        public string appVersion;	//APP版本，如1.0.0
        public string deviceType;	//必填，具体内容：Android设备：android	IOS设备：iOS/iPhone/iPad/iPod touch其中一种
        public string deviceCountry;//国家编码，大写，手机的系统国家，参考ISO 3166-1 alpha-2
        public string appCountry;	//国家编码，大写，如果APP内可以设置国家信息则使用该属性，参考ISO 3166-1 alpha-2
        public string locale;		//locale编码，如en-US
        public string language;		//语言编码，如en
        public string timezone;		//必填，时区，用于按照当地时间发推送消息，如America/Chicago
        public string brand;		//手机品牌
        public string model;		//手机品牌下的手机型号
        public string androidId;	//android设备有效
        
        public DeviceInfoData()
        {
            DeviceUtil.GetDeviceInfo();
            appIdentifier = IPMConfig.IPM_APP_PACKAGE_NAME;
            appVersion = IPMConfig.IPM_APP_VERSION;
            deviceType = IPMConfig.GetDeviceType();
            deviceCountry = IPMConfig.IPM_COUNTRY_CODE;
            appCountry = IPMConfig.IPM_COUNTRY_CODE;
            locale = IPMConfig.IPM_LOCALE;
            language = IPMConfig.IPM_LANGUAGE;
            timezone = IPMConfig.IPM_TIMEZONE;
            brand = IPMConfig.IPM_BRAND;
            model = IPMConfig.IPM_MODEL;
            androidId = IPMConfig.ANDROID_ID;
        }

        public override string ToString()
        {
            string value = $"{nameof(appIdentifier)}={appIdentifier};{nameof(appVersion)}={appVersion};{nameof(deviceType)}={deviceType};{nameof(deviceCountry)}={deviceCountry};{nameof(appCountry)}={appCountry};{nameof(locale)}={locale};{nameof(language)}={language};{nameof(timezone)}={timezone};{nameof(brand)}={brand};{nameof(model)}={model};{nameof(androidId)}={androidId}";
            this.Log($"IPM.Auth.User DeviceInfo Header:[{value}]");
            return value;
        }
    }
}