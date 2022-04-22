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
  
  login(): Observable<Object> {
    var $login = new Observable<Object> ( observer => {
      var loginRequest: LoginRequest = {
        url: `http://${this.current.rootURL}`,
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

  private getRequiredHeaders() {
    var today = new Date();
    return {
      authorizationData: {
        type:"JWT",
        securityValue: this.current.jwtToken
      },
      serverInfo: {
        subscription: this.current.subscriptionKey,
        gmtOffset: -60,
        ui_culture: this.current.ui_culture,
        culture: this.current.culture,
        date : {
          day: today.getDate(),
          month: today.getMonth(),
          year: today.getFullYear()
        }
      }
    };
  }

  getData(xlmParamsBase64: string): Observable<Object> {
    var getDataRequest = Object.assign({ payload: xlmParamsBase64, loginName: this.current.accountName }, this.getRequiredHeaders());
    var $getData = new Observable<Object> ( observer => {
      this.http.post(this.baseUrl + "connection/getdata", getDataRequest, { params: {url : ""}}).subscribe((data:any) => {
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
