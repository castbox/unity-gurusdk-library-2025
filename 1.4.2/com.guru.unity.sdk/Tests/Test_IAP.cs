




namespace Guru.Tests
{
    using UnityEditor;
    using NUnit.Framework;
    using UnityEngine;
    using System;
    using Guru.IAP;
    
    public class Test_IAP
    {

        [Test]
        public void Test__AppleOrders()
        {
            var model = IAPModel.Load();
            int level = 1;
            int orderType = 0;
            for (int i = 0; i < 5; i++)
            {
                // model.AddAppleOrder(new AppleOrderData(orderType, 
                //     $"i.iap.test.icon_{i}", 
                //     $"receipt_{i}", 
                //     $"order_id_{i}", 
                //     DateTime.Now.ToString("g"), 
                //     level, 
                //     "RMB", 6.99d, "Store"));
                
                level++;
            }
            
            if (model.HasUnreportedAppleOrder)
            {
                int i = 0;
                while (model.appleOrders.Count > 0 
                       && i < model.appleOrders.Count)
                {
                    var o = model.appleOrders[i];
                    model.RemoveAppleOrder(o);
                    i++;
                }
            }
        }



    }
}