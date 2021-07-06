import { getTranslationDeclStmts } from '@angular/compiler/src/render3/view/template';
import { EventEmitter, Output } from '@angular/core';
import { Input } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '../api.service';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { Station } from '../models/station';
import { StationListSelection } from '../models/stationListSelection';
import { Table } from '../models/table';
import { StationDialogComponent } from '../station-dialog/station-dialog.component';
import { TableDialogComponent } from '../table-dialog/table-dialog.component';

@Component({
  selector: 'app-stations',
  templateUrl: './stations.component.html',
  styleUrls: ['./stations.component.scss']
})
export class StationsComponent implements OnInit {

  constructor(private apiService: ApiService, public dialog: MatDialog) { }

  @Input() readonly = false;
  @Input() title = 'Stations';
  @Input() set refresh(refreshSubject: Subject<void>){
    refreshSubject.subscribe({
      next: () => {
        this.stations = this.getStations();
        this.selection = new StationListSelection();
      }
    });
  }
  stations = this.getStations();
  private _selection = new StationListSelection();
  get selection(): StationListSelection {
    return this._selection;
  }
  set selection(value: StationListSelection) {
    this._selection = value;
    this.selectionChange.emit(value);
  }

  @Output() tableClick = new EventEmitter<[Table, Station]>();
  @Output() selectionChange = new EventEmitter<StationListSelection>();

  getStations(): Observable<Station[]>{
    return this.apiService.getType<Station>('stations', Station).pipe(
      tap(ss => ss.forEach(s => s.tables.forEach(t => t.stationId = s.id))));
  }

  ngOnInit(): void {
  }

  addStation(): void{
    const dialogRef = this.dialog.open(StationDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      if (result){
        this.apiService.create('stations', {name: result}).subscribe(
          {complete: this.getStations}
        );
      }
    });
  }

  addTable(station: Station): void{
    const dialogRef = this.dialog.open(TableDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      if (result){
        if (station.tables == null) {
        station.tables = [];
        }
        station.tables.push({name: result, networkPath: '', stationId: station.id});
        this.apiService.update('stations', station.id, station).subscribe(
          {complete: this.getStations}
        );
      }
    });
  }

  removeTable(station: Station, table: Table): void{
    const confirmDialog = this.dialog.open(ConfirmDialogComponent);
    confirmDialog.afterClosed().subscribe(result => {
      if (result === true) {
        station.tables.splice(station.tables.indexOf(table), 1);
        this.apiService.update('stations', station.id, station).subscribe({
          complete: this.getStations
        });
      }
    });
  }

  onTableClick(table: Table, station: Station): void{
    this.tableClick.emit([table, station]);
    if (!this.readonly) {
      this.selection = {table, station};
    }
  }


  onStationClick(station: Station): void{
    if (!this.readonly) {
      this.selection = {station};
    }
  }
}
