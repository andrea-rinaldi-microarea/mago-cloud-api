import { SetDataResponse } from './../models/connection';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ConnectionService } from '../services/connection.service';

@Component({
  selector: 'app-post-contact',
  templateUrl: './post-contact.component.html',
  styleUrls: ['./post-contact.component.css']
})
export class PostContactComponent implements OnInit {

  public alertMessage: string | null  = null;
  public profileName: string = "DefaultLight";
  public profileType: string = "Standard";
  public contactName: string = "";
  public company: string = "";
  public address: string = "";
  public phone: string = "";
  public postbackData: string = "";

  constructor(
    public connection: ConnectionService,
    private router: Router
  ) { }

  ngOnInit() {
    if (!this.connection.current.jwtToken) {
      this.router.navigate(['']);
    }
  }

  onPostContact() {
    var xmlData =
    `<?xml version="1.0" encoding="utf-8"?>
    <maxs:Contacts tbNamespace="Document.ERP.Contacts.Documents.Contacts" xTechProfile="${this.profileName}" xmlns:maxs="http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/${this.profileType}/${this.profileName}.xsd">
      <maxs:Data>
        <maxs:Contacts master='true'>
          <maxs:CompanyName>${this.company}</maxs:CompanyName>
          <maxs:Address>${this.address}</maxs:Address>
          <maxs:Telephone1>${this.phone}</maxs:Telephone1>
          <maxs:ContactPerson>${this.contactName}</maxs:ContactPerson>
        </maxs:Contacts>
      </maxs:Data>
    </maxs:Contacts>`;
    this.connection.setData(xmlData).subscribe({
      next: (result: SetDataResponse) => {
        this.postbackData = result.xmlData;
      },
      error: (error) => {
        this.alertMessage = error;
      }
    });

  }
}
