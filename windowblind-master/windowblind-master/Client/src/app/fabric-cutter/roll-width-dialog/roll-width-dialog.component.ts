import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-roll-width-dialog',
  templateUrl: './roll-width-dialog.component.html',
  styleUrls: ['./roll-width-dialog.component.scss']
})
export class RollWidthDialogComponent implements OnInit {

  constructor(public dialogRef: MatDialogRef<RollWidthDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) { }

  newValue: string = "";
  ngOnInit(): void {
  }
  UpdateRollWidth() {
    this.dialogRef.close(this.newValue);
  }
}
