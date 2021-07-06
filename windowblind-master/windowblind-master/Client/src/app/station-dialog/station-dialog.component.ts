import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-station-dialog',
  templateUrl: './station-dialog.component.html',
  styleUrls: ['./station-dialog.component.scss']
})
export class StationDialogComponent implements OnInit {

  constructor(
    public dialogRef: MatDialogRef<StationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: string) {}

  close(newData?: string): void {
    this.dialogRef.close(newData);
  }
  ngOnInit(): void {
  }

}
