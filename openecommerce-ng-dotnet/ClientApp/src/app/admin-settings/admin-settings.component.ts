import { Component, OnInit } from '@angular/core';
import { CatalogService } from '../services/catalog.service';
import { ConnectionService } from '../services/connection.service';

@Component({
  selector: 'app-admin-settings',
  templateUrl: './admin-settings.component.html',
  styleUrls: ['./admin-settings.component.css']
})
export class AdminSettingsComponent implements OnInit {

  public authenticated: boolean = false;
  public login: string = "";
  public password: string = "";

  public alertMessage: string | null  = null;

  public alertType:  string = 'alert-danger';

  constructor(
    public connection: ConnectionService,
    public catalog: CatalogService
  ) {}


  ngOnInit(): void {
  }

  onLogin() {
    if (this.login == "admin" && this.password == "admin") {
      this.authenticated = true;
      this.alertMessage = "";
    } else {
      this.alertType = 'alert-danger';
      this.alertMessage = "Bad username or password.";
    }
  }

  onCancel() {
  }

  onConnect() {
    this.connection.login().subscribe({
      next: () => {
        this.alertType = 'alert-success';
        this.alertMessage = "Connected successfully!";
      },
      error: (error) => {
        this.alertType = 'alert-danger';
        this.alertMessage = error;
      }
    })
  }

  onDisconnect() {
    this.connection.logout().subscribe({
      next: () => {

      },
      error: (error) => {
        this.alertType = 'alert-danger';
        this.alertMessage = error;
      }
    });
  }

  onSynchronizaCatalog() {
    if (this.connection.current.jwtToken) {
      this.catalog.synchronizeCatalog().subscribe({
        next: (success) => {
          console.log(success);
        },
        error: (error) => {
          console.log(error);
        }
      });
    }
  }
}
