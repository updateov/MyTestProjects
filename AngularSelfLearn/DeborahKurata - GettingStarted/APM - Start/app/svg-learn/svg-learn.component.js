"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var SVGLearnComponent = (function () {
    function SVGLearnComponent() {
    }
    SVGLearnComponent.prototype.ngOnInit = function () {
    };
    return SVGLearnComponent;
}());
SVGLearnComponent = __decorate([
    core_1.Component({
        moduleId: module.id,
        selector: 'd3bar',
        template: "\n            <div id=\"main-div\">\n                <div class=\"row\">\n                    <h3>SVG Bar</h3>\n                    <!--<svg>\n                        <rect width=\"50\" height=\"200\" style=\"fill:blue\"/>\n                    </svg>-->\n                </div>\n                <div class=\"row\" id=\"d3-bar1\">\n                    <h3>D3 Bar</h3>\n                    <script>\n                        d3.select(\"#d3-bar1\").append(\"svg\").append(\"rect\").attr(\"width\",50).attr(\"height\",200).style(\"fill\",\"blue\");\n                    </script>\n                </div>\n            </div>        \n    ",
        styleUrls: ['svg-learn.component.css']
    })
], SVGLearnComponent);
exports.SVGLearnComponent = SVGLearnComponent;
//# sourceMappingURL=svg-learn.component.js.map