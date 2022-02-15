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
		
		private Acme.Profiling.AcmeCompositeVP[] cacheAcmeCompositeVP;

		
		public Acme.Profiling.AcmeCompositeVP AcmeCompositeVP()
		{
			return AcmeCompositeVP(Input);
		}


		
		public Acme.Profiling.AcmeCompositeVP AcmeCompositeVP(ISeries<double> input)
		{
			if (cacheAcmeCompositeVP != null)
				for (int idx = 0; idx < cacheAcmeCompositeVP.Length; idx++)
					if ( cacheAcmeCompositeVP[idx].EqualsInput(input))
						return cacheAcmeCompositeVP[idx];
			return CacheIndicator<Acme.Profiling.AcmeCompositeVP>(new Acme.Profiling.AcmeCompositeVP(), input, ref cacheAcmeCompositeVP);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.Acme.Profiling.AcmeCompositeVP AcmeCompositeVP()
		{
			return indicator.AcmeCompositeVP(Input);
		}


		
		public Indicators.Acme.Profiling.AcmeCompositeVP AcmeCompositeVP(ISeries<double> input )
		{
			return indicator.AcmeCompositeVP(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.Acme.Profiling.AcmeCompositeVP AcmeCompositeVP()
		{
			return indicator.AcmeCompositeVP(Input);
		}


		
		public Indicators.Acme.Profiling.AcmeCompositeVP AcmeCompositeVP(ISeries<double> input )
		{
			return indicator.AcmeCompositeVP(input);
		}

	}
}

#endregion
