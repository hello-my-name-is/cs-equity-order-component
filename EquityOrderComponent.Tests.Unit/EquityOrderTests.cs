using EquityOrderComponent;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    public class EquityOrderTests
    {
        const string relevantEquityCode = "CS";
        const decimal orderThreshhold = 10m;
        const decimal priceBelowThreshhold = orderThreshhold - 0.01m;
        const int quantity = 5;
        EquityOrder equityOrder;
        Mock<IOrderService> orderServiceMock;

        [SetUp]
        public void Setup()
        {
            orderServiceMock = new Mock<IOrderService>();
            equityOrder = new EquityOrder(orderServiceMock.Object, relevantEquityCode, orderThreshhold, quantity);
        }

        [Test]
        public void ReceiveTickShouldPlaceOrderForRelevantTickAndPrice()
        {
            equityOrder.ReceiveTick(relevantEquityCode, priceBelowThreshhold);

            orderServiceMock.Verify(x => x.Buy(relevantEquityCode, quantity, priceBelowThreshhold));
            orderServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void ReceiveTickShouldNotPlaceOrderForIrrelevantTick()
        {
            equityOrder.ReceiveTick("NOTCS", priceBelowThreshhold);

            orderServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(10)]
        [TestCase(10.01)]
        public void ReceiveTickShouldNotPlaceOrderIfPriceIsNotBelowThreshold(decimal price)
        {
            equityOrder.ReceiveTick(relevantEquityCode, price);

            orderServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void ReceiveTickShouldRaiseOrderPlacedEventWhenOrderIsPlaced()
        {
            var eventFired = false;
            equityOrder.OrderPlaced += (e) =>
            {
                eventFired = true;
                e.EquityCode.Should().Be(relevantEquityCode);
                e.Price.Should().Be(priceBelowThreshhold);
            };

            equityOrder.ReceiveTick(relevantEquityCode, priceBelowThreshhold);

            eventFired.Should().BeTrue();
        }

        [Test]
        public void ReceiveTickShouldNotRaiseErrorEventWhenOrderIsSuccessful()
        {
            var eventFired = false;
            equityOrder.OrderErrored += (e) => eventFired = true;

            equityOrder.ReceiveTick(relevantEquityCode, priceBelowThreshhold);

            eventFired.Should().BeFalse();
        }

        [Test]
        public void ReceiveTickShouldRaiseErrorEventWhenExceptionIsThrown()
        {
            var exception = new Exception();
            var eventFired = false;
            equityOrder.OrderErrored += (e) =>
            {
                eventFired = true;
                e.EquityCode.Should().Be(relevantEquityCode);
                e.Price.Should().Be(priceBelowThreshhold);
                e.GetException().Should().Be(exception);
            };

            orderServiceMock.Setup(m => m.Buy(relevantEquityCode, quantity, priceBelowThreshhold)).Throws(exception);

            equityOrder.ReceiveTick(relevantEquityCode, priceBelowThreshhold);

            eventFired.Should().BeTrue();
        }

        [Test]
        public void ReceiveTickShouldOnlyProcessOneOrder()
        {
            equityOrder.ReceiveTick(relevantEquityCode, priceBelowThreshhold);
            orderServiceMock.Verify(x => x.Buy(relevantEquityCode, quantity, priceBelowThreshhold));
            equityOrder.ReceiveTick(relevantEquityCode, priceBelowThreshhold);
            orderServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void ReceiveTickShouldStopProcessingAfterError()
        {
            var exception = new Exception();
            orderServiceMock.Setup(m => m.Buy(relevantEquityCode, quantity, priceBelowThreshhold)).Throws(exception);
            equityOrder.ReceiveTick(relevantEquityCode, priceBelowThreshhold);
            equityOrder.ReceiveTick(relevantEquityCode, priceBelowThreshhold);

            orderServiceMock.Verify(m => m.Buy(relevantEquityCode, quantity, priceBelowThreshhold), Times.Once);
        }

        [Test]
        public void ReceiveTickShouldOnlyProcessOneOrderFromConcurrentCalls()
        {
            equityOrder.OrderPlaced += (e) =>
            {
                Console.WriteLine($"EquityCode: {e.EquityCode}, Price: {e.Price}");
            };

            for (int i = 0; i < 20; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    var price = orderThreshhold - (decimal)Math.Round(new Random().NextDouble(), 2);
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} sending tick for {price} start");
                    equityOrder.ReceiveTick(relevantEquityCode, price);
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} sending tick for {price} finish");
                });
            }

            orderServiceMock.Verify(m => m.Buy(relevantEquityCode, quantity, It.IsAny<decimal>()), Times.Once);
        }
    }
}