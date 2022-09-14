import {Inject, Injectable} from '@angular/core'
import { HttpClient, HttpHeaders } from '@angular/common/http'
import { Observable } from 'rxjs'

const httpOptions = {
  headers: new HttpHeaders({'Content-Type': 'application/json'})
};

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  url: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.url = baseUrl + 'auth';
  }

  public login(login: string, password: string): Observable<any> {
    return this.http.post(
      this.url + '/login',
      {login, password},
      httpOptions
    );
  }
}
