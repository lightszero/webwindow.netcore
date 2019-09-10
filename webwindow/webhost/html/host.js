"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var http = require("http");
var webhost;
(function (webhost) {
    var wshost = /** @class */ (function () {
        function wshost() {
        }
        wshost.prototype.begin = function (url) {
            var _this = this;
            this.server = new http.Server(function (req, res) {
                _this.onRequest(req, res);
            });
            this.server.listen(8888);
        };
        wshost.prototype.onRequest = function (req, res) {
            res.write("hello");
        };
        return wshost;
    }());
    webhost.wshost = wshost;
})(webhost || (webhost = {}));
//# sourceMappingURL=host.js.map