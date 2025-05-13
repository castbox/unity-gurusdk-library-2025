namespace Guru
{
    public interface IConsentAgent
    {
        void Init(string objectName, string callbackName);
        void RequestGDPR(string deviceId = "", int debugGeography = -1);
        string GetPurposesValue();
    }
}