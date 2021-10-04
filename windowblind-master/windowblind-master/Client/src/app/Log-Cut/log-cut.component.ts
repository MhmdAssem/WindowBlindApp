import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
import { DataTableDirective } from 'angular-datatables';
import { table } from 'console';
import { Subject } from 'rxjs';
import { AuthService } from '../auth.service';
import { FabricCutterService } from '../fabric-cutter/fabric-cutter.service';
import { FabricCutterCBDetailsModel, FabricCutterCBDetailsModelTableRow } from '../fabric-cutter/FabricCutterCBDetailsModel';
import { HoldingStationService } from '../holding-station/holding-station.service';
import { RejectionModel } from '../holding-station/RejectionModel';
import { SettingService } from '../settings/setting.service';
import { LogCutService } from './log-cut.service';

@Component({
  selector: 'app-log-cut',
  templateUrl: './log-cut.component.html',
  styleUrls: ['./log-cut.component.scss']
})
export class LogCutComponent implements OnInit {
  HoldLoading: boolean;
  FirstTimeOnly: boolean;

  constructor(private HoldingService: HoldingStationService, private logcutService: LogCutService, private FBRservice: FabricCutterService, private settingService: SettingService, private authService: AuthService) { }

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



    this.FirstTimeOnly = true;

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

      if (data && data.columnNames.length != 0) {
        if (this.FirstTimeOnly) {

          this.tableModelColNames = [];
          this.ReviewtableModelColNames = [];
          this.BlindNumbers = [];
          this.Data = [];
          this.ReviewData = [];
          this.ReviewDataWithBlindsNumbers = {}
        }


        setTimeout(() => {
          this.updateTable();
        }, 10);

        this.tableModelColNames = data.columnNames
        this.ReviewtableModelColNames.push("Blind Number");

        if (!this.FirstTimeOnly)
          this.Data = this.Data.concat(data.rows);
        else
          this.Data = data.rows;

        console.log(this.Data);

        this.updateTable();


        setTimeout(() => {
          for (let index = 0; index < this.Data.length; index++) {
            if (this.Data[index].row['FromHoldingStation'] == 'YES') {
              (document.getElementById("RowNumber_" + index) as HTMLElement).setAttribute("style", 'color: white !important;' + 'background-color: crimson !important'); continue;
            }

            (document.getElementById("RowNumber_" + index) as HTMLElement).setAttribute("style", 'background-color:' + this.Data[index].row['DropColour'] + " !important")
            if (this.Data[index].row['DropColour'].toLowerCase() != "white" && this.Data[index].row['DropColour'].trim() != "")
              (document.getElementById("RowNumber_" + index) as HTMLElement).setAttribute("style", 'color: white !important;' + 'background-color:' + this.Data[index].row['DropColour'] + " !important")

            if (this.Data[index].row['DropColour'].toLowerCase() == "yellow" && this.Data[index].row['DropColour'].trim() != "")
              (document.getElementById("RowNumber_" + index) as HTMLElement).setAttribute("style", 'color: white !important;' + 'background-color: LightYellow !important');
          }

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


    let Data: FabricCutterCBDetailsModel = {
      columnNames: this.tableModelColNames,
      rows: this.ReviewData
    };
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    this.logcutService.LogCutSend(
      tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => {
        this.SendLoading = false;

        this.ReviewData.forEach(element => {
          let ind = this.Data.findIndex(d => d.uniqueId == element.uniqueId);
          this.Data.splice(ind, 1);
        });

        this.ReviewData = [];
        this.ReviewDataWithBlindsNumbers = {};
      });
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


  Hold() {

    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    var time = new Date();

    this.HoldLoading = true;
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();

    let RejectionModels: RejectionModel[] = [];
    this.ReviewData.forEach(element => {
      let RejectionModel: RejectionModel =
      {
        dateTime: time.toLocaleString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit', day: '2-digit', month: "2-digit", year: 'numeric', hour12: true }),
        forwardedToStation: "Admin",
        id: "",
        row: element,
        stationName: "LogCut",
        tableName: tableName,
        userName: UserName
      };
      RejectionModels.push(RejectionModel);
    });



    this.HoldingService.RejectThisRow(RejectionModels).subscribe(() => {

      this.ReviewData.forEach(element => {

        var ind = this.Data.findIndex(d => d.uniqueId == element.uniqueId);
        this.Data.splice(ind, 1);

      });
      this.ReviewData = [];
      this.ReviewDataWithBlindsNumbers = {};
      this.updateTable();

      this.HoldLoading = false;
    });
  }

  GetHeldBasedOnTable() {
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    if (tableName == '-') return;

    (document.getElementById("SearchButton1") as HTMLButtonElement).disabled = true;
    (document.getElementById("SearchButton2") as HTMLButtonElement).disabled = true;


    setTimeout(() => {
      (document.getElementById("SearchButton1") as HTMLButtonElement).disabled = false;
      (document.getElementById("SearchButton2") as HTMLButtonElement).disabled = false;
    }, 1200);

    if (this.FirstTimeOnly) {
      this.FirstTimeOnly = false;

      this.logcutService.GetHeldObjects(tableName).subscribe(
        data => {
          if (data && data.columnNames.length != 0) {

            this.tableModelColNames = data.columnNames

            if (!this.FirstTimeOnly)
              this.Data = this.Data.concat(data.rows);
            else
              this.Data = data.rows;



            let cntr = 0;
            setTimeout(() => {
              this.Data.forEach(element => {
                if (element.row['FromHoldingStation'] == 'YES') {
                  (document.getElementById("RowNumber_" + cntr) as HTMLElement).setAttribute("style", 'color: white !important;' + 'background-color: crimson !important');
                }
                cntr++;
              });
            }, 40);

          }
        }
      );
    }
  }

}
