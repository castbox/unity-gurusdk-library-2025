namespace Guru
{
    using Guru.IAP;
    
    internal class GuruIAP: IAPServiceBaseV4<GuruIAP>
    {
        
        /// <summary>
        /// 获取BLevel
        /// </summary>
        /// <returns></returns>
        protected override int GetBLevel() => Model.BLevel; // BLevel

        private GuruSDKModel Model => GuruSDKModel.Instance;
        
        protected override void OnPurchaseOver(bool success, string productName)
        {
        }

        public void ClearData()
        {
            _model.ClearData();
        }
    }
}