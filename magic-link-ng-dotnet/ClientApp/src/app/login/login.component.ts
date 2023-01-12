import { ConnectionService } from './../services/connection.service';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  public alertMessage: string | null  = null;

  constructor(
    public connection: ConnectionService,
    private router: Router
  ) { }

  ngOnInit() {
  }

  onLogin() {
    this.connection.login().subscribe(() => {
      this.router.navigate(['menu']);
    },
    (error) => {
      this.alertMessage = error;
    });
  }
  onLogout() {
    this.connection.logout().subscribe(() => {

    },
    (error) => {
      this.alertMessage = error;
    });
  }
  onCancel() {

  }
}
