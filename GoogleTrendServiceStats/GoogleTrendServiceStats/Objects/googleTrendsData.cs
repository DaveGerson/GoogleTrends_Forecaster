using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using Newtonsoft.Json.Linq;


namespace GoogleTrendServiceStats.Objects
{

    public class dateObj
    {
        public DateTime date { get; set; }
        public double v { get; set; }
        public double f { get; set; }
    }


    public class googleTrendsData
    {

        public List<timeSeries> timeData { get; set; }
        public googleTrendsData(string googleInput)
        {

            List<timeSeries> googleTimeObj = new List<timeSeries>();
            string[] googleLines = googleInput.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in googleLines)
            {
                dynamic jsonparsed = JObject.Parse(s);
                DateTime date = jsonparsed.date;
                double v = jsonparsed.v;
                googleTimeObj.Add(new timeSeries(date, v));
            }
            timeData = googleTimeObj;
        }
    }

    public class googleTrendsValues
    {

        public List<double> valueList { get; set; }
        public googleTrendsValues(googleTrendsData input)
        {

            List<double> valueList_tmp = new List<double>();

            IEnumerable<timeSeries> orderedData = input.timeData.OrderBy(timeSeries => timeSeries.MonthYear);

            foreach (timeSeries ts in orderedData)
            {
                valueList_tmp.Add(ts.Value);
            }
            valueList = valueList_tmp;
        }
    }

    public class timeSeries
    {
        public DateTime MonthYear { get; set; }
        public double Value { get; set; }
        public timeSeries(DateTime monthyear, double value)
        {
            MonthYear = monthyear;
            Value = value;
        }
    }

    public class timeSeriesobj
    {
        public DateTime MonthYear { get; set; }
        public double Value { get; set; }
    }




}