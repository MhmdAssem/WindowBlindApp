import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
import { DataTableDirective } from 'angular-datatables';
import { Subject } from 'rxjs';
import { AuthService } from '../auth.service';
import { FabricCutterService } from '../fabric-cutter/fabric-cutter.service';
import { FabricCutterCBDetailsModel, FabricCutterCBDetailsModelTableRow } from '../fabric-cutter/FabricCutterCBDetailsModel';
import { SettingService } from '../settings/setting.service';
import { LogCutService } from './log-cut.service';

@Component({
  selector: 'app-log-cut',
  templateUrl: './log-cut.component.html',
  styleUrls: ['./log-cut.component.scss']
})
export class LogCutComponent implements OnInit {

  constructor(private logcutService: LogCutService, private FBRservice: FabricCutterService, private settingService: SettingService, private authService: AuthService) { }

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
  CBLoading: boolean = false;
  LineLoading: boolean = false;
  SendLoading: boolean = false;
  ReviewDataWithBlindsNumbers: { [Key: string]: number } = {}
  PrinterTableDictionary = {};
  ngOnInit(): void {
    this.dtOptions = {
      pagingType: 'full_numbers',
      lengthChange: false,
      searching: false,
      destroy: true,
      ordering: true,
      //pageLength: 4,
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

    this.settingService.getTableNumber('LogCutterTable').subscribe(data => {
      if ((data as string).indexOf("@@@@@") != -1) {
        let entries = (data as string).split("#####");

        entries.forEach(element => {

          let data = element.split("@@@@@");
          this.TableNames.push(data[1]);
          this.PrinterTableDictionary[data[1]] = data[0];

        });
      }


    });

  }

  ngAfterViewInit(): void {

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

  GetLineNoDetails() {

  }

  GetCBDetails(CBOrLine) {
    let input = "";
    if (CBOrLine == "CB") {
      input = (document.getElementById("CBNumber") as HTMLInputElement).value.trim();
      this.CBLoading = true;
    }
    else {
      input = (document.getElementById("LineNumber") as HTMLInputElement).value.trim();
      this.LineLoading = true;
    }


    this.logcutService.getCBNumberDetails(input, CBOrLine).subscribe(data => {
      console.log(data);
      if (data && data.columnNames.length != 0) {
        this.tableModelColNames = [];
        this.ReviewtableModelColNames = [];
        this.BlindNumbers = [];
        this.Data = [];
        this.ReviewData = [];
        this.ReviewDataWithBlindsNumbers = {}

        //this.PrinterTableDictionary = {};
        
        setTimeout(() => {
          this.updateTable();
        }, 50);

        this.tableModelColNames = data.columnNames
        this.ReviewtableModelColNames.push("Blind Number");
        data.columnNames.forEach((order: any) => {
          this.ReviewtableModelColNames.push(order);
        });

        this.Data = data.rows;

        this.updateTable();



        setTimeout(() => {
          for (let index = 0; index < data.rows.length; index++) {
            (document.getElementById("RowNumber_" + index) as HTMLElement).setAttribute("style", 'background-color:' + data.rows[index].row['DropColour'] + " !important")
          }
          $("#Custom_Table_Pagination").html("");
          $("#Custom_Table_Info").html("");
          $("#dScenario-table_paginate").appendTo('#Custom_Table_Pagination');
          $("#dScenario-table_info").appendTo('#Custom_Table_Info');
          (document.getElementById('theSelectColumn') as HTMLElement).scrollIntoView({
            behavior: 'smooth',
            block: 'start'
          });
        }, 50);




      }
      this.LineLoading = false;
      this.CBLoading = false;

    });

  }

  SelectThisRow(ind) {

    if ((document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent == "Select") {
      (document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent = "UnSelect";

      this.ReviewDataWithBlindsNumbers[this.Data[ind].uniqueId] = this.ReviewData.length;
      this.ReviewData.push(this.Data[ind]);
    }
    else {
      this.UnSelectThisRow(ind);
    }
  }

  UnSelectThisRow(ind) {

    (document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent = "Select";

    this.ReviewData.splice(this.ReviewDataWithBlindsNumbers[this.Data[ind].uniqueId], 1);
    this.ReviewDataWithBlindsNumbers[this.Data[ind].uniqueId] = -1;

  }


  Send() {
    this.SendLoading = true;
    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    console.log(this.PrinterTableDictionary);

    let Data: FabricCutterCBDetailsModel = {
      columnNames: this.tableModelColNames,
      rows: this.ReviewData
    };
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    this.logcutService.LogCutSend(
      tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => { this.SendLoading = false; });
  }

  Delete() {

    this.ReviewData.forEach(element => {
      let ind = this.Data.findIndex(e => e.uniqueId == element.uniqueId);
      this.Data.splice(ind, 1);

    });
    this.ReviewData = [];
    this.ReviewDataWithBlindsNumbers = {};
    this.updateTable();
    setTimeout(() => {

      $("#Custom_Table_Pagination").html("");
      $("#Custom_Table_Info").html("");

      $("#dScenario-table_paginate").appendTo('#Custom_Table_Pagination');
      $("#dScenario-table_info").appendTo('#Custom_Table_Info');
      (document.getElementById('theSelectColumn') as HTMLElement).scrollIntoView({
        behavior: 'smooth',
        block: 'start'
      });
    }, 100);

  }
}
