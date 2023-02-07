import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ConnectionInfo, LoginRequest, TbUserData } from '../models/connection';

const CONNECTION_INFO_TAG = "connectionInfo";

@Injectable({
  providedIn: 'root'
})
export class ConnectionService {

  public current: ConnectionInfo;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
    var currConnInfo = localStorage.getItem(CONNECTION_INFO_TAG);
    if (currConnInfo != null) {
      this.current = JSON.parse(currConnInfo);
      this.current.jwtToken = null; // should not be stored, but just in case
    }
    else
      this.current = new ConnectionInfo();
  }

  composeURL(): string {
    return `https://${this.current.rootURL}`;
  }

  login(): Observable<Object> {
    var $login = new Observable<Object> ( observer => {
      var loginRequest: LoginRequest = {
        url: this.composeURL(),
        accountName: this.current.accountName,
        password: this.current.password,
        subscriptionKey: this.current.subscriptionKey
      };
      this.http.post<TbUserData>(this.baseUrl + "connection/login", loginRequest).subscribe({
        next: (data:TbUserData) => {
          if (data.token == "" || data.isLogged == false) { // some login error, i.e.: bad subscription
            observer.error("Login failed unexpectedly.");
          } else {
            localStorage.setItem(CONNECTION_INFO_TAG, JSON.stringify(this.current));
            this.current.jwtToken = data.token;
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
      var data: TbUserData = {
        token: this.current.jwtToken,
        userName: this.current.accountName,
        password: this.current.password,
        subscriptionKey: this.current.subscriptionKey,
        isLogged: true
      }
      this.http.post(this.baseUrl + "connection/logout", data).subscribe((data:any) => {
        this.current.jwtToken = null;
        observer.next();
        observer.complete();
      },
      (error) => {
        observer.error(`${error.status} - ${error.error} - ${error.message}`);
      });
    });
    return $logout;
  }
}
