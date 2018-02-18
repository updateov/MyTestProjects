import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { IProduct } from './product';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/observable/throw';


@Injectable()
export class ProductService
{
    private m_productUrl = 'api/products/products.json';

    constructor(private m_http: Http)
    {

    }

    getProducts(): Observable<IProduct[]>
    {
        return this.m_http.get(this.m_productUrl).map((response: Response) => <IProduct[]>response.json())
                .do(data => console.log('All: ' + JSON.stringify(data)))
                .catch(this.handleError);
    }

    getProduct(id: number): Observable<IProduct>
    {
        return this.getProducts().map((products: IProduct[]) => products.find(p => p.productId === id));
    }

    private handleError(error: Response)
    {
        console.error(error);
        return Observable.throw(error.json().error || 'Server error');
   }
        
}