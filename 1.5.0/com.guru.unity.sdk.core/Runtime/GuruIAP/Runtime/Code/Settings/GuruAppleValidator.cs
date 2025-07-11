using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Purchasing.Security;

namespace Guru
{
    public class GuruAppleValidator
    {


        private readonly AppleValidator _appleValidator;



        public GuruAppleValidator(byte[] appleRootCert)
        {
            _appleValidator = new AppleValidator(appleRootCert);
        }


        /// <summary>
        /// 验单
        /// </summary>
        /// <param name="unityIAPReceipt"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="IAPSecurityException"></exception>
        public IPurchaseReceipt[] Validate(string unityIAPReceipt)
        {
            try
            {
                var wrapper = JsonConvert.DeserializeObject<Dictionary<string, object>>(unityIAPReceipt);
                if (null == wrapper)
                {
                    throw new Exception();
                }

                var store = (string)wrapper["Store"];
                var payload = (string)wrapper["Payload"];

                switch (store)
                {
                    case "AppleAppStore":
                    case "MacAppStore":
                    {
                        if (null == _appleValidator)
                        {
                            throw new Exception(
                                "Cannot validate an Apple receipt without supplying an Apple root certificate");
                        }
                        var r = _appleValidator.Validate(Convert.FromBase64String(payload));
                        if (!Application.identifier.Equals(r.bundleID))
                        {
                            throw new Exception();
                        }
                        return r.inAppPurchaseReceipts.ToArray();
                    }
                    default:
                    {
                        throw new Exception("Store not supported: " + store);
                    }
                }
            }
            catch (IAPSecurityException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot validate due to unhandled exception. (" + ex + ")");
            }
        }

        



    }
}