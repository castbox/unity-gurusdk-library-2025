namespace Guru.IAP
{
    public interface IGuruIapDataProvider
    {
        // Apple Bundle ID
        string AppBundleId { get; }
        
        // Certs
        byte[] GooglePublicKeys { get; }
        byte[] AppleRootCerts { get; }

        string IDFV { get; }
        string UID { get; }
        string UUID { get; }

        ProductSetting[] ProductSettings { get; }

        public int BLevel { get; }
        
        public bool IsDebug { get; }
        
    }
}