/// <reference path="c:\git\statusboard\statusboard.core\scripts\_references.js" />

(function () {
    "use strict";

    var addCheck = function (check) {
        var resultElement = $(
            "<tr class='status-loading'>" +
            "<td class='check-result'>...</td>" +
            "<td class='check-name'>" + check.Name + "</td>" +
            "<td class='check-duration'></td>" +
            "<td class='check-message'></td>" +
            "</tr>");
        $.ajax(
        {
            type: "POST",
            async: true,
            url: "Status/Check/" + check.CheckId,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                var checkResult = data.CheckResult;
                resultElement.removeClass('status-loading').addClass('status-' + checkResult.StatusValue);
                resultElement.find('td.check-result').text(checkResult.StatusValue);
                resultElement.find('td.check-duration').text(data.Duration);
                var messageElement = resultElement.find('td.check-message');
                if (checkResult.UseHtml) {
                    messageElement.html(checkResult.Message);
                }
                else {
                    messageElement.text(checkResult.Message);
                }

            },
            error: function (jqXHR, textStatus, errorThrown) {
                resultElement.find('td.check-result').text("SERVER ERROR");
                resultElement.removeClass('status-loading').addClass('status-ERROR');
            }
        });

        return resultElement;
    };

    var loadTestsDirectory = function (placeHolder) {
        var tBody = placeHolder.find("tBody");
        // Fetch own checks
        var url = "Status/Directory";
        if (placeHolder.data.proxyId) {
            url += "/" + placeHolder.data.proxyId;
        }
        $.ajax(
            {
                type: "POST",
                async: true,
                url: url,
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
    };

    // Run all checks
    $(function () {
        var placeHolder = $("#status-board-checks-placeholder");
        var placeHolderTemplate = placeHolder.clone();
        loadTestsDirectory(placeHolder);
        // Fetch proxies
        $.ajax(
            {
                type: "POST",
                async: true,
                url: "Status/Proxy",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    var proxies = data.Proxies;
                    var proxyPlaceholder = $("#proxy-placeholder");
                    $.each(proxies, function (idx, proxy) {
                        proxyPlaceholder.append("<h1>" + proxy.Title + "<h1>");
                        var url = proxy.ProxyBaseUri;
                        var clone = placeHolderTemplate.clone();
                        clone.id = "proxy_" + idx;
                        clone.data.proxyId = idx;
                        proxyPlaceholder.append(clone);
                    });
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    placeHolder.text("ERROR in response");
                    placeHolder.addClass("fail");
                }
            });

    });

    // Show/hide JSON links
    $(function () {
        var jsonLinks = $("#JSON-links");
        jsonLinks.hide(); // Initially hidden
        var jsonLinksToggle = $("#JSON-links-toogle");
        jsonLinksToggle.click(function (eventObject) {
            eventObject.preventDefault();
            jsonLinks.toggle();
        });
    });
})();