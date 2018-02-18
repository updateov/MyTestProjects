import { Component } from '@angular/core';

@Component({
    selector: 'pm-app',
    template: `
        <div>
            <nav class='navBar navbar-default'>
                <div class='container-fluid'>
                    <a class='navbar-brand'>{{pageTitle}}</a>
                    <ul class='nav nav-bar'>
                        <li><a [routerLink]="['/welcome']">Home</a></li>
                        <li><a [routerLink]="['/products']">Product List</a></li>
                        <li><a [routerLink]="['/svg-learn']">SVG Learn</a></li>
                    </ul>
                </div>
            </nav>
        </div>
        <div class='container'>
            <router-outlet></router-outlet>
        </div>
    `
})
export class AppComponent 
{ 
    pageTitle: string = 'Acme Product Management';
}
