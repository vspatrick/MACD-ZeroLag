// Patrick VIVES © 2021. All rights reserved.

using System;
using System.Drawing;
using TradingPlatform.BusinessLayer;

namespace MACD_ZeroLag
{
   
	public class MACD_ZeroLag : Indicator
    {
        /// <summary>
        /// Indicator's constructor. Contains general information: name, description, LineSeries etc. 
        /// </summary>
        /// 

        #region Parameters

        // Displays Input Parameter as input field (or checkbox if value type is bolean).
        [InputParameter("Short Term EMA", 0, 1, 999, 1, 0)]
        public int ShortEMA = 12;

        [InputParameter("Long Term EMA", 0, 1, 999, 1, 0)]
        public int LongEMA = 26;

        [InputParameter("Signal EMA", 0, 1, 999, 1, 0)]
        public int Signal = 9;
        #endregion

         private HistoricalDataCustom FastEMABuffer;
         private HistoricalDataCustom SlowEMABuffer;
         private HistoricalDataCustom MACDBuffer;
         private HistoricalDataCustom SignalEMABuffer;
         private HistoricalDataCustom SignalBuffer;
        
        private Indicator DEMAShort;
        private Indicator DEMALong;

        private Indicator FastEMA;
        private Indicator SlowEMA;
        private Indicator SignalEMA;
        private Indicator SignalBEMA;


        public MACD_ZeroLag()
            : base()
        {
            // Defines indicator's name and description.
            Name = "MACD_ZeroLag";
            Description = "MACD using Double EMA Calculation / BRC Technic";

            // Defines line on demand with particular parameters.
            AddLineSeries("MACD", Color.CadetBlue, 1, LineStyle.Solid);
            AddLineSeries("Signal", Color.Red, 1, LineStyle.Solid);
            AddLineSeries("MACD-Signal", Color.Gold, 2, LineStyle.Dash);
            AddLineLevel(0);
            // By default indicator will be applied on main window of the chart
            SeparateWindow = true;
        }

        /// <summary>
        /// This function will be called after creating an indicator as well as after its input params reset or chart (symbol or timeframe) updates.
        /// </summary>
        protected override void OnInit()
        {
            ShortName = "MACD ZL (" + ShortEMA.ToString() + ": " + LongEMA.ToString() +": "+Signal.ToString()+  ")";

           FastEMABuffer = new HistoricalDataCustom(this);
           SlowEMABuffer = new HistoricalDataCustom(this);
           SignalEMABuffer = new HistoricalDataCustom(this);
           MACDBuffer = new HistoricalDataCustom(this);
           SignalBuffer = new HistoricalDataCustom(this);

            DEMAShort = Core.Indicators.BuiltIn.MA(ShortEMA, PriceType.Close, MaMode.EMA);
            DEMALong = Core.Indicators.BuiltIn.MA(LongEMA, PriceType.Close, MaMode.EMA);

            FastEMA = Core.Indicators.BuiltIn.MA(ShortEMA, PriceType.Close, MaMode.EMA);
            SlowEMA = Core.Indicators.BuiltIn.MA(LongEMA, PriceType.Close, MaMode.EMA);
            SignalEMA = Core.Indicators.BuiltIn.MA(Signal, PriceType.Close, MaMode.EMA);
            SignalBEMA = Core.Indicators.BuiltIn.MA(Signal, PriceType.Close, MaMode.EMA);

            FastEMABuffer.AddIndicator(FastEMA);
            SlowEMABuffer.AddIndicator(SlowEMA);
            SignalEMABuffer.AddIndicator(SignalEMA);
            MACDBuffer.AddIndicator(SignalBEMA);


            AddIndicator(DEMAShort);
            AddIndicator(DEMALong);

            
        }

        /// <summary>
        /// Calculation entry point. This function is called when a price data updates. 
        /// Will be runing under the HistoricalBar mode during history loading. 
        /// Under NewTick during realtime. 
        /// Under NewBar if start of the new bar is required.
        /// </summary>
        /// <param name="args">Provides data of updating reason and incoming price.</param>
        protected override void OnUpdate(UpdateArgs args)
        {
            // Add your calculations here.         

            //
            // An example of accessing the prices          
            // ----------------------------
            //
            // double bid = Bid();                          // To get current Bid price
            // double open = Open(5);                       // To get open price for the fifth bar before the current
            // 

            //
            // An example of settings values for indicator's lines
            // -----------------------------------------------
            //            
            // SetValue(1.43);                              // To set value for first line of the indicator
            // SetValue(1.43, 1);                           // To set value for second line of the indicator
            // SetValue(1.43, 1, 5);                        // To set value for fifth bar before the current for second line of the indicator

            double EMA, ZerolagEMAp, ZerolagEMAq;

            if (Count <= ShortEMA)
                return;

            FastEMABuffer[PriceType.Close] = DEMAShort.GetValue();
            SlowEMABuffer[PriceType.Close] = DEMALong.GetValue();

            for (int i = 0; i < Count; i++)
            {
                 EMA = FastEMA.GetValue(i);
                ZerolagEMAp = FastEMABuffer[PriceType.Close, i] + FastEMABuffer[PriceType.Close, i] - EMA;

                EMA = SlowEMA.GetValue(i);
                ZerolagEMAq = SlowEMABuffer[PriceType.Close, i] + SlowEMABuffer[PriceType.Close, i] - EMA;

                MACDBuffer[PriceType.Close,i] = ZerolagEMAp - ZerolagEMAq;
            }

            SignalEMABuffer[PriceType.Close] = SignalBEMA.GetValue();

            for (int i = 0; i < Count; i++)
            {
                EMA = SignalEMA.GetValue(i);
                SignalBuffer[PriceType.Close, i] = SignalEMABuffer[PriceType.Close, i] + SignalEMABuffer[PriceType.Close, i] - EMA;

            }

         

            SetValue(MACDBuffer[PriceType.Close]);
            SetValue(SignalBuffer[PriceType.Close], 1);
            SetValue(SignalEMABuffer[PriceType.Close], 2);

        }
    }
}
