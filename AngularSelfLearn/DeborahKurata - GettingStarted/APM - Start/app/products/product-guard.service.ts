import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot } from '@angular/router';

@Injectable()
export class ProductDetailGuard implements CanActivate
{
    constructor(private m_router: Router)
    {

    }

    canActivate(route: ActivatedRouteSnapshot): boolean
    {
        let id = +route.url[1].path;
        if (isNaN(id) || id < 1)
        {
            alert('Invalid product Id');
            // start new navigation to redirect to list page
            this.m_router.navigate(['/products']);
            // abort current navigation
            return false;
        }
        return true;
    }
}