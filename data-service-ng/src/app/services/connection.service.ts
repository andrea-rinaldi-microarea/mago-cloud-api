import { LoginResponse } from './../models/login-response';
import { ConnectionInfo } from './../models/connection';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

const CONNECTION_INFO_TAG = "connectionInfo";

@Injectable({
  providedIn: 'root'
})
export class ConnectionService {
  public current: ConnectionInfo;

  constructor(private http: HttpClient) {
    var currConnInfo = localStorage.getItem(CONNECTION_INFO_TAG);
    if (currConnInfo != null) {
      this.current = JSON.parse(currConnInfo);
      this.current.jwtToken = null; // should not be stored, but just in case
    }
    else
      this.current = new ConnectionInfo();
  }

  composeURL(path: string): string {
    if (this.current.isDebugEnv) {
      return `http://${this.current.rootURL}/${path}`;
    } else {
      return `https://${this.current.rootURL}/be/${path}`;
    }
  }

  login(): Observable<Object> {
    var $login = new Observable<Object> ( observer => {
      var loginRequest = {
        accountName: this.current.accountName,
        password: this.current.password,
        overwrite: false,
        subscriptionKey: this.current.subscriptionKey,
        appId: "M4"
      };

      this.http.post<LoginResponse>(this.composeURL("account-manager/login"), loginRequest).subscribe({
        next: (data) => {
          if (data.JwtToken == "" || data.SubscriptionKey == "") { // some login error, i.e.: bad subscription
            observer.error(`Login failed: ${data.Message}`);
          } else {
            localStorage.setItem(CONNECTION_INFO_TAG, JSON.stringify(this.current));
            this.current.jwtToken = data.JwtToken;
            observer.next();
            observer.complete();
          }
        },
        error: (error) => {
          observer.error(`${error.status} - ${error.error} - ${error.message}`);
        }
      });
    })
    return $login;
  }

  logout(): Observable<Object> {
    var $logout = new Observable<Object> ( observer => {
      let headers = new HttpHeaders().set("Authorization", JSON.stringify({
        Type:"JWT",
        SecurityValue: this.current.jwtToken
      }));
      this.http.post(this.composeURL("account-manager/logoff"), { }, { headers: headers }).subscribe((data:any) => {
        this.current.jwtToken = null;
        observer.next();
        observer.complete();
      });
    });
    return $logout;
  }
}
