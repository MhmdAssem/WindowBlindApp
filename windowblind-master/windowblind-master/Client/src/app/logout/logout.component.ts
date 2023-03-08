import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { User } from '../models/user';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.scss']
})
export class LogoutComponent implements OnInit {

  constructor(private authService: AuthService, private router: Router) { }
  ngOnInit(): void {
  }

  get currentUser(): User | undefined { return this.authService.currentUser; }

  onSignOut(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

}
