import { Injectable } from '@angular/core'
import { HttpClient, HttpHeaders } from '@angular/common/http'
import { Observable } from 'rxjs'

const AUTH_API = 'https://localhost:7069/api/auth/'

const httpOptions = {
  headers: new HttpHeaders({'Content-Type': 'application/json'})
};

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private http: HttpClient) {}

  public login(login: string, password: string): Observable<any> {
    return this.http.post(
      AUTH_API + 'login',
      {login, password},
      httpOptions
    );
  }
}
