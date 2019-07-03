using System;

namespace EquityOrderComponent
{
    public class EquityOrder : IEquityOrder
    {
        private readonly object tickProcessingLock = new object();
        private readonly IOrderService orderService;
        private readonly string equityCode;
        private readonly decimal priceThreshhold;
        private readonly int quantity;
        private bool orderIsActive = true;

        public event OrderPlacedEventHandler OrderPlaced;
        public event OrderErroredEventHandler OrderErrored;

        public EquityOrder(IOrderService orderService, string equityCode, decimal priceThreshhold, int quantity)
        {
            this.orderService = orderService;
            this.equityCode = equityCode;
            this.priceThreshhold = priceThreshhold;
            this.quantity = quantity;
        }

        public void ReceiveTick(string equityCode, decimal price)
        {
            if (IsRelevantEquityCode(equityCode) && IsBelowOrderThreshold(price))
            {
                lock (tickProcessingLock)
                {
                    if (orderIsActive)
                    {
                        try
                        {
                            orderService.Buy(equityCode, quantity, price);
                            OnOrderPlaced(new OrderPlacedEventArgs(equityCode, price));
                        }
                        catch (Exception ex)
                        {
                            OnErrored(new OrderErroredEventArgs(equityCode, price, ex));
                        }
                        finally
                        {
                            orderIsActive = false;
                        }
                    }
                }
            }
        }

        private bool IsRelevantEquityCode(string equityCode)
        {
            return this.equityCode == equityCode;
        }

        private bool IsBelowOrderThreshold(decimal price)
        {
            return price < this.priceThreshhold;
        }

        private void OnOrderPlaced(OrderPlacedEventArgs e)
        {
            OrderPlacedEventHandler handler = OrderPlaced;
            handler?.Invoke(e);
        }

        private void OnErrored(OrderErroredEventArgs e)
        {
            OrderErroredEventHandler handler = OrderErrored;
            handler?.Invoke(e);
        }
    }
}
