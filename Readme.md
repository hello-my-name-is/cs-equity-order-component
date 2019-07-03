# Credit Suisse - Equity Order Component

## Requirements:
1. Build a concrete implementation of the IEquityOrder
2. It will receive all ticks (price updates for equities) from an external tick source via the ReceiveTick method
3. When a (relevant) tick is received whose price is below a threshold level, the component should then:
  1. Place a buy order via the IOrderService interface
  2. Signal the Order Placed Event Handler
  3. Shut down - ignoring all further ticks
4. Any errors experienced should cause the component to signal the Order Errored Event Handler, and then shut down - ignoring all further ticks
5. Each instance of your component should only ever place one order. There may be several instances active simultaneously

## Notes
As per the brief I aimed to keep things simple.  The code and unit tests should hopefully be sufficiently self-descriptive.  
In regard to requirement 5 I have taken this to mean that a single order is represented by a single instance of the EquityOrder class
and that all other instances represent different orders.
