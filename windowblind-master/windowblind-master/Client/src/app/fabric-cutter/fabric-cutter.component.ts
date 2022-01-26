import { SelectionModel } from '@angular/cdk/collections';
import { parseI18nMeta } from '@angular/compiler/src/render3/view/i18n/meta';
import { AfterViewInit, Component, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { Observable, of, Subject } from 'rxjs';
import { ApiService } from '../api.service';
import { AuthService } from '../auth.service';
import { SettingService } from '../settings/setting.service';
import { DataTableDirective, } from 'angular-datatables';
import { FabricCutterService } from './fabric-cutter.service';
import { FabricCutterCBDetailsModel, FabricCutterCBDetailsModelTableRow } from './FabricCutterCBDetailsModel';
import { MatDialog } from '@angular/material/dialog';
import { RollWidthDialogComponent } from './roll-width-dialog/roll-width-dialog.component';
import { HoldingStationService } from '../holding-station/holding-station.service';
import { RejectionModel } from '../holding-station/RejectionModel';
import { data } from 'jquery';
import { MatTabChangeEvent } from '@angular/material/tabs';


@Component({
  selector: 'app-fabric-cutter',
  templateUrl: './fabric-cutter.component.html',
  styleUrls: ['./fabric-cutter.component.scss']
})


export class FabricCutterComponent implements OnInit, AfterViewInit {
  HoldLoading: boolean;
  UrgentReviewDataWithBlindsNumbers: any;
  UrgentAutoUploadedSelectedRows: any = [];
  UrgentBlindNumbers: number[] = [];
  UrgentReviewDataWithBlindsObjects: any;

  constructor(private HoldingService: HoldingStationService, private dialog: MatDialog, private FBRservice: FabricCutterService, private settingService: SettingService, private apiService: ApiService, private authService: AuthService) { }

  NumberOfTables: number = 0;
  TableNames: string[] = [];
  @ViewChildren(DataTableDirective)
  dtElements: QueryList<DataTableDirective>;
  dtTrigger: Subject<any> = new Subject();
  UrgentdtTrigger: Subject<any> = new Subject();
  dtOptions: DataTables.Settings = {};
  UrgentdtOptions: DataTables.Settings = {};
  dtTriggerReview: Subject<any> = new Subject();
  UrgentdtTriggerReview: Subject<any> = new Subject();
  dtOptionsReview: DataTables.Settings = {};
  UrgentdtOptionsReview: DataTables.Settings = {};

  tableModelColNames: string[] = [];
  ReviewtableModelColNames: string[] = [];
  BlindNumbers: number[] = [];
  Data: FabricCutterCBDetailsModelTableRow[] = [];
  UrgentData: FabricCutterCBDetailsModelTableRow[] = [];
  ReviewData: FabricCutterCBDetailsModelTableRow[] = [];
  UrgentReviewData: FabricCutterCBDetailsModelTableRow[] = [];
  Loading: boolean = false;
  ReviewDataWithBlindsNumbers: { [Key: string]: number } = {}
  ReviewDataWithBlindsObjects: { [Key: string]: FabricCutterCBDetailsModelTableRow } = {}
  PrinterTableDictionary = {};
  Printing = false;
  Creating = false;
  SearchType = false;

  CurrentTab: number;
  AutoUploadedSelectedRows: string[] = [];

  ButtonIsDisabled = false;

  ngOnInit(): void {
    let Ts = this;

    this.PrinterTableDictionary = {};
    this.CurrentTab = -1;

    document.addEventListener("keydown", function (event) {
      if (event.keyCode === 13) {
        event.preventDefault();

        Ts.GetCBDetails();
      }
    });

    this.InitAllVariables();

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
      //pageLength: 4,
      paging: false,
      info: false

    };

    this.UrgentdtOptions = {
      pagingType: 'full_numbers',
      lengthChange: false,
      searching: false,
      destroy: true,
      ordering: true,
      //pageLength: 4,
      paging: false,
      info: false
    };

    this.UrgentdtOptionsReview = {
      pagingType: 'full_numbers',
      lengthChange: false,
      searching: false,
      destroy: true,
      ordering: true,
      //pageLength: 4,
      paging: false,
      info: false

    };

    this.settingService.getTableNumber('FabricCutterTable').subscribe(data => {

      if ((data as string).indexOf("@@@@@") != -1) {
        let entries = (data as string).split("#####");

        entries.forEach(element => {
          let data = element.split("@@@@@");
          this.TableNames.push(data[1]);
          this.PrinterTableDictionary[data[1]] = data[0];

        });
      }

    });

    this.settingService.GetSearchType().subscribe(res => {
      this.SearchType = res;
      this.CurrentTab = 0;
    })
  }

  ngAfterViewInit(): void {

  }

  updateTable() {
    try {

      this.dtElements.forEach((dtElement: DataTableDirective) => {
        dtElement.dtInstance.then((dtInstance: DataTables.Api) => {
          dtInstance.destroy(); // Will be ok on last dataTable, will fail on previous instances
          //dtInstance.data().clear();
          dtElement.dtTrigger.next();
          console.log("Try");
        });
      });
    }
    catch {
      this.dtTrigger.next();
      this.dtTriggerReview.next();
      this.UrgentdtTrigger.next();
      this.UrgentdtTriggerReview.next();
      console.log("Catch");
    }
  }

  ClearTable() {
    try {

      this.dtElements.forEach((dtElement: DataTableDirective) => {
        dtElement.dtInstance.then((dtInstance: DataTables.Api) => {
          //dtInstance.destroy(); // Will be ok on last dataTable, will fail on previous instances
          dtInstance.data().clear();
          dtElement.dtTrigger.next();
          console.log("Try");
        });
      });
    }
    catch {
      this.dtTrigger.next();
      this.dtTriggerReview.next();
      this.UrgentdtTrigger.next();
      this.UrgentdtTriggerReview.next();
      console.log("Catch");
    }
  }
  
  
  GetCBDetails() {
    let cb = (document.getElementById("CBNumber") as HTMLInputElement).value.trim();
    if (cb == "") { alert("Please enter a valid CB"); return; }
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    if (tableName == 'DefaultTableName') {
      alert("Please Choose a valid Table Name");
      return;
    }
    this.Loading = true;

    this.FBRservice.getCBNumberDetails(cb.toString()).subscribe(data => {
      if (data && data.columnNames.length != 0) {
        setTimeout(() => {
          this.updateTable();
        }, 50);

        this.tableModelColNames = data.columnNames
        this.ReviewtableModelColNames.push("Blind Number");
        this.ReviewtableModelColNames = this.ReviewtableModelColNames.concat(data.columnNames);

        this.Data = this.Data.concat(data.rows);


        setTimeout(() => {
          this.updateTable();
        }, 0);
        let cntr = 0;

        setTimeout(() => {
          this.Data.forEach(element => {
            if (element.row['FromHoldingStation'] == 'YES') {
              (document.getElementById("RowNumber_" + cntr) as HTMLElement).setAttribute("style", 'color: white !important;' + 'background-color: crimson !important');
            }
            cntr++;
          });
        }, 40);

        setTimeout(() => {
          (document.getElementById('theSelectColumn') as HTMLElement).scrollIntoView({
            behavior: 'smooth',
            block: 'start'
          });
        }, 500);


      }
      this.Loading = false;

    });

  }

  SelectThisRow(ind) {


    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    if (tableName == 'DefaultTableName') {
      alert("Please Choose a valid Table Name");
      return;
    }

    if (this.CurrentTab <= 0) {

      (document.getElementById("TableViewReview") as HTMLElement).style.display = '';


      this.ReviewDataWithBlindsNumbers[this.Data[ind].uniqueId] = this.Data[ind].blindNumbers.length;
      this.ReviewDataWithBlindsObjects[this.Data[ind].uniqueId] = JSON.parse(JSON.stringify(this.Data[ind]));

      this.AutoUploadedSelectedRows.push(this.Data[ind].uniqueId);
      if (this.Data[ind].row['Quantity'] != null && this.Data[ind].row['Quantity'] != undefined) this.Data[ind].row['Quantity'] = '1';
      for (let i = 0; i < this.Data[ind].blindNumbers.length; i++) {
        let NewEntry = JSON.parse(JSON.stringify(this.Data[ind]));

        NewEntry.row['Blind Number'] = this.Data[ind].blindNumbers[i].toString();

        this.ReviewData.push(NewEntry);
        this.BlindNumbers.push(this.Data[ind].blindNumbers[i]);
      }
      this.ReviewData.sort((a, b) => a.row['Blind Number'].localeCompare(b.row['Blind Number']));

      this.Data.splice(ind, 1);

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
    else {
      (document.getElementById("UrgentTableViewReview") as HTMLElement).style.display = '';


      this.UrgentReviewDataWithBlindsNumbers[this.UrgentData[ind].uniqueId] = this.UrgentData[ind].blindNumbers.length;
      this.UrgentReviewDataWithBlindsObjects[this.UrgentData[ind].uniqueId] = JSON.parse(JSON.stringify(this.UrgentData[ind]));

      this.UrgentAutoUploadedSelectedRows.push(this.UrgentData[ind].uniqueId);
      if (this.UrgentData[ind].row['Quantity'] != null && this.UrgentData[ind].row['Quantity'] != undefined)
        this.UrgentData[ind].row['Quantity'] = '1';

      for (let i = 0; i < this.UrgentData[ind].blindNumbers.length; i++) {
        let NewEntry = JSON.parse(JSON.stringify(this.UrgentData[ind]));

        NewEntry.row['Blind Number'] = this.UrgentData[ind].blindNumbers[i].toString();
        this.UrgentReviewData.push(NewEntry);
        this.UrgentBlindNumbers.push(this.UrgentData[ind].blindNumbers[i]);
      }

      this.UrgentData.splice(ind, 1);

      this.updateTable();

      setTimeout(() => {


        (document.getElementById('UrgenttheSelectColumn') as HTMLElement).scrollIntoView({
          behavior: 'smooth',
          block: 'start'
        });
      }, 100);



    }


  }

  UnSelectThisRow(ind, local = false) {

    if (this.CurrentTab <= 0) {
      this.ReviewDataWithBlindsNumbers[this.ReviewData[ind].uniqueId]--;

      if (this.ReviewDataWithBlindsNumbers[this.ReviewData[ind].uniqueId] == 0) {
        this.Data.push(this.ReviewDataWithBlindsObjects[this.ReviewData[ind].uniqueId]);
      }

      let id = this.ReviewData[ind].uniqueId;
      if (!this.SearchType) {
        let indOfAutoUploadSelected = this.AutoUploadedSelectedRows.findIndex(e => e == id);
        if (indOfAutoUploadSelected != -1)
          this.AutoUploadedSelectedRows.splice(indOfAutoUploadSelected, 1);

      }

      setTimeout(() => {
        let cntr = 0;
        this.Data.forEach(element => {
          if (element.row['FromHoldingStation'] == 'YES') {
            (document.getElementById("RowNumber_" + cntr) as HTMLElement).setAttribute("style", 'color: white !important;' + 'background-color: crimson !important');
          }
          cntr++;
        });
      }, 40);

      this.ReviewData.splice(ind, 1);

      if (local == false)
        this.updateTable();

      if (this.ReviewData.length == 0)
        (document.getElementById("TableViewReview") as HTMLElement).style.display = 'none';
    }
    else {

      this.UrgentReviewDataWithBlindsNumbers[this.UrgentReviewData[ind].uniqueId]--;

      if (this.UrgentReviewDataWithBlindsNumbers[this.UrgentReviewData[ind].uniqueId] == 0) {
        this.UrgentData.push(this.UrgentReviewDataWithBlindsObjects[this.UrgentReviewData[ind].uniqueId]);
      }

      let id = this.UrgentReviewData[ind].uniqueId;
      if (!this.SearchType) {
        let indOfAutoUploadSelected = this.UrgentAutoUploadedSelectedRows.findIndex(e => e == id);
        if (indOfAutoUploadSelected != -1)
          this.UrgentAutoUploadedSelectedRows.splice(indOfAutoUploadSelected, 1);
      }
      this.UrgentReviewData.splice(ind, 1);

      if (local == false)
        this.updateTable();

      if (this.UrgentReviewData.length == 0)
        (document.getElementById("UrgentTableViewReview") as HTMLElement).style.display = 'none';
    }
  }

  PrintLabelsOnly() {
    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    this.Printing = true;
    let Data: FabricCutterCBDetailsModel = {
      columnNames: this.tableModelColNames,
      rows: this.ReviewData
    };
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    if (tableName == 'DefaultTableName') {
      alert("Please Choose a valid Table Name");
      return;
    }


    if (this.SearchType == false) {
      if (this.CurrentTab <= 0) {
        this.FBRservice.PrintLabels(
          tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => {
            this.FBRservice.UpdateRows(this.AutoUploadedSelectedRows).subscribe();
            this.AutoUploadedSelectedRows = [];
            this.Printing = false;
            this.ReviewData = [];
            this.updateTable();
          });



      }
      else {
        Data = {
          columnNames: this.tableModelColNames,
          rows: this.UrgentReviewData
        };
        this.FBRservice.PrintLabels(
          tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => {
            this.FBRservice.UpdateRows(this.UrgentAutoUploadedSelectedRows).subscribe();
            this.UrgentAutoUploadedSelectedRows = [];
            this.Printing = false;
            this.UrgentReviewData = [];
            this.updateTable();

          });


      }
    }
    else {
      this.FBRservice.PrintLabels(
        tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => {
          this.Printing = false;
          this.ReviewData = [];
          this.updateTable();

        });

    }

  }

  CreateFileLabels() {
    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    this.Creating = true;
    let Data: FabricCutterCBDetailsModel = {
      columnNames: this.tableModelColNames,
      rows: this.ReviewData
    };
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    if (tableName == 'DefaultTableName') {
      alert("Please Choose a valid Table Name");
      return;
    }

    if (this.SearchType == false) {
      if (this.CurrentTab <= 0) {
        this.FBRservice.CreateFilesAndLabels(
          tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => {
            this.FBRservice.UpdateRows(this.AutoUploadedSelectedRows).subscribe();
            this.AutoUploadedSelectedRows = [];
            this.Creating = false;
            this.ReviewData = [];
            this.updateTable();

          });


      }
      else {

        Data = {
          columnNames: this.tableModelColNames,
          rows: this.UrgentReviewData
        };
        this.FBRservice.CreateFilesAndLabels(
          tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => {
            this.FBRservice.UpdateRows(this.UrgentAutoUploadedSelectedRows).subscribe();
            this.UrgentAutoUploadedSelectedRows = [];
            this.Creating = false;
            this.UrgentReviewData = [];
            this.updateTable();

          });

      }
    }
    else {
      this.FBRservice.CreateFilesAndLabels(
        tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => {
          this.Creating = false;
          this.ReviewData = [];
          this.updateTable();

        });
    }

  }

  ClearReview() {

    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    if (tableName == 'DefaultTableName') {
      alert("Please Choose a valid Table Name");
      return;
    }

    if (this.CurrentTab <= 0) {
      let Data: FabricCutterCBDetailsModel = {
        columnNames: this.tableModelColNames,
        rows: this.ReviewData
      };

      this.FBRservice.ClearOrdersFromFabricCutter(Data, UserName, tableName).subscribe(ans => {

        this.ReviewData.splice(0);
        this.updateTable();

      });
    }
    else {

      let Data: FabricCutterCBDetailsModel = {
        columnNames: this.tableModelColNames,
        rows: this.UrgentReviewData
      };
      this.FBRservice.ClearOrdersFromFabricCutter(Data, UserName, tableName).subscribe(ans => {
        this.UrgentReviewData.splice(0);
        this.updateTable();
      });
    }

  }

  OpenDialog() {
    let diag = this.dialog.open(RollWidthDialogComponent);

    diag.afterClosed().subscribe(res => {
      if (res != null && res != "") {
        if (this.CurrentTab <= 0) {
          this.ReviewData.forEach(element => {
            element.row['Roll Width'] = res;
          });
          this.updateTable();

        }
        else {
          this.UrgentReviewData.forEach(element => {
            element.row['Roll Width'] = res;
          });
          this.updateTable();

        }
      }
    });
  }

  Hold() {

    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    var time = new Date();

    this.HoldLoading = true;
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();

    let RejectionModels: RejectionModel[] = [];
    if (this.CurrentTab <= 0)
      this.ReviewData.forEach(element => {
        let RejectionModel: RejectionModel =
        {
          dateTime: time.toLocaleString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit', day: '2-digit', month: "2-digit", year: 'numeric', hour12: true }),
          forwardedToStation: "Admin",
          id: "",
          row: element,
          stationName: "FabricCut",
          tableName: tableName,
          userName: UserName,
          rejectionReasons: []
        };
        RejectionModels.push(RejectionModel);
      });
    else
      this.UrgentReviewData.forEach(element => {
        let RejectionModel: RejectionModel =
        {
          dateTime: time.toLocaleString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit', day: '2-digit', month: "2-digit", year: 'numeric', hour12: true }),
          forwardedToStation: "Admin",
          id: "",
          row: element,
          stationName: "Fabric",
          tableName: tableName,
          userName: UserName,
          rejectionReasons: []
        };
        RejectionModels.push(RejectionModel);
      });

    this.HoldingService.RejectThisRow(RejectionModels).subscribe(() => {

      this.ReviewData = [];

      this.ReviewDataWithBlindsNumbers = {};

      setTimeout(() => {
        this.updateTable();
      }, 50);


      this.HoldLoading = false;
    });

  }

  AutoUploadFeatureOnly() {
    this.InitAllVariables();

    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    if (tableName == '-') return;
    let ShiftTable = "";
    if (!this.SearchType)
      ShiftTable = (document.getElementById("ShiftTable") as HTMLSelectElement).value.toString();

    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";

    if (!this.SearchType) {
      if (tableName == 'DefaultTableName') {
        alert("Please Choose a valid Table Name");
        return;
      }
      //normal
      this.FBRservice.GetDataUsingAutoUpload(tableName, UserName, ShiftTable, "Normal").subscribe(data => {

        if (data && data.columnNames.length != 0) {

          this.tableModelColNames = data.columnNames
          this.ReviewtableModelColNames.push("Blind Number");
          data.columnNames.forEach((order: any) => {
            this.ReviewtableModelColNames.push(order);
          });
          
         
          this.ClearTable();  
        
          
          
          this.Data = data.rows;

          this.updateTable();

          setTimeout(() => {
            let cntr = 0;

            this.Data.forEach(element => {
              if (element.row['FromHoldingStation'] == 'YES') {
                (document.getElementById("RowNumber_" + cntr) as HTMLElement).setAttribute("style", 'color: white !important;' + 'background-color: crimson !important');
              }
              cntr++;
            });
          }, 40);
          setTimeout(() => {

            $("#Custom_Table_Pagination").html("");
            $("#Custom_Table_Info").html("");
            $("#dScenario-table_paginate").appendTo('#Custom_Table_Pagination');
            $("#dScenario-table_info").appendTo('#Custom_Table_Info');
            (document.getElementById('theSelectColumn') as HTMLElement).scrollIntoView({
              behavior: 'smooth',
              block: 'start'
            });
          }, 500);
        }

      });

      // get Urgent
      this.FBRservice.GetDataUsingAutoUpload(tableName, UserName, ShiftTable, "Urgent").subscribe(data => {

        if (data && data.columnNames.length != 0) {
          

          setTimeout(() => {
            this.updateTable();
          }, 50);

          this.tableModelColNames = data.columnNames
        
          this.UrgentData = data.rows;

          this.updateTable();

          setTimeout(() => {
            let cntr = 0;

            this.UrgentData.forEach(element => {
              if (element.row['FromHoldingStation'] == 'YES') {
                (document.getElementById("RowNumber_" + cntr) as HTMLElement).setAttribute("style", 'color: white !important;' + 'background-color: crimson !important');
              }
              cntr++;
            });
          }, 40);
          setTimeout(() => {

            $("#Custom_Table_Pagination").html("");
            $("#Custom_Table_Info").html("");
            $("#dScenario-table_paginate").appendTo('#Custom_Table_Pagination');
            $("#dScenario-table_info").appendTo('#Custom_Table_Info');
            (document.getElementById('theSelectColumn') as HTMLElement).scrollIntoView({
              behavior: 'smooth',
              block: 'start'
            });
          }, 500);
        }

      });

    }
    else {

      this.ButtonIsDisabled = true;

      this.FBRservice.GetHeldObjects(tableName).subscribe(
        data => {
          if (data && data.columnNames.length != 0) {

            this.tableModelColNames = data.columnNames
            this.ReviewtableModelColNames.push("Blind Number");
            this.ReviewtableModelColNames = this.ReviewtableModelColNames.concat(data.columnNames);

            this.Data = this.Data.concat(data.rows);

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
          this.ButtonIsDisabled = false;
        }

      );

    }

  }

  tabChanged(tabChangeEvent: MatTabChangeEvent): void {

    this.CurrentTab = tabChangeEvent.index;

  }

  SelectAll() {
    let Buttons = document.getElementsByClassName("SelectAllTag") as unknown as HTMLButtonElement[];
    console.log(Buttons.length)
    let btn = document.getElementById("AllButton");

    if (btn?.textContent?.trim() == 'Select All') {

      btn.textContent = "UnSelect All";
      for (let i = Buttons.length - 1; i >= 0; i--) {
        if (Buttons[i].textContent == 'Select') Buttons[i].click();
      }
    }
    else {
      btn ? btn.textContent = "Select All" : null;
      Buttons = document.getElementsByClassName("UnSelectAllTag") as unknown as HTMLButtonElement[];
      for (let i = Buttons.length - 1; i >= 0; i--) {
        if (Buttons[i].textContent == 'UnSelect') Buttons[i].click();
      }
    }

  }


  InitAllVariables() {
    this.tableModelColNames = [];
    this.ReviewtableModelColNames = [];
    this.BlindNumbers = [];
    this.Data = [];
    this.ReviewData = [];
    this.ReviewDataWithBlindsNumbers = {}
    this.ReviewDataWithBlindsObjects = {}
    this.UrgentData = [];
    this.UrgentReviewData = [];
    this.UrgentReviewDataWithBlindsNumbers = {}
    this.UrgentReviewDataWithBlindsObjects = {}
   }
}
