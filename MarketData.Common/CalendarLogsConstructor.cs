using ModelLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketData.Common
{
   public class CalendarLogsCreator
   {
      IEnumerable<HistoricalOrderDto> _historicalOrders;

      public CalendarLogsCreator(IEnumerable<HistoricalOrderDto> historicalOrders)
      {
         _historicalOrders = historicalOrders;
      }

      public List<CalendarLog> SetupCalendarLogs(IEnumerable<OrderLog> orderLogs, SetupInfo setupInfo)
      {
         List<CalendarLog> calendarLogs = PrepareCalendarLogsWithOperationDays(setupInfo.StartDate, setupInfo.EndDate);

         SetupCommissionSum(calendarLogs, orderLogs);
         SetupTradesOpenClosed(calendarLogs);
         CalculateAvarageCommissions(calendarLogs, setupInfo);
         SetupEodResult(calendarLogs, setupInfo.StartDate, setupInfo.InitialDeposit, _historicalOrders);

         return calendarLogs;
      }

      public List<CalendarLog> SetupSlicedCalendarLogs(DateTime startDate, DateTime endDate, double initialDeposit)
      {
         List<CalendarLog> calendarLogs = PrepareCalendarLogsWithOperationDays(startDate, endDate);
         SetupEodResult(calendarLogs, startDate, initialDeposit, _historicalOrders);
         
         return calendarLogs;
      }

      private void CalculateAvarageCommissions(List<CalendarLog> calendarLogs, SetupInfo setupInfo)
      {
         //AvgCmsn = сумма_комиссий / (сделок_открыто + сделок_закрыто)
         foreach (var calendarLog in calendarLogs)
         {
            int tradesTotal = calendarLog.TradesOpened.Count() + calendarLog.TradesClosed.Count();
            if (calendarLog.SumCmsn != 0 && tradesTotal != 0)
            {
               calendarLog.AvgCmsn = calendarLog.SumCmsn / tradesTotal;
            }
         }
      }

      private void SetupTradesOpenClosed(List<CalendarLog> calendarLogs)
      {
         //TradesOpened\TradesClosed set
         foreach (var calendarLog in calendarLogs)
         {
            foreach (var currentOpenedOrder in _historicalOrders.Where(order => DateTime.Compare(
                                                               order.AdjustedOpenDate.Date, calendarLog.OperationDay.Date) == 0))
            {
               calendarLog.TradesOpened.Add(currentOpenedOrder.Label);
            }

            foreach (var currentClosedOrder in _historicalOrders.Where(order => DateTime.Compare(
                                                                        order.AdjustedCloseDate.Date, calendarLog.OperationDay.Date) == 0))
            {
               calendarLog.TradesClosed.Add(currentClosedOrder.Label);
            }
         }
      }

      private List<CalendarLog> PrepareCalendarLogsWithOperationDays(DateTime startDate, DateTime endDate)
      {
         var calendarLogs = new List<CalendarLog>();
         DateTime currentdate = startDate.AddDays(-1);

         //preparing calendarLogs skeleton 
         while (DateTime.Compare(currentdate, endDate) <= 0)
         {
            var calendarLog = new CalendarLog()
            {
               OperationDay = currentdate
            };

            calendarLogs.Add(calendarLog);
            currentdate = currentdate.AddDays(1);
         }
         return calendarLogs;
      }

      private void SetupCommissionSum(List<CalendarLog> calendarLogs, IEnumerable<OrderLog> orderLogs)
      {
         foreach (var calendarLog in calendarLogs)
         {
            var logWithCommission = orderLogs.FirstOrDefault(log => DateTime.Compare(
                                                             log.OperationDay.Date, calendarLog.OperationDay.Date) == 0);
            if (logWithCommission != null)
            {
               calendarLog.SumCmsn = logWithCommission.Comisions.Sum();
            }
         }
      }

      private void SetupEodResult(List<CalendarLog> calendarLogs, DateTime startDate, double initialDeposit, IEnumerable<HistoricalOrderDto> historicalOrders)
      {
         double currentEodResult = initialDeposit;
         foreach (var calendarLog in calendarLogs)
         {
            if (DateTime.Compare(calendarLog.OperationDay, startDate) < 0)
            {
               calendarLog.EodFinRes = currentEodResult;
               continue;
            }
            //checking 
            var pickedOrders = historicalOrders.Where(currentOrder => DateTime.Compare(currentOrder.CloseDate.Date, calendarLog.OperationDay.Date) == 0);
            if (!pickedOrders.Any())
            {
               calendarLog.EodFinRes = currentEodResult;
            }
            double totalProfitLoss = 0;
            foreach (var order in pickedOrders)
            {
               totalProfitLoss += order.ProfitLoss;
            }
            currentEodResult = currentEodResult + totalProfitLoss - calendarLog.SumCmsn;
            calendarLog.EodFinRes = currentEodResult;
         }
      }
   }
}
