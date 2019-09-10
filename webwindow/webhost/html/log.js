"use strict";
var fs = require("fs");
var mylog;
(function (mylog) {
    function log(msg) {
        var date = new Date();
        fs.appendFile("log.txt", date.toDateString() + ":" + msg, function () {
        });
    }
    mylog.log = log;
})(mylog || (mylog = {}));
//# sourceMappingURL=log.js.map