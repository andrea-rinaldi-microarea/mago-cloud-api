import { Observable } from 'rxjs';
import { Component, OnInit } from '@angular/core';
import { ConnectionService } from '../services/connection.service';
import { HttpClient, HttpResponse, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-get-contacts',
  templateUrl: './get-contacts.component.html',
  styleUrls: ['./get-contacts.component.css']
})
export class GetContactsComponent implements OnInit {

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

  onGetContacts() {
    let headers = this.connection.getConnectionHeaders();
    let options = new Object({
      headers: headers,
      // observe: "response",
      // withCredentials: true,
      // responseType: "json"
    });
    // this.http.post(this.connection.composeURL("tbserver/api/tb/document/initTBLogin/"), {}, options).subscribe( (data:any) => {
    //   console.log(document.cookie);
    // },
    // (error: any) => {
    //   console.log(error);
    // });
  }

}
