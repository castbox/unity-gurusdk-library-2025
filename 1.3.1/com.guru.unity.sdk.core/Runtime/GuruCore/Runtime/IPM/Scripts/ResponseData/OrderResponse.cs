using System;

namespace Guru
{
    [Serializable]
    public class OrderResponse
    {
        public double usdPrice;
        public bool test;
        public string state;
        
        public override string ToString()
        {
            return $"{nameof(usdPrice)}: {usdPrice}  {nameof(test)}: {test}  {nameof(state)}: {state}";
        }
    }
}