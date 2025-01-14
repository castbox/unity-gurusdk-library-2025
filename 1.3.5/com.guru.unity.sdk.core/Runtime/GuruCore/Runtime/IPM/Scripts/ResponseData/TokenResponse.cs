using System;

namespace Guru
{
	[Serializable]
	public class TokenResponse
	{
		/// <summary>
		/// 用户ID，APP内唯一
		/// </summary>
		public string uid; 
		
		/// <summary>
		/// 权token，用于访问API
		/// </summary>
		public string token;
		
		/// <summary>
		/// 代表是否是新用户，用户第一次访问会生成新用户
		/// </summary>
		public bool newUser;

		/// <summary>
		/// 用户ID（整型），与uid一一对应，APP内唯一
		/// </summary>
		public long uidInt;
		
		/// <summary>
		/// Firebase token，用于firebase的登录，可以使用firebase sdk的signInWithCustomToken函数登录。
		/// 为了保证firebase的uid和中台登录系统的用户统一。
		/// </summary>
		public string firebaseToken;

		/// <summary>
		/// 用户创建时间
		/// 请各个项目组把该属性在客户端打点时放到user_properties里，
		/// 属性的打点的属性key是user_created_timestamp（单位是微秒，透传即可）
		/// BI后续会用这个属性做用户方面的相关数据分析和统计。
		/// </summary>
		public long createdAtTimestamp;

		public override string ToString()
		{
			return $"{nameof(uid)}: {uid}, {nameof(token)}: {token}, {nameof(newUser)}: {newUser}, {nameof(uidInt)}: {uidInt}, {nameof(firebaseToken)}: {firebaseToken}, {nameof(createdAtTimestamp)}: {createdAtTimestamp}";
		}
	}
}