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
    var xmlParam =
    `<?xml version='1.0' encoding='utf-8'?>
    <maxs:Contacts tbNamespace='Document.ERP.Contacts.Documents.Contacts' xTechProfile='DefaultLight' xmlns:maxs='http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/Standard/DefaultLight.xsd'>
        <maxs:Parameters>
        </maxs:Parameters>
    </maxs:Contacts>`;
    this.connection.getData(btoa(xmlParam)).subscribe((base64Result: string[]) => {
      var result = [];
      base64Result.forEach(base64Item => {
        result.push(atob(base64Item));
      })
    },
    (error) => {
      this.alertMessage = error;
    });
  }

}
