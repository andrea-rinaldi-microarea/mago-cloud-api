import { LoginResponse, ConnectionInfo, AuthorizationData, LoginRequest } from './../models/connection';
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
  
  composeURL(path: string): string {
    if (this.current.isDebugEnv) {
      return `http://${this.current.rootURL}/${path}`;
    } else {
      return `https://${this.current.rootURL}/be/${path}`;
    }
  }

  login(): Observable<Object> {
    var $login = new Observable<Object> ( observer => {
      var loginRequest: LoginRequest = {
        accountName: this.current.accountName,
        password: this.current.password,
        subscriptionKey: this.current.subscriptionKey,
        appId: "M4"
      };
      this.http.post(this.baseUrl + "connection/login", loginRequest, { params: {url : this.composeURL("account-manager/login")}}).subscribe((data:LoginResponse) => {
        if (data.jwtToken == "" || data.result == "false") { // some login error, i.e.: bad subscription
          observer.error(`Result code: ${data.resultCode} - ${data.message}`);
        } else {
          localStorage.setItem(CONNECTION_INFO_TAG, JSON.stringify(this.current));
          this.current.jwtToken = data.jwtToken;
          this.current.ui_culture = data.language;
          this.current.culture = data.regionalSettings;
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
    var authorizationData: AuthorizationData = {
      type: "JWT",
      securityValue: this.current.jwtToken
    };

    var $logout = new Observable<Object> ( observer => {
      this.http.post(this.baseUrl + "connection/logout", authorizationData, { params: {url : this.composeURL("account-manager/logoff")}}).subscribe((data:any) => {
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
      this.http.post(this.baseUrl + "connection/getdata", getDataRequest, { params: {url : this.composeURL("")}}).subscribe((data:any) => {
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
