import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { DataTableDirective } from 'angular-datatables';
import { Subject } from 'rxjs';
import { AuthService } from '../auth.service';
import { FabricCutterService } from '../fabric-cutter/fabric-cutter.service';
import { FabricCutterCBDetailsModelTableRow } from '../fabric-cutter/FabricCutterCBDetailsModel';
import { LogCutService } from '../Log-Cut/log-cut.service';
import { PackingStationService } from '../packing-station/packing-station.service';
import { SettingService } from '../settings/setting.service';
import { AdminNotesModelComponent } from './Admin_Notes_Model/admin-notes-model/admin-notes-model.component';
import { ReportStationService } from './report-station.service';

@Component({
  selector: 'app-report-station',
  templateUrl: './report-station.component.html',
  styleUrls: ['./report-station.component.scss']
})
export class ReportStationComponent implements OnInit {
  constructor(private dialog: MatDialog, private reportservice: ReportStationService, private settingService: SettingService, private authService: AuthService) { }

  NumberOfTables: number = 0;
  TableNames: string[] = [];
  @ViewChildren(DataTableDirective)
  dtElements: QueryList<DataTableDirective>;
  dtTrigger: Subject<any> = new Subject();
  dtOptions: DataTables.Settings = {};
  dtTriggerReview: Subject<any> = new Subject();
  dtOptionsReview: DataTables.Settings = {};

  tableModelColNames: string[] = [];
  ReviewtableModelColNames: string[] = [];
  BlindNumbers: number[] = [];
  Data: FabricCutterCBDetailsModelTableRow[] = [];
  ReviewData: FabricCutterCBDetailsModelTableRow[] = [];
  RefreshLoading: boolean = false;
  SendLoading: boolean = false;
  Loading: boolean = false;
  ReviewDataWithBlindsNumbers: { [Key: string]: number } = {}
  PrinterTableDictionary = {};
  ngOnInit(): void {
    this.dtOptions = {
      pagingType: 'full_numbers',
      lengthChange: false,
      searching: false,
      destroy: true,
      ordering: true,
      //pageLength: 10,
      paging: false,
      info: false
    };

    this.dtOptionsReview = {
      pagingType: 'full_numbers',
      lengthChange: false,
      searching: false,
      destroy: true,
      ordering: true,
      pageLength: 4,

    };

  }

  updateTable() {
    try {
      this.dtElements.forEach((dtElement: DataTableDirective) => {
        dtElement.dtInstance.then((dtInstance: DataTables.Api) => {
          dtInstance.destroy(); // Will be ok on last dataTable, will fail on previous instances
          dtElement.dtTrigger.next();
          console.log("Try");
        });
      });
    }
    catch {
      this.dtTrigger.next();
      this.dtTriggerReview.next();

      console.log("Catch");
    }
  }




  GetCBNumberReports() {
    let cb = (document.getElementById("CBNumber") as HTMLInputElement).value.trim();
    if (cb == "") { alert("Please enter a valid CB"); return; }
    this.Loading = true;

    this.reportservice.GenerateReports(cb).subscribe(data => {

      if (data && data.rows.length != 0 && data.columnNames.length != 0) {
        setTimeout(() => {
          this.updateTable();
        }, 50);

        this.tableModelColNames = data.columnNames

        this.Data = data.rows;
        this.updateTable();


      }
      if (this.Data.length == 0)
        alert("This CB or Line number is not found !");
      (document.getElementById("CBNumber") as HTMLInputElement).value = "";
      this.Loading = false;
    });

  }


  ViewAdminNotes(i) {
    this.dialog.open(AdminNotesModelComponent, { data: this.Data[i].row['Admin_Notes'] });
  }

}
