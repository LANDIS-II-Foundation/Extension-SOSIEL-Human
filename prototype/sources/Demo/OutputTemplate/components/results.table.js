"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var core_1 = require('@angular/core');
var http_1 = require('@angular/http');
require('rxjs/Rx');
var ResultsTable = (function () {
    function ResultsTable(http) {
        var _this = this;
        this.http = http;
        this.data = null;
        this.http.request('./output.json?ts=' + (new Date()).getTime()).subscribe(function (res) { console.log(res.json()); return _this.data = res.json(); });
    }
    ResultsTable = __decorate([
        core_1.Component({
            selector: 'es-results',
            templateUrl: 'components/results.table.template.html?ts=' + (new Date()).getTime()
        }), 
        __metadata('design:paramtypes', [(typeof (_a = typeof http_1.Http !== 'undefined' && http_1.Http) === 'function' && _a) || Object])
    ], ResultsTable);
    return ResultsTable;
    var _a;
}());
exports.ResultsTable = ResultsTable;
//# sourceMappingURL=results.table.js.map