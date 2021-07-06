
import { Component } from '@angular/core';
import { User } from '../models/user';
import { UsersService } from './users.service';
import * as $AB from 'jquery';
import * as bootstrap from 'bootstrap';
declare var jQuery: any;

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent {
  users!: User[];
  addUser!: User;
  name!: string;
  password!: string
  role!: string;
  id!: string;
  constructor(
    private usersService: UsersService) {

  }

  ngOnInit() {
    this.usersService.getAll().subscribe(e => this.users = e);
  }
  editUser(userId) {
    var user = this.users.find(e => e.id == userId);
    if (user == undefined) return;
    this.name = user?.name
    this.password = user?.password
    this.role = user?.role
    this.id = user?.id
    jQuery('#UserModal').modal('toggle');
    $("#addUser").hide();
    $("#editUser").show();

  }
  EditAUser() {
    this.addUser = new User();
    if (this.name == "" || this.role == "" || this.password == "") {
      alert("Plz fill the data")
      return;
    }
    this.addUser.name = this.name;
    this.addUser.password = this.password;
    this.addUser.role = this.role;
    this.addUser.id = this.id;
    console.log(this.addUser);
    this.usersService.update(this.addUser).subscribe(e => {
      jQuery('#UserModal').modal('toggle')
      this.usersService.getAll().subscribe(e => this.users = e);
      this.resetData();
    });
  }
  addAUser() {

    this.addUser = new User();
    if (this.name == "" || this.role == "" || (this.password == "" && this.role == 'Admin')) {
      alert("Plz fill the data")
      return;
    }
    this.addUser.name = this.name;
    this.addUser.password = this.password;
    this.addUser.role = this.role;
    this.addUser.id = this.id;
    console.log(this.addUser);
    this.usersService.create(this.addUser).subscribe(e => {
      jQuery('#UserModal').modal('toggle')
      this.usersService.getAll().subscribe(e => this.users = e);
      this.resetData();
    })
  }

  deleteUser(userId) {
    this.usersService.delete(userId).subscribe(e => {
      this.usersService.getAll().subscribe(e => {
        this.users = e
        alert("User Deleted")
      });
    })
  }

  openDialog(): void {
    $("#editUser").hide();
    $("#addUser").show();
    this.resetData();
    jQuery('#UserModal').modal('toggle')
  }

  resetData() {
    this.addUser = new User();
    this.name = "";
    this.id = "";
    this.password = "";
    this.role = "";
  }
}



