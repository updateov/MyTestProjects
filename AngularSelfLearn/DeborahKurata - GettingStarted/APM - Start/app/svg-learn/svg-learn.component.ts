import { Component, OnInit } from '@angular/core';

import * as D3 from 'd3-selection';
import * as Moment from 'moment';

@Component({
    moduleId: module.id,
    selector: 'd3bar',
    template:`
            <div id="main-div">
                <div class="row">
                    <h3>SVG Bar</h3>
                    <!--<svg>
                        <rect width="50" height="200" style="fill:blue"/>
                    </svg>-->
                </div>
                <div class="row" id="d3-bar1">
                    <h3>D3 Bar</h3>
                    <script>
                        d3.select("#d3-bar1").append("svg").append("rect").attr("width",50).attr("height",200).style("fill","blue");
                    </script>
                </div>
            </div>        
    `,
    styleUrls: ['svg-learn.component.css']
}) 
export class SVGLearnComponent implements OnInit
{
    ngOnInit(): void
    {
    }
}