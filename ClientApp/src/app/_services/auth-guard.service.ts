import {ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot} from "@angular/router"
import {Injectable} from "@angular/core"
import {StorageService} from "./storage.service"

@Injectable(
  {
    providedIn: 'root'
  }
)
export class AuthGuardService implements CanActivate {
  constructor(private router: Router, private storageService: StorageService) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    if (this.storageService.getToken().username !== undefined) {
      return true
    }

    this.router.navigate(['/login'], {queryParams: { returnUrl: state.url }})
      .catch(err => console.log(err))
    return false
  }
}
