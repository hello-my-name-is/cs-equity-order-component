namespace EquityOrderComponent
{
    public class OrderPlacedEventArgs
    {
        public OrderPlacedEventArgs(string equityCode, decimal price)
        {
            EquityCode = equityCode; Price = price;
        }

        public string EquityCode { get; }

        public decimal Price { get; }
    }
}

