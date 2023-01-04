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
    this.current = JSON.parse(localStorage.getItem(CONNECTION_INFO_TAG));
    if (!this.current) 
      this.current = new ConnectionInfo();
    else
      this.current.jwtToken = null; // should not be stored, but just in case
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
      this.http.post(this.composeURL("account-manager/login"), loginRequest).subscribe((data:LoginResponse) => {
        if (data.JwtToken == "" || data.SubscriptionKey == "") { // some login error, i.e.: bad subscription
          observer.error(`Login failed: ${data.Message}`);
        } else {
          localStorage.setItem(CONNECTION_INFO_TAG, JSON.stringify(this.current));
          this.current.jwtToken = data.JwtToken;
          observer.next();
          observer.complete();        
        }
      },
      (error) => {
        observer.error(`${error.status} - ${error.error} - ${error.message}`);
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
