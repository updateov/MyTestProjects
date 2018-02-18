import { NgModule} from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';

import { SVGLearnComponent } from './svg-learn.component'

@NgModule(
    {
        declarations:
        [
            SVGLearnComponent
        ],
        imports:
        [
            RouterModule.forChild(
                [
                    { path: 'svg-learn', component: SVGLearnComponent }
                ]
            )
        ],
        providers: 
        [ 
        ],

    }
)
export class SVGModule
{

}