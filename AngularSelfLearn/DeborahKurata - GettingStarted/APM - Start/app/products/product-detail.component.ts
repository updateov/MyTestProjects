import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Subscription } from 'rxjs/Subscription';

import { IProduct } from './product';
import { ProductService } from './product.service';

@Component({
    templateUrl: 'app/products/product-detail.component.html'
}) 
export class ProductDetailComponent
{
    pageTitle: string = 'Product Detail';
    product: IProduct;
    errorMessage: string;
    private m_sub: Subscription;

    constructor(private m_activatedRoute: ActivatedRoute, private m_router: Router, private m_productService: ProductService)
    {
    }

    ngOnDestroy()
    {
        this.m_sub.unsubscribe();
    }

    ngOnInit(): void
    {
        this.m_sub = this.m_activatedRoute.params.subscribe(params => 
        {
            let id = +params['id'];
            this.getProduct(id);
        });
    }

    onBack(): void
    {
        this.m_router.navigate(['/products']);
    }

    getProduct(id: number)
    {
        this.m_productService.getProduct(id).subscribe(product => this.product = product, error => this.errorMessage = <any>error);
    }

    onRatingClicked(message: string): void 
    {
        this.pageTitle = 'Product Detail: ' + message;
    }
}