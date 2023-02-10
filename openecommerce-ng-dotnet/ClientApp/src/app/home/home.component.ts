import { Component, OnInit } from '@angular/core';
import { CatalogService } from '../services/catalog.service';
import { ConnectionService } from '../services/connection.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  constructor(
    public connection: ConnectionService,
    public catalog: CatalogService
    ) {}

  ngOnInit() {
    if (this.connection.current.jwtToken) {
      this.catalog.getCatalog().subscribe({
        next: (result) => {
          console.log(result);
        },
        error: (error) => {
          console.log(error);
        }
      });
    }
  }


}
