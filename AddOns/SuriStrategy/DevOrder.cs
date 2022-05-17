/*using System;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class SuriOrder {
        public Order order;
        public readonly string signalName;
        public readonly OrderAction orderAction;
        public Order stopOrder;
        public static Strategy strategy;

        public SuriOrder(string signalName, OrderAction orderAction, OrderType orderType, double limitPrice = 0, double stopPrice = 0) {
            if (orderAction == OrderAction.SellShort || orderAction == OrderAction.BuyToCover) throw new Exception("Don't create a new Order with " + orderAction);
            if (signalName == null) {
                
            }
            this.signalName = signalName;
            this.orderAction = orderAction;
            order = strategy.SubmitOrderUnmanaged(0, orderAction, orderType, 1, limitPrice, stopPrice, null, signalName);
        }

        public void Cancel() {
            strategy.CancelOrder(order);
        }
        public void SetStopLoss(double stopPrice) {
            stopOrder = strategy.SubmitOrderUnmanaged(0, ContraAction(), OrderType.StopMarket, 1, stopPrice, stopPrice, null, "Stop Loss " + signalName);
        }
        public void Exit() {
            if (order.OrderState == OrderState.Filled && stopOrder.OrderState != OrderState.Filled) {
                strategy.SubmitOrderUnmanaged(0, ContraAction(), OrderType.Market, 1, 0, 0, null, "Exit " + signalName);
            } else {
                strategy.CancelOrder(order);
            }
            strategy.CancelOrder(stopOrder);
            order = null;
            stopOrder = null;
        }
        private OrderAction ContraAction() {
            return orderAction == OrderAction.Buy ? OrderAction.SellShort : OrderAction.BuyToCover;
        }

        public bool IsLong() {
            return orderAction == OrderAction.Buy;
        }
        public bool IsShort() {
            return orderAction == OrderAction.Sell;
        }

        // Returns true if the entry limit order is reached tomorrow
        public bool ReachesLimitTomorrow(Bars bars, int currentBar, double entry) {
            return IsLong() && bars.GetHigh(currentBar + 1) >= entry - 0.00000000001 || IsShort() && bars.GetLow(currentBar + 1) <= entry + 0.00000000001;
        }
        
    }
}
*/