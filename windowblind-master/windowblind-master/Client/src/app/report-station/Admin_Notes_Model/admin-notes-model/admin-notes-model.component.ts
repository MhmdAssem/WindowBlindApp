import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-admin-notes-model',
  templateUrl: './admin-notes-model.component.html',
  styleUrls: ['./admin-notes-model.component.scss']
})
export class AdminNotesModelComponent implements OnInit {
  constructor(public dialogRef: MatDialogRef<AdminNotesModelComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) {
  }

  newValue: string = "";
  ngOnInit(): void {
    this.newValue = this.data;
  }


}
