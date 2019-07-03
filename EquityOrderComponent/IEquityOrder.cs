namespace EquityOrderComponent
{
    public interface IEquityOrder : IOrderPlaced, IOrderErrored
    {
        void ReceiveTick(string equityCode, decimal price);
    }
}
