import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CatalogRequest } from '../models/catalog';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {

  }

  getCatalog(): Observable<Object> {
    var $getCatalog = new Observable<Object> ( observer => {

      var catalogRequest = new CatalogRequest();
      catalogRequest.filter = "01";

      this.http.post(this.baseUrl + "catalog/getCatalog", catalogRequest).subscribe({
        next: (data: any) => {
          observer.next(data);
          observer.complete();
        },
        error: (error) => {
          observer.error(`${error.status} - ${error.error} - ${error.message}`);
        }
      });
    });
    return $getCatalog;
  }
}
