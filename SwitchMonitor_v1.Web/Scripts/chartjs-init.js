var gridbordercolor = "#eee";

var randomNumber = Math.floor(Math.random() * 20);
var InitiateChartJS = function () {
    return {
        init: function () {

            var doughnutData = [
                    {
                        value: randomNumber,
                        color: themeprimary
                    },
                    {
                        value: 50,
                        color: themesecondary
                    },
                    {
                        value: 100,
                        color: themethirdcolor
                    },
                    {
                        value: 40,
                        color: themefourthcolor
                    },
                    {
                        value: 120,
                        color: themefifthcolor
                    }

            ];
           
            //new Chart(document.getElementById("doughnut").getContext("2d")).Doughnut(doughnutData);
           

        }
    };
}();
