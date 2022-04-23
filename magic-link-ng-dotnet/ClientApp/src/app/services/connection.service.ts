import { TbUserData, ConnectionInfo, LoginRequest } from './../models/connection';
import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

const CONNECTION_INFO_TAG = "connectionInfo";

@Injectable({
  providedIn: 'root'
})
export class ConnectionService {
  public current: ConnectionInfo;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { 
    this.current = JSON.parse(localStorage.getItem(CONNECTION_INFO_TAG));
    if (!this.current) 
      this.current = new ConnectionInfo();
    else
      this.current.jwtToken = null; // should not be stored, but just in case
  }
  
  composeURL(): string {
    if (this.current.isDebugEnv) {
      return `http://${this.current.rootURL}`;
    } else {
      return `https://${this.current.rootURL}`;
    }
  }

  login(): Observable<Object> {
    var $login = new Observable<Object> ( observer => {
      var loginRequest: LoginRequest = {
        url: this.composeURL(),
        accountName: this.current.accountName,
        password: this.current.password,
        subscriptionKey: this.current.subscriptionKey
      };
      this.http.post(this.baseUrl + "connection/login", loginRequest).subscribe((data:TbUserData) => {
        if (data.token == "" || data.isLogged == false) { // some login error, i.e.: bad subscription
          observer.error("Login failed unexpectedly.");
        } else {
          localStorage.setItem(CONNECTION_INFO_TAG, JSON.stringify(this.current));
          this.current.jwtToken = data.token;
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

  getData(xmlParams: string): Observable<Object> {
    var request = {
      xmlParams: xmlParams,
      userData: {
        token: this.current.jwtToken,
        userName: this.current.accountName,
        password: this.current.password,
        subscriptionKey: this.current.subscriptionKey,
        isLogged: true
      }
    }
    var $getData = new Observable<Object> ( observer => {
      this.http.post(this.baseUrl + "connection/getdata", request).subscribe((data:any) => {
        observer.next(data);
        observer.complete(); 
      },
      (error) => {
        observer.error(`${error.status} - ${error.error} - ${error.message}`);
      });
    });
    return $getData;
  }

}
