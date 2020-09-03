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

  public alertMessage: string  = null;

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
    this.connection.getData("ABCD").subscribe(() => {
    },
    (error) => {
      this.alertMessage = error;
    });
  }

}
