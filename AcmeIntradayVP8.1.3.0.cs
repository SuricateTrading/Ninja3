#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;

#endregion



#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		
		private Acme.Profiling.AcmeIntradayVP[] cacheAcmeIntradayVP;

		
		public Acme.Profiling.AcmeIntradayVP AcmeIntradayVP()
		{
			return AcmeIntradayVP(Input);
		}


		
		public Acme.Profiling.AcmeIntradayVP AcmeIntradayVP(ISeries<double> input)
		{
			if (cacheAcmeIntradayVP != null)
				for (int idx = 0; idx < cacheAcmeIntradayVP.Length; idx++)
					if ( cacheAcmeIntradayVP[idx].EqualsInput(input))
						return cacheAcmeIntradayVP[idx];
			return CacheIndicator<Acme.Profiling.AcmeIntradayVP>(new Acme.Profiling.AcmeIntradayVP(), input, ref cacheAcmeIntradayVP);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.Acme.Profiling.AcmeIntradayVP AcmeIntradayVP()
		{
			return indicator.AcmeIntradayVP(Input);
		}


		
		public Indicators.Acme.Profiling.AcmeIntradayVP AcmeIntradayVP(ISeries<double> input )
		{
			return indicator.AcmeIntradayVP(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.Acme.Profiling.AcmeIntradayVP AcmeIntradayVP()
		{
			return indicator.AcmeIntradayVP(Input);
		}


		
		public Indicators.Acme.Profiling.AcmeIntradayVP AcmeIntradayVP(ISeries<double> input )
		{
			return indicator.AcmeIntradayVP(input);
		}

	}
}

#endregion
