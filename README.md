GoogleTrends_Forecaster
=======================

Web Service Interface to Google Trends with Holt Winters Implementation.

This code sets up a web service usign the .net framework which will take a single text parameter.

From google it retrieves javascript command notation which it will first parse with regex into a JSON.  

At this point it will be comverted to a list of time-series objects that can be easily used in mathematical calculations.  

Using Holt winters with static paremets the code forecasts for the next year.  

Finally the time series object is leveraged again for descriptive statistics.  

All objects are returned in an XML format.  
