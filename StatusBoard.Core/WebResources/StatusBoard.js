﻿(function () {
    "use strict";

    var addCheck = function (check) {
        var resultElement = $(
            "<tr class='status-loading'>" +
            "<td class='check-result'>...</td>" +
            "<td class='check-name'>" + check.Name + "</td>" +
            "<td class='check-message'></td>" +
            "</tr>");
        $.ajax(
        {
            type: "POST",
            async: true,
            url: "Status/Check/" + check.CheckId,
            data: "{'id': '" + resultElement.attr("id") + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                var checkResult = data.CheckResult;
                resultElement.removeClass('status-loading').addClass('status-' + checkResult.StatusValue);
                resultElement.find('td.check-result').text(checkResult.StatusValue);
                resultElement.find('td.check-message').text(checkResult.Message);

            },
            error: function (jqXHR, textStatus, errorThrown) {
                resultElement.find('td.check-result').text("SERVER ERROR");
                resultElement.removeClass('status-loading').addClass('status-ERROR');
            }
        });

        return resultElement;
    };

    // Run all checks
    $(function () {
        var placeHolder = $("#status-board-checks-placeholder");
        var tBody = placeHolder.find("tBody");
        $.ajax(
            {
                type: "POST",
                async: true,
                url: "Status/Directory",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    tBody.html('');
                    var checks = data.Checks;
                    $.each(checks, function (idx, check) {
                        tBody.append(addCheck(check));
                    });
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    placeHolder.text("ERROR in response");
                    placeHolder.addClass("fail");
                }
            });


    });

})();