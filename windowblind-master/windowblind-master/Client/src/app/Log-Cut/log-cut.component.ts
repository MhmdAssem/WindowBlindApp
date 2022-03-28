import { Component, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatTable, MatTableDataSource } from '@angular/material/table';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { DataTableDirective } from 'angular-datatables';
import { Console, debug, table } from 'console';
import { Subject } from 'rxjs';
import { isJSDocThisTag } from 'typescript';
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
  [x: string]: {};
  HoldLoading: boolean;
  FirstTimeOnly: boolean;
  ButtonIsDisabled: boolean;

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

  SearchType = false;
  CurrentTab: number;
  AutoUploadedSelectedRows: string[] = [];
  UrgentAutoUploadedSelectedRows: string[] = [];
  UrgentData: FabricCutterCBDetailsModelTableRow[] = [];
  UrgentReviewDataWithBlindsNumbers: any;
  UrgentReviewData: FabricCutterCBDetailsModelTableRow[] = [];
  //// For Mat tables
  //// for the normal table
  @ViewChild('Datatable', { static: false }) Datatable: MatTable<any>;
  DataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.Data);
  tableModelColNamesWithActions: string[] = [];
  @ViewChild('paginator', { static: false }) paginator: MatPaginator;



  /// for the urgent table
  @ViewChild('UrgentDatatable', { static: false }) UrgentDatatable: MatTable<any>;
  UrgentdataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.UrgentData);
  @ViewChild('Urgentpaginator', { static: false }) Urgentpaginator: MatPaginator;


  /// To Hold SelectedRows in the table



  SelectedRows = {};


  ngOnInit(): void {

    let Ts = this;
    this.SelectedRows = {};
    this.SelectedRows['Default'] = 's';
    this.InitAllVariables();
    document.addEventListener("keydown", function (event) {
      if (event.keyCode === 13) {
        event.preventDefault();

        let CBNumber = (document.getElementById("CBNumber") as HTMLInputElement).value;

        if (CBNumber != '')
          Ts.GetCBDetails();

      }
    });


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


    this.settingService.GetSearchType('_LogCut').subscribe(res => {
      this.SearchType = res;
      this.CurrentTab = 0;
    })
    this.FirstTimeOnly = true;

  }

  ngAfterViewInit(): void {

  }

  GetCBDetails() {
    let input = "";

    input = (document.getElementById("CBNumber") as HTMLInputElement).value.trim();
    this.CBLoading = true;



    this.logcutService.getCBNumberDetails(input).subscribe(data => {

      if (data && data.rows.length != 0 && data.columnNames.length != 0) {
        this.tableModelColNames = data.columnNames;
        this.tableModelColNamesWithActions = [...data.columnNames];
        this.ReviewtableModelColNames = [];
        this.ReviewtableModelColNames.push("Blind Number");
        this.ReviewtableModelColNames = this.ReviewtableModelColNames.concat(data.columnNames);


        this.tableModelColNamesWithActions.push('SelectColumn')

        this.Data = this.Data.concat(data.rows);
        this.DataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.Data);

        setTimeout(() => {
          this.DataSource.paginator = this.paginator;
          this.UpdateMatTables();
        }, 100);


      }

      if (this.Data.length == 0)
        alert("This CB or Line number is not found !");
      this.LineLoading = false;
      this.CBLoading = false;
      (document.getElementById("CBNumber") as HTMLInputElement).value = "";

    });

  }

  SelectThisRow(ind) {
    if (this.CurrentTab <= 0) {
      if ((document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent?.trim() == "Select") {

        (document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent = "UnSelect";
        ind += this.paginator.pageIndex * this.paginator.pageSize;

        this.ReviewDataWithBlindsNumbers[this.Data[ind].uniqueId] = this.ReviewData.length;
        this.ReviewData.push(this.Data[ind]);
        this.AutoUploadedSelectedRows.push(this.Data[ind].uniqueId);
        this.SelectedRows[this.Data[ind].uniqueId] = 'Selected';

      }
      else {
        ind += this.paginator.pageIndex * this.paginator.pageSize;
        this.UnSelectThisRow(ind);
        this.SelectedRows[this.Data[ind].uniqueId] = 'UnSelected';
      }
    }
    else {
      if ((document.getElementById('UrgentSelectCol_' + ind) as HTMLButtonElement).textContent?.trim() == "Select") {
        (document.getElementById('UrgentSelectCol_' + ind) as HTMLButtonElement).textContent = "UnSelect";

        ind += this.Urgentpaginator.pageIndex * this.Urgentpaginator.pageSize;
        this.UrgentReviewDataWithBlindsNumbers[this.UrgentData[ind].uniqueId] = this.UrgentReviewData.length;
        this.UrgentReviewData.push(this.UrgentData[ind]);
        this.SelectedRows[this.UrgentData[ind].uniqueId] = 'Selected';
        this.UrgentAutoUploadedSelectedRows.push(this.UrgentData[ind].uniqueId);
      }
      else {
        ind += this.Urgentpaginator.pageIndex * this.Urgentpaginator.pageSize;

        this.UnSelectThisRow(ind);
        this.SelectedRows[this.UrgentData[ind].uniqueId] = 'UnSelected';

      }
    }
  }

  UnSelectThisRow(ind) {
    if (this.CurrentTab <= 0) {
      (document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent = "Select";
      let id = this.ReviewData[ind].uniqueId;
      if (!this.SearchType) {
        let indOfAutoUploadSelected = this.AutoUploadedSelectedRows.findIndex(e => e == id);
        if (indOfAutoUploadSelected != -1)
          this.AutoUploadedSelectedRows.splice(indOfAutoUploadSelected, 1);

      }
      this.ReviewData.splice(this.ReviewDataWithBlindsNumbers[this.Data[ind].uniqueId], 1);
      this.ReviewDataWithBlindsNumbers[this.Data[ind].uniqueId] = -1;



    }
    else {
      (document.getElementById('UrgentSelectCol_' + ind) as HTMLButtonElement).textContent = "Select";
      let id = this.UrgentReviewData[ind].uniqueId;
      if (!this.SearchType) {
        let indOfAutoUploadSelected = this.UrgentAutoUploadedSelectedRows.findIndex(e => e == id);
        if (indOfAutoUploadSelected != -1)
          this.UrgentAutoUploadedSelectedRows.splice(indOfAutoUploadSelected, 1);

      }
      this.UrgentReviewData.splice(this.UrgentReviewDataWithBlindsNumbers[this.UrgentData[ind].uniqueId], 1);
      this.UrgentReviewDataWithBlindsNumbers[this.UrgentData[ind].uniqueId] = -1;
    }
  }


  Send() {
    this.SendLoading = true;
    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";

    if (this.CurrentTab <= 0) {
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
          this.DataSource = new MatTableDataSource(this.Data);
           setTimeout(() => {
            this.DataSource.paginator = this.paginator;
            this.UpdateMatTables();
          }, 100);
          this.logcutService.UpdateRows(this.AutoUploadedSelectedRows).subscribe();
          this.AutoUploadedSelectedRows = [];
          this.ReviewData = [];
          this.ReviewDataWithBlindsNumbers = {};
        });
    }
    else {
      let Data: FabricCutterCBDetailsModel = {
        columnNames: this.tableModelColNames,
        rows: this.UrgentReviewData
      };
      let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
      this.logcutService.LogCutSend(
        tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => {
          this.SendLoading = false;

          this.UrgentReviewData.forEach(element => {
            let ind = this.UrgentData.findIndex(d => d.uniqueId == element.uniqueId);
            this.UrgentData.splice(ind, 1);
            setTimeout(() => {
              this.UrgentdataSource.paginator = this.Urgentpaginator;
              this.UpdateMatTables();
            }, 100);
          });
          this.logcutService.UpdateRows(this.UrgentAutoUploadedSelectedRows).subscribe();
          this.UrgentAutoUploadedSelectedRows = [];
          this.UrgentReviewData = [];
          this.UrgentReviewDataWithBlindsNumbers = {};
        });
    }
  }

  Delete() {

    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    if (tableName == 'DefaultTableName') {
      alert("Please Choose a valid Table Name");
      return;
    }

    if (this.CurrentTab <= 0) {
      this.ReviewData.forEach(element => {
        let ind = this.Data.findIndex(e => e.uniqueId == element.uniqueId);
        this.Data.splice(ind, 1);
      });

      let Model: FabricCutterCBDetailsModel = {
        columnNames: this.tableModelColNames,
        rows: this.ReviewData
      };

      this.logcutService.ClearOrdersFromLogCut(Model, UserName, tableName).subscribe(res => {
        this.ReviewData = [];
        this.ReviewDataWithBlindsNumbers = {};
        setTimeout(() => {
          this.DataSource.paginator = this.paginator;
          this.UpdateMatTables();
        }, 100);

      });
      this.logcutService.UpdateRows(this.AutoUploadedSelectedRows).subscribe();
      this.AutoUploadedSelectedRows = [];
    }
    else {
      this.UrgentReviewData.forEach(element => {
        let ind = this.UrgentData.findIndex(e => e.uniqueId == element.uniqueId);
        this.UrgentData.splice(ind, 1);

      });

      let Model: FabricCutterCBDetailsModel = {
        columnNames: this.tableModelColNames,
        rows: this.UrgentReviewData
      };

      this.logcutService.ClearOrdersFromLogCut(Model, UserName, tableName).subscribe(res => {
        this.UrgentReviewData = [];
        this.UrgentReviewDataWithBlindsNumbers = {};

        setTimeout(() => {
          this.UrgentdataSource.paginator = this.Urgentpaginator;
          this.UpdateMatTables();
        }, 100);

      });
      this.logcutService.UpdateRows(this.UrgentAutoUploadedSelectedRows).subscribe();
      this.UrgentAutoUploadedSelectedRows = [];
    }




  }


  Hold() {

    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    var time = new Date();

    this.HoldLoading = true;
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();

    let RejectionModels: RejectionModel[] = [];
    if (this.CurrentTab <= 0) {
      this.ReviewData.forEach(element => {
        let RejectionModel: RejectionModel =
        {
          dateTime: time.toLocaleString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit', day: '2-digit', month: "2-digit", year: 'numeric', hour12: true }),
          forwardedToStation: "Admin",
          id: "",
          row: element,
          stationName: "LogCut",
          tableName: tableName,
          userName: UserName,
          rejectionReasons: []
        };
        RejectionModels.push(RejectionModel);
      });

      this.HoldingService.RejectThisRow(RejectionModels).subscribe(() => {

        this.ReviewData.forEach(element => {

          var ind = this.Data.findIndex(d => d.uniqueId == element.uniqueId);
          this.Data.splice(ind, 1);
          setTimeout(() => {
            this.DataSource.paginator = this.paginator;
            this.UpdateMatTables();
          }, 100);
        });
        this.ReviewData = [];
        this.ReviewDataWithBlindsNumbers = {};

        this.HoldLoading = false;
      });
      this.logcutService.UpdateRows(this.AutoUploadedSelectedRows).subscribe();
          this.AutoUploadedSelectedRows = [];
    }
    else {
      this.UrgentReviewData.forEach(element => {
        let RejectionModel: RejectionModel =
        {
          dateTime: time.toLocaleString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit', day: '2-digit', month: "2-digit", year: 'numeric', hour12: true }),
          forwardedToStation: "Admin",
          id: "",
          row: element,
          stationName: "LogCut",
          tableName: tableName,
          userName: UserName,
          rejectionReasons: []
        };
        RejectionModels.push(RejectionModel);
      });

      this.HoldingService.RejectThisRow(RejectionModels).subscribe(() => {

        this.UrgentReviewData.forEach(element => {

          var ind = this.UrgentData.findIndex(d => d.uniqueId == element.uniqueId);
          this.UrgentData.splice(ind, 1);
          setTimeout(() => {
            this.UrgentdataSource.paginator = this.Urgentpaginator;
            this.UpdateMatTables();
          }, 100);
        });
        this.UrgentReviewData = [];
        this.UrgentReviewDataWithBlindsNumbers = {};

        this.UrgentHoldLoading = false;
      });
      this.logcutService.UpdateRows(this.UrgentAutoUploadedSelectedRows).subscribe();
      this.UrgentAutoUploadedSelectedRows = [];
    }
  }

  GetHeldBasedOnTable() {
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    if (tableName == '-') return;

    this.ButtonIsDisabled = true;




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
          this.ButtonIsDisabled = false;
        }
      );
    }
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
      this.logcutService.GetDataUsingAutoUpload(tableName, UserName, ShiftTable, "Normal").subscribe(data => {

        if (data && data.rows.length != 0 && data.columnNames.length != 0) {

          this.tableModelColNames = data.columnNames;
          this.tableModelColNamesWithActions = [...data.columnNames];
          this.ReviewtableModelColNames = [];
          this.ReviewtableModelColNames.push("Blind Number");
          this.ReviewtableModelColNames = this.ReviewtableModelColNames.concat(data.columnNames);
          this.ReviewtableModelColNamesWithActions = [...this.ReviewtableModelColNames];

          this.tableModelColNamesWithActions.push('SelectColumn')

          this.Data = this.Data.concat(data.rows);
          this.DataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.Data);
          this.ReviewDataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.ReviewData);
          setTimeout(() => {
            this.DataSource.paginator = this.paginator;
            this.UpdateMatTables();
          }, 100);
        }

      });

      // get Urgent
      this.logcutService.GetDataUsingAutoUpload(tableName, UserName, ShiftTable, "Urgent").subscribe(data => {

        if (data && data.rows.length != 0 && data.columnNames.length != 0) {

          this.tableModelColNames = [...data.columnNames];
          this.tableModelColNamesWithActions = [...data.columnNames];
          this.ReviewtableModelColNames = [];
          this.ReviewtableModelColNames.push("Blind Number");
          this.ReviewtableModelColNames = this.ReviewtableModelColNames.concat(data.columnNames);
          this.ReviewtableModelColNamesWithActions = [...this.ReviewtableModelColNames];

          this.tableModelColNamesWithActions.push('SelectColumn')
          this.UrgentData = this.UrgentData.concat(data.rows);
          this.UrgentdataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.UrgentData);
          this.UrgentreviewdataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.UrgentReviewData);

          setTimeout(() => {
            this.UrgentdataSource.paginator = this.Urgentpaginator;
            this.UpdateMatTables();
          }, 100);
        }

      });

    }
    else {

      this.ButtonIsDisabled = true;

      this.FBRservice.GetHeldObjects(tableName).subscribe(
        data => {
          if (data && data.columnNames.length != 0) {

            this.tableModelColNames = data.columnNames;
            this.tableModelColNamesWithActions = [...data.columnNames];
            this.ReviewtableModelColNames = [];
            this.ReviewtableModelColNames.push("Blind Number");
            this.ReviewtableModelColNames = this.ReviewtableModelColNames.concat(data.columnNames);
            this.ReviewtableModelColNamesWithActions = [...this.ReviewtableModelColNames];

            this.tableModelColNamesWithActions.push('SelectColumn')

            this.Data = this.Data.concat(data.rows);
            this.DataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.Data);
            this.ReviewDataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.ReviewData);
            setTimeout(() => {
              this.DataSource.paginator = this.paginator;
              this.UpdateMatTables();
            }, 100);




          }
          this.ButtonIsDisabled = false;
        }

      );

    }

  }


  SelectAll() {
    let Buttons = document.getElementsByClassName("SelectAllTag") as unknown as HTMLButtonElement[];
    let btn = document.getElementById("AllButton");

    if (btn?.textContent?.trim() == 'Select All') {

      btn.textContent = "UnSelect All";
      for (let i = Buttons.length - 1; i >= 0; i--) {
        if (Buttons[i].textContent?.trim() == 'Select') Buttons[i].click();
      }
    }
    else {
      btn ? btn.textContent = "Select All" : null;
      for (let i = Buttons.length - 1; i >= 0; i--) {
        if (Buttons[i].textContent?.trim() == 'UnSelect') Buttons[i].click();
      }
    }

  }

  tabChanged(tabChangeEvent: MatTabChangeEvent): void {

    this.CurrentTab = tabChangeEvent.index;

  }


  InitAllVariables() {
    this.tableModelColNames = [];
    this.ReviewtableModelColNames = [];
    this.BlindNumbers = [];
    this.Data = [];
    this.ReviewData = [];
    this.UrgentData = [];
    this.UrgentReviewDataWithBlindsNumbers = {}
    this.UrgentReviewDataWithBlindsObjects = {}
    this.UrgentReviewData = [];
    this.UrgentAutoUploadedSelectedRows = [];
    this.AutoUploadedSelectedRows = [];
  }

  UpdateMatTables() {
    try {
      this.Datatable.renderRows();
    }
    catch (e) { }

    try {
      this.UrgentDatatable.renderRows();
    }
    catch (e) { }

  }

  PageChange() {
    if (this.CurrentTab <= 0) {
      setTimeout(() => { /// to do the action after the table update as pagechange happens on clicking next or prev
        let Buttons = document.getElementsByClassName("SelectAllTag") as unknown as HTMLButtonElement[];

        for (let i = Buttons.length - 1; i >= 0; i--) {

          let index = ((Buttons[i].id)?.indexOf('_'));
          var ID = Buttons[i].id?.substring(index ? index + 1 : -1);
          let ind: number = (ID != undefined) ? +ID : 0;
          ind += this.paginator.pageIndex * this.paginator.pageSize;

          if (this.SelectedRows[ind != undefined ? this.Data[ind].uniqueId : 'Default'] == 'Selected') Buttons[i].textContent = 'UnSelect';

        }
      }, 10);
    }

    else {
      setTimeout(() => { /// to do the action after the table update as pagechange happens on clicking next or prev
        let Buttons = document.getElementsByClassName("SelectAllTag") as unknown as HTMLButtonElement[];

        for (let i = Buttons.length - 1; i >= 0; i--) {

          let index = ((Buttons[i].id)?.indexOf('_'));
          var ID = Buttons[i].id?.substring(index ? index + 1 : -1);
          let ind: number = (ID != undefined) ? +ID : 0;
          ind += this.paginator.pageIndex * this.paginator.pageSize;

          if (this.SelectedRows[ind != undefined ? this.UrgentData[ind].uniqueId : 'Default'] == 'Selected') Buttons[i].textContent = 'UnSelect';

        }
      }, 10);
    }

  }
}
