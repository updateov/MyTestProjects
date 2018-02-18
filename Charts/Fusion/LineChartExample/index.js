  FusionCharts.ready(function(){
    var fusioncharts = new FusionCharts({
    id: "stockRealTimeChart",
    type: 'realtimeline',
    renderAt: 'chart-container',
    width: '1900',
    height: '400',
    dataFormat: 'json',
    dataSource: {
        "chart": {
        "caption": "Real-time stock price monitor",
        "subCaption": "Harry's SuperMart",
        "xAxisName": "Time",
        "yAxisName": "FHR",
        "numberPrefix": "",
        "refreshinterval": "2",
        "yaxisminvalue": "30",
        "yaxismaxvalue": "240",
        "numdisplaysets": "900",
		"yAxisValuesStep": "3",
        "labeldisplay": "none",
		"drawAnchors": "0",
        "showValues": "0",
		"showLimits": "1",
        "showRealTimeValue": "0",
		"showYAxisValues": "1",
		"showXAxisLine": "1",
		"numDivLines": "21",
		"numVDivLines": "14",
        "theme": "fint"
        },
        "categories": [{
            "category": [{
                "label": ""
            }]
        }],
        "dataset": [{
            "data": [{
                "value": ""
            }]
        }]
    },
    "events": {
        "initialized": function(e) {
            function addLeadingZero(num) {
                return (num <= 9) ? ("0" + num) : num;
            }

            function updateData() {
                // Get reference to the chart using its ID
                var chartRef = FusionCharts("stockRealTimeChart"),
                    // We need to create a querystring format incremental update, containing
                    // label in hh:mm:ss format
                    // and a value (random).
                    currDate = new Date(),
					hours = addLeadingZero(currDate.getHours())
					minutes = addLeadingZero(currDate.getMinutes()),
					seconds = addLeadingZero(currDate.getSeconds()),
                    label = (minutes % 3 == 0 && (seconds == 0 || seconds == 1)) ? hours + ":" +
                    minutes + ":00": 
					"",
                    // Get random number between 35.25 & 35.75 - rounded to 2 decimal places
                    randomValue = Math.floor(Math.random() * 1400) / 100 + 225.25,
                    // Get random number between 130 & 135 - rounded
                    //randomValue = Math.floor(Math.random() * 5) + 130,
					//toSet = "" + randomValue + "|" + (randomValue + 1) + "|" + (randomValue + 2) + "|" + (randomValue - 1);
                    // Build Data String in format &label=...&value=...
                    strData = "&label=" + label + "&value=" + randomValue;
                    //strData = "&label=" + label + "&value=" + toSet;
                // Feed it to chart.
                chartRef.feedData(strData);
            }

            e.sender.chartInterval = setInterval(function() {
                updateData();
            }, 2000);
        },
        "disposed": function(evt, arg) {
            clearInterval(evt.sender.chartInterval);
        }
    }
}
);
    fusioncharts.render();
});