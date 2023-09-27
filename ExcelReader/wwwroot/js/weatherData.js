$(document).ready(function () {
    $("#getDataButton").click(function () {
        $.ajax({
            url: "https://api.weatherapi.com/v1/current.json?q=Lahore&key=07dc69e8bb3b4b94bc3131403232808",
            method: "GET",
            cache: false,
            dataType: "json",

            success: function (data) {
                // Clear existing table data
                $("#dataTable tbody").empty();
                console.log(data)
                $("#dataTable tbody").append(
                    "<tr>" +
                    "<td>" + data.location.name + "</td>" +
                    "<td>" + data.current.temp_c + "</td>" +
                    "<td>" + data.current.condition.text + "</td>" +
                    "<td>" + data.location.localtime + "</td>" +
                    "<td>" + data.current.last_updated + "</td>" +
                    "</tr>"
                );

                // Show the table
                $("#dataTable").show();
            },
            error: function (error) {
                alert("Error fetching data from the API.");
            }
        });
    });
});