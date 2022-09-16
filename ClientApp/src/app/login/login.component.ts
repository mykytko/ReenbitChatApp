import { Component, OnInit } from '@angular/core'
import { AuthService } from '../_services/auth.service'
import {StorageService} from "../_services/storage.service"

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  form: any = {
    username: null,
    password: null
  }
  isSuccessful = false
  isLoginFailed = false
  errorMessage = ''

  constructor(private authService: AuthService, private storageService: StorageService) { }

  ngOnInit(): void {
    if (this.storageService.getToken().username !== undefined) {
      this.isSuccessful = true
      this.isLoginFailed = false
    }
  }

  onSubmit() {
    this.authService.login(this.form.username, this.form.password).subscribe({
      next: response => {
        this.storageService.saveToken(response)
        this.isSuccessful = true
        this.isLoginFailed = false
        window.location.reload()
      },
      error: () => {
        this.isSuccessful = false
        this.isLoginFailed = true
        this.errorMessage = 'Wrong login or password'
      }
    })
    return
  }

  logout() {
    this.storageService.resetToken()
    this.isSuccessful = false
    this.isLoginFailed = false
  }
}
