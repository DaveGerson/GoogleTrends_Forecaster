using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoogleTrendServiceStats.Objects;
using GAF;
using GAF.Operators;

namespace GoogleTrendServiceStats.functions
{

    public class HoltWinters
    {
        public static List<timeSeries> HWforecast(List<double> valueList, int seasonality, DateTime forecastFrom ,double alpha , double beta ,double gamma)
        {
            List<double> forecastedValues = new List<double>();
            List<double> Lt = new List<double>();
            List<double> Tt = new List<double>();
            List<double> St = new List<double>();
            List<double> Ft_tmp = new List<double>();
            List<double> EtSQR = new List<double>();

            double initAVG = valueList.GetRange(0, seasonality).Average();
            Lt.Add(initAVG); //Start term of level
            Tt.Add(0); //Start term of Trend
            int i;
            for (i = 0; i < seasonality; i++)
            {
                St.Add(valueList[i] / initAVG);
            }

            int Curcounter = seasonality;
            int valIndicator = 0;
            //Neccesary Vars

            IEnumerable<double> values = valueList.GetRange(seasonality,valueList.Count-seasonality);
            foreach (double curVal in values)
            {
                Lt.Add( alpha * (curVal / St[valIndicator]) + (1 - alpha) * (Lt[valIndicator] * Tt[valIndicator]) );
                Tt.Add(beta * (Lt[valIndicator] - Lt[valIndicator + 1]) + (1 - beta) * Tt[valIndicator]);
                St.Add(gamma * (curVal / Lt[valIndicator + 1])+(1-gamma)*St[valIndicator]);
                Ft_tmp.Add((Lt[valIndicator] + Tt[valIndicator]) * St[valIndicator] );
                EtSQR.Add( (Ft_tmp[valIndicator] - curVal) * (Ft_tmp[valIndicator] - curVal) );
                valIndicator++;
            }

            List<timeSeries> forecastedTS = new List<timeSeries>();
            double forecastedValue;
            DateTime forecastedPeriod;

            for (i = 0; i < 12; i++)
            {
                forecastedValue = ((Lt[Lt.Count - 1] + Tt[Lt.Count - 1]) * St[St.Count - seasonality - i ]);
                forecastedPeriod = forecastFrom.AddMonths(i + 1);
                forecastedTS.Add(new timeSeries(forecastedPeriod , forecastedValue));
            }

            return forecastedTS;
        }

    }

}
