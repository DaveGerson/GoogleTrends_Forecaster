using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using GoogleTrendServiceStats.Objects;
using GoogleTrendServiceStats.functions;
using System.Xml;

namespace GoogleTrendServiceStats
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        public string GetData(string value)
        {

            var samples = new ChiSquared(5).Samples().Take(1000);


            string url = "http://www.google.com/trends/fetchComponent?q=" + value + "&cid=TIMESERIES_GRAPH_0&export=3";
            string result = webCall(url);


            //Due to quotas at google I was forced to run my data with a sample already extracted rather than interactively.

            //string path = HostingEnvironment.ApplicationPhysicalPath + @"/testresources/googleout.js";
            //FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            //http://stackoverflow.com/questions/17801761/converting-stream-to-string-and-back-what-are-we-missing
            //StreamReader reader = new StreamReader(stream);
            //string result = reader.ReadToEnd();

            string json = parseToJson(result.ToString());

            googleTrendsData googleDat =new googleTrendsData(json);
            googleTrendsValues trendValues = new googleTrendsValues(googleDat);


            var statistics = new DescriptiveStatistics(trendValues.valueList);
            var largestElement = statistics.Maximum;
            var smallestElement = statistics.Minimum;
            var mean = statistics.Mean;
            var variance = statistics.Variance;
            var stdDev = statistics.StandardDeviation;
            var kurtosis = statistics.Kurtosis;
            var skewness = statistics.Skewness;


             DateTime maxDateTime = googleDat.timeData.Max(timeSeries => timeSeries.MonthYear);

             List<timeSeries> forecast = HoltWinters.HWforecast(trendValues.valueList, 12, maxDateTime, .3 , .05 , .6 );

             string output = parseFinalXML(value, largestElement, smallestElement, mean, variance, stdDev, kurtosis, skewness, forecast);




            return output;
        }

        public string webCall(string url)
        {
            HttpWebRequest req = WebRequest.Create(url)
                       as HttpWebRequest;
            string result = null;
            using (HttpWebResponse resp = req.GetResponse()
                                          as HttpWebResponse)
            {
                StreamReader reader =
                    new StreamReader(resp.GetResponseStream());
                result = reader.ReadToEnd();
            }

            return result.ToString();

        }

        private string parseToJson(string googleInput)
        {
            Regex headerEval = new Regex("// Data.*\n.*rows\":.");
            string googleInputBody = headerEval.Replace(googleInput, "");
            Regex rowheadEval = new Regex("{\"c.{25,30},");
            string googleInputRowbeheaded = rowheadEval.Replace(googleInputBody, "{");
            Regex rowmidEval = new Regex("},{\"v\"");
            string googleInputRowUnsplit = rowmidEval.Replace(googleInputRowbeheaded, ",\"v\"");
            Regex splitter = new Regex("}]},{");
            string googleInputRowSplit = splitter.Replace(googleInputRowUnsplit, "}\n{");
            Regex tailEval = new Regex("......;");
            string googleJson = tailEval.Replace(googleInputRowSplit, "");
            Regex reNamer = new Regex("{\"f\"");
            string googleJsonFinal = reNamer.Replace(googleJson, "{\"date\"");
            return googleJsonFinal;

        }


        private string parseFinalXML(string trend,double max, double min, double average, double variance, double stdDev, double kurtosis , double skewness , List<timeSeries> forecasts )
        {

            XmlDocument outdoc = new XmlDocument();
            XmlElement xmlRoot = outdoc.CreateElement("Trend");
            outdoc.AppendChild(xmlRoot);

            XmlNode trendNode = outdoc.CreateElement(trend);

            XmlAttribute maxAtt = outdoc.CreateAttribute("Max");
            maxAtt.Value = max.ToString();
            trendNode.Attributes.Append(maxAtt);

            XmlAttribute minAtt = outdoc.CreateAttribute("Min");
            minAtt.Value = min.ToString();
            trendNode.Attributes.Append(maxAtt);

            XmlAttribute avgAtt = outdoc.CreateAttribute("Avg");
            avgAtt.Value = average.ToString();
            trendNode.Attributes.Append(avgAtt);

            XmlAttribute varAtt = outdoc.CreateAttribute("Variance");
            varAtt.Value = variance.ToString();
            trendNode.Attributes.Append(varAtt);

            XmlAttribute stdDevAtt = outdoc.CreateAttribute("stdDev");
            stdDevAtt.Value = max.ToString();
            trendNode.Attributes.Append(stdDevAtt);

            XmlAttribute kurtAtt = outdoc.CreateAttribute("kurtosis");
            kurtAtt.Value = max.ToString();
            trendNode.Attributes.Append(kurtAtt);

            XmlAttribute skewAtt = outdoc.CreateAttribute("skew");
            skewAtt.Value = max.ToString();
            trendNode.Attributes.Append(skewAtt);

            XmlNode forecastNode = outdoc.CreateElement("Forecast");

            IEnumerable<timeSeries> orderedData = forecasts.OrderBy(timeSeries => timeSeries.MonthYear);
            foreach (timeSeries ts in orderedData)
            {
                XmlNode monthNode = outdoc.CreateElement(ts.MonthYear.ToString("MMMM_yyyy"));
                monthNode.InnerText = ts.Value.ToString();
                forecastNode.AppendChild(monthNode);
            }

            trendNode.AppendChild(forecastNode);
            xmlRoot.AppendChild(trendNode);

            return outdoc.OuterXml; 

        }















    }
}
