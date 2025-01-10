namespace Guru
{
    using System;
    
    /// <summary>
    /// Guru SDK未初始化报错
    /// </summary>
    public class GuruNotInitializedException: Exception
    {
        public override string Message => "SDK not initialized! Please call <GuruSDK.Init> first, before you call any other API in GuruSDK.";
    }
}