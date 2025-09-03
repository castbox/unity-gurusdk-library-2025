namespace Guru
{
	using System;
	using UnityEngine;
	
	/// <summary>
	/// 设备数据
	/// </summary>
	
	[Serializable]
	public class DeviceData
	{
		private const string PUSH_TYPE_FCM = "FCM";
		public bool pushNotificationEnable = true;	//必填，默认true，发送消息的总开关

		public string deviceId;		//必填，设备唯一ID
		public string uid;			//必填，用户唯一ID，授权结果返回的uid
		public string androidId;	//android设备有效
		public string appCountry;	//国家编码，大写，如果APP内可以设置国家信息则使用该属性，参考ISO 3166-1 alpha-2
		public string deviceCountry;//国家编码，大写，手机的系统国家，参考ISO 3166-1 alpha-2
		public string language;		//语言编码，如en
		public string locale;		//locale编码，如en-US
		public string deviceToken;  //必填，设备发送消息的token，如google firebase的token
		public string deviceType;	//必填，具体内容：Android设备：android	IOS设备：iOS/iPhone/iPad/iPod touch其中一种
		public string pushType;		//必填，目前仅支持：FCM
		public string appIdentifier;//APP包名，如android的packageName或者ios的bundleId
		public string appVersion;	//APP版本，如1.0.0
		public string brand;		//手机品牌
		public string model;		//手机品牌下的手机型号
		public string timezone;		//必填，时区，用于按照当地时间发推送消息，如America/Chicago
		public string firebaseAppInstanceId; // 可选, firebase应用实例id
		public string idfa; // 可选, ios广告id 
		public string idfv; // 可选, ios广告id 
		public string adid; // 可选, adjust id 
		public string gpsAdid; // 可选, android广告id
		public string userUuid; // 可选, 用户唯一ID，由 uid 关联生成的 uuid
		public string appsflyer_id;
		
		public DeviceData(bool pushServiceEnable = true)
		{
			DeviceUtil.GetDeviceInfo();
			if (string.IsNullOrEmpty(IPMConfig.IPM_UUID))
			{
				IPMConfig.IPM_UUID = IDHelper.GenUUID(IPMConfig.IPM_UID);
			}
			pushNotificationEnable = pushServiceEnable;
			deviceId = IPMConfig.IPM_DEVICE_ID;
			uid = IPMConfig.IPM_UID;
			androidId = SystemInfo.deviceUniqueIdentifier; // Unity get AndroidID
			appCountry = IPMConfig.IPM_COUNTRY_CODE;
			deviceCountry = IPMConfig.IPM_COUNTRY_CODE;
			language = IPMConfig.IPM_LANGUAGE;
			locale = IPMConfig.IPM_LOCALE;
			deviceToken = IPMConfig.FIREBASE_PUSH_TOKEN;
			deviceType = IPMConfig.GetDeviceType();
			pushType = PUSH_TYPE_FCM;
			appIdentifier = IPMConfig.IPM_APP_PACKAGE_NAME;
			appVersion = IPMConfig.IPM_APP_VERSION;
			brand = IPMConfig.IPM_BRAND;
			model = IPMConfig.IPM_MODEL;
			timezone = IPMConfig.IPM_TIMEZONE;
			firebaseAppInstanceId = IPMConfig.FIREBASE_ID;
			idfa = IPMConfig.IDFA;
			idfv = IPMConfig.IDFV;
			adid = IPMConfig.ADJUST_DEVICE_ID;
			gpsAdid = IPMConfig.GOOGLE_ADID;
			userUuid = IPMConfig.IPM_UUID;
			appsflyer_id = IPMConfig.APPSFLYER_ID;
		}

		public override string ToString()
		{
			return $"{nameof(deviceId)}: {deviceId}, {nameof(uid)}: {uid}, " +
			       $"{nameof(androidId)}: {androidId}, {nameof(appCountry)}: {appCountry}, {nameof(deviceCountry)}: {deviceCountry}, " +
			       $"{nameof(language)}: {language}, {nameof(locale)}: {locale}, {nameof(deviceToken)}: {deviceToken}, {nameof(deviceType)}: {deviceType}, " +
			       $"{nameof(pushType)}: {pushType}, {nameof(appIdentifier)}: {appIdentifier}, {nameof(appVersion)}: {appVersion}, {nameof(brand)}: {brand}, " +
			       $"{nameof(model)}: {model}, {nameof(timezone)}: {timezone}, {nameof(pushNotificationEnable)}: {pushNotificationEnable}, " +
			       $"{nameof(firebaseAppInstanceId)}: {firebaseAppInstanceId}, {nameof(idfa)}: {idfa}, {nameof(idfv)}: {idfv}, {nameof(adid)}: {adid}, {nameof(gpsAdid)}: {gpsAdid}, {nameof(userUuid)}: {userUuid},{nameof(appsflyer_id)}: {appsflyer_id}";
		}

	}
}