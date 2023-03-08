import { ApiService } from '../api.service';
import { Table } from '../models/table';
import { Station } from '../models/station';
import { AuthService } from './../auth.service';
import { EventEmitter, OnInit } from '@angular/core';
import { Component, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { UserDialogComponent } from '../user-dialog/user-dialog.component';
import { User } from '../models/user';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  // constructor(private authService: AuthService, private apiService: ApiService, private router: Router) { }
  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authenticationService: AuthService,
  ) {
    // redirect to home if already logged in
    if (this.authenticationService.currentUser) {
      this.router.navigate(['/Home']);
    }
  }

  get currentTable(): Table | undefined { return this.authenticationService.currentTable; }
  get currentStation(): Station | undefined { return this.authenticationService.currentStation; }

  loginForm!: FormGroup;
  loading = false;
  submitted = false;
  TypeLoginAdmin = true;
LastRole = "Admin";

  ngOnInit() {
    this.loginForm = this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });

    // get return url from route parameters or default to '/'
    // this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  // convenience getter for easy access to form fields
  get f() { return this.loginForm.controls; }

  onSubmit() {
    this.submitted = true;

    // stop here if form is invalid
    if (this.loginForm.invalid ) {
      return;
    }

    
    var user = new User();
    user.name = this.f.username.value;
    user.password = this.f.password?this.f.password.value:"";
    user.role = this.LastRole;
    this.loading = true;
    this.authenticationService.login(user)
      .subscribe(
        data => {
          if (data != null) {
            this.router.navigate(["/Home"]);
          } else {
            alert("Check your login info")
            this.loading = false;
          }
        },
        error => {
          // this.alertService.error(error);
          this.loading = false;
        });
  }

  tableClick(position: [Table, Station]): void {
    this.authenticationService.setTable(position);
  }

  changeLoginType(value) {
    this.TypeLoginAdmin = (value.target.value == 'Admin') ? true : false;
    this.LastRole = value.target.value;
    if(this.TypeLoginAdmin)
    {
      this.loginForm = this.formBuilder.group({
        username: ['', Validators.required],
        password: ['', Validators.required]
      });
    }
    else 
    {
      this.loginForm = this.formBuilder.group({
        username: ['', Validators.required],
      });
    }
  }
  
}


