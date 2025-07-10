using System;

namespace Guru.IAP
{
    public interface IGuruIapService
    {
        string CurrentBuyingProductId { get; }

        void SetUID(string uid);
        void SetUUID(string uuid);
        void ClearData();

        // 添加回调
        void AddInitResultAction(Action<bool> onInitResult);
        void AddRestoredAction(Action<bool, string> onRestored);
        void AddPurchaseStartAction(Action<string> onBuyStart);
        void AddPurchaseEndAction(Action<string, bool> onBuyEnd);
        void AddPurchaseFailedAction(Action<string, string> onBuyFailed);
        void AddGetProductReceiptAction(Action<string, string, bool> onInitResult);

        void Restore();
        
        void Initialize(IGuruIapDataProvider provider);

        ProductInfo GetInfo(string productName);
        ProductInfo GetInfoById(string productId);
        bool IsProductHasReceipt(string productName);
        string GetLocalizedPriceString(string productName);
        ProductInfo[] GetAllProductInfos();
        void Purchase(string productName, string category);
        bool IsSubscriptionCancelled(string productName);
        bool IsSubscriptionAvailable(string productName);
        bool IsSubscriptionExpired(string productName);
        bool IsSubscriptionFreeTrail(string productName);
        bool IsSubscriptionAutoRenewing(string productName);
        bool IsSubscriptionIntroductoryPricePeriod(string productName);
        DateTime GetSubscriptionExpireDate(string productName);
        DateTime GetSubscriptionPurchaseDate(string productName);
        DateTime GetSubscriptionCancelDate(string productName);
        TimeSpan GetSubscriptionRemainingTime(string productName);
        TimeSpan GetSubscriptionIntroductoryPricePeriod(string productName);
        TimeSpan GetSubscriptionFreeTrialPeriod(string productName);
        string GetSubscriptionInfoJsonString(string productName);
    }
}