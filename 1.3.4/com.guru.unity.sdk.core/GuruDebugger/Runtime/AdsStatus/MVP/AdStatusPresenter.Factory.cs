namespace Guru
{
    using System;
    public partial class AdStatusPresenter
    {
        private static AdStatusInfo CreateLoadingInfo(string adUnitId, AdType adType)
        {
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = AdStatusType.Loading,
                date = DateTime.Now,
            };
        }

        private static AdStatusInfo CreateLoadedInfo(string adUnitId, AdType adType, string network, string waterfall)
        {
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = AdStatusType.Loaded,
                date = DateTime.Now,
                network = network,
                waterfall = waterfall
            };
        }

        private static AdStatusInfo CreateFailInfo(string adUnitId, AdType adType, string waterfall, int errorCode = -1)
        {
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = AdStatusType.Failed,
                date = DateTime.Now,
                errorCode = errorCode,
                waterfall = waterfall
            };
        }


        private static AdStatusInfo CreateClosedInfo(string adUnitId, AdType adType)
        {
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = AdStatusType.Closed,
                date = DateTime.Now,
            };
        }


        private static AdStatusInfo CreatePaidInfo(string adUnitId, AdType adType, double revenue, string network, string networkPlacement)
        {
            if (string.IsNullOrEmpty(network)) network = "unknown";
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = AdStatusType.Paid,
                date = DateTime.Now,
                revenue = revenue,
                network = network,
                networkPlacement = networkPlacement
            };
        }


        private static AdStatusInfo CreateClickedInfo(string adUnitId, AdType adType, string placement)
        {
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = AdStatusType.Clicked,
                date = DateTime.Now,
                placement = placement,
            };
        }
        
    }
}