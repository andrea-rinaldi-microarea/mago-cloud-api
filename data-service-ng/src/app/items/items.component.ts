import { Component, OnInit } from '@angular/core';
import { ConnectionService } from '../services/connection.service';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Router } from '@angular/router';

export interface ItemColumn {
  id: string;
  title: string;
}

@Component({
  selector: 'app-items',
  templateUrl: './items.component.html',
  styleUrls: ['./items.component.css']
})
export class ItemsComponent implements OnInit {

  public columns : ItemColumn[] = [];
  public items = [];
  public filter = "";

  constructor(
    public connection: ConnectionService,
    private http: HttpClient,
    private router: Router
  ) { }

  ngOnInit() {
    if (!this.connection.current.jwtToken) {
      this.router.navigate(['']);
    }
  }

  onList() {
    let headers = new HttpHeaders().set("Authorization", JSON.stringify({
          Type:"JWT",
          SecurityValue: this.connection.current.jwtToken
    }));
    var params : HttpParams | undefined;
    if (this.filter != "") {
      params = new HttpParams().set("filter", this.filter);
    }
    this.http.get(this.connection.composeURL("data-service/getdata/ERP.Items.Dbl.Items/default"), { headers, params }).subscribe({
      next: (data:any) => {
        this.columns = data.columns;
        this.items = data.rows;
      },
      error: (error: any) => {
        console.log(error);
      }
    });
  }
}
