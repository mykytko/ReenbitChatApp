import {Injectable} from "@angular/core";

const AUTH_TOKEN = 'auth-token'

@Injectable({
  providedIn: 'root'
})
export class StorageService {
  constructor() {}

  public getToken() {
    const result = window.sessionStorage.getItem(AUTH_TOKEN);
    if (result) {
      return JSON.parse(result);
    }

    return {};
  }

  public resetToken() {
    window.sessionStorage.removeItem(AUTH_TOKEN);
  }

  public saveToken(token: any): void {
    window.sessionStorage.removeItem(AUTH_TOKEN);
    window.sessionStorage.setItem(AUTH_TOKEN, JSON.stringify(token));
  }
}
