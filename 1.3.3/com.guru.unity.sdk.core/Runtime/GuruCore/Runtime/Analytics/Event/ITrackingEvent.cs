
namespace Guru
{
    using System.Collections.Generic;

    
    public interface ITrackingEvent
    {
        string EventName { get; }
        Dictionary<string, object> Data { get; }
        EventSetting Setting { get; } 
        EventPriority Priority { get; }
    }

    // ------ Facebook Event ------
    public interface IFBEvent
    {
        float Value { get; }
    }

    public interface IAdImpressionEvent
    {

    }


    public interface IAdPaidEvent
    {
        
    }

    /// <summary>
    /// FB货币消耗事件
    /// </summary>
    public interface IFBSpentCreditsEvent: IFBEvent
    {
        string ContentID { get; }
        string ContentType { get; }
    }
    
    public interface IFBPurchaseEvent: IFBEvent
    {
        string Currency { get; }
    }



    // ------ Adjust Event --------
    public interface IAdjustAdImpressionEvent
    {
        AdjustAdImpressionEvent ToAdjustAdImpressionEvent();
    }
    public interface IAdjustIapEvent
    {
        AdjustIapEvent ToAdjustIapEvent();
    }
}