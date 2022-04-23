import { Component, OnInit } from '@angular/core';
import { ConnectionService } from '../services/connection.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-get-contacts',
  templateUrl: './get-contacts.component.html',
  styleUrls: ['./get-contacts.component.css']
})
export class GetContactsComponent implements OnInit {

  public alertMessage: string  = null;
  public profileName: string = "DefaultLight";
  public profileType: string = "Standard";
  public extractedData: string = "";

  constructor(
    public connection: ConnectionService,
    private router: Router
  ) { }

  ngOnInit() {
    if (!this.connection.current.jwtToken) {
      this.router.navigate(['']);
    }
  }

  onGetContacts() {
    var xmlParams =
    `<?xml version="1.0" encoding="utf-8"?>
    <maxs:Contacts tbNamespace="Document.ERP.Contacts.Documents.Contacts" xTechProfile="${this.profileName}" xmlns:maxs="http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/${this.profileType}/${this.profileName}.xsd">
        <maxs:Parameters>
        </maxs:Parameters>
    </maxs:Contacts>`;
      this.extractedData = "";
      this.connection.getData(xmlParams).subscribe((result: string[]) => {
        result.forEach(record => {
          this.extractedData += record + "\n\r";
      })
    },
    (error) => {
      this.alertMessage = error;
    });
  }

}
