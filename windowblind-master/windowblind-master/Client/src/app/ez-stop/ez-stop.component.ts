import { Component, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatTable, MatTableDataSource } from '@angular/material/table';
import { DataTableDirective } from 'angular-datatables';
import { Console, debug } from 'console';
import { Subject } from 'rxjs';
import { AuthService } from '../auth.service';
import { FabricCutterService } from '../fabric-cutter/fabric-cutter.service';
import { FabricCutterCBDetailsModelTableRow, FabricCutterCBDetailsModel } from '../fabric-cutter/FabricCutterCBDetailsModel';
import { HoldingStationService } from '../holding-station/holding-station.service';
import { RejectionModel } from '../holding-station/RejectionModel';
import { LogCutService } from '../Log-Cut/log-cut.service';
import { SettingService } from '../settings/setting.service';
import { EzStopService } from './ez-stop.service';

@Component({
  selector: 'app-ez-stop',
  templateUrl: './ez-stop.component.html',
  styleUrls: ['./ez-stop.component.scss']
})
export class EzStopComponent implements OnInit {
  HoldLoading: boolean;
  DataInTheTable: any = {};

  constructor(private HoldingService: HoldingStationService, private ezStopService: EzStopService, private logcutService: LogCutService, private FBRservice: FabricCutterService, private settingService: SettingService, private authService: AuthService) { }

  NumberOfTables: number = 0;
  TableNames: string[] = [];


  tableModelColNames: string[] = [];
  ReviewtableModelColNames: string[] = [];
  BlindNumbers: number[] = [];
  Data: FabricCutterCBDetailsModelTableRow[] = [];
  ReviewData: FabricCutterCBDetailsModelTableRow[] = [];
  RefreshLoading: boolean = false;
  SendLoading: boolean = false;
  ReviewDataWithBlindsNumbers: { [Key: string]: number } = {}
  PrinterTableDictionary = {};

  //// for the normal table
  @ViewChild('Datatable', { static: false }) Datatable: MatTable<any>;
  DataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.Data);
  tableModelColNamesWithActions: string[] = [];
  @ViewChild('paginator', { static: false }) paginator: MatPaginator;


  SelectedRows = {};


  ngOnInit(): void {
    this.InitAllVariables();

    this.settingService.getTableNumber('EzStopTable').subscribe(data => {
      if ((data as string).indexOf("@@@@@") != -1) {
        let entries = (data as string).split("#####");

        entries.forEach(element => {

          let data = element.split("@@@@@");
          this.TableNames.push(data[1]);
          this.PrinterTableDictionary[data[1]] = data[0];

        });
      }


    });


    setTimeout(() => {
      this.Refresh();
    }, 100);

    setInterval(() => {
      this.Refresh();
    }
      ,
      180000);


  }


  SelectThisRow(ind) {

    if ((document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent == "Select") {
      (document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent = "UnSelect";


      ind += this.paginator.pageIndex * this.paginator.pageSize;

      this.ReviewDataWithBlindsNumbers[this.Data[ind].uniqueId] = this.ReviewData.length;
      this.ReviewData.push(this.Data[ind]);
      this.SelectedRows[this.Data[ind].uniqueId] = 'Selected';

    }
    else {
      ind += this.paginator.pageIndex * this.paginator.pageSize;
      this.UnSelectThisRow(ind);
      this.SelectedRows[this.Data[ind].uniqueId] = 'UnSelected';
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
    this.ezStopService.EzStopSend(
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
        this.ReviewData = [];
        this.ReviewDataWithBlindsNumbers = {};

      });
  }

  Refresh() {
    this.RefreshLoading = true;

    $("#LoadingDiv").fadeIn();

    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();


    this.ezStopService.GetHeldObjects(tableName).subscribe(data => {

      if (data && data.rows.length != 0 && data.columnNames.length != 0) {
        this.tableModelColNames = data.columnNames;
        this.tableModelColNamesWithActions = [...data.columnNames];
        this.ReviewtableModelColNames = [];
        this.ReviewtableModelColNames.push("Blind Number");
        this.ReviewtableModelColNames = this.ReviewtableModelColNames.concat(data.columnNames);


        this.tableModelColNamesWithActions.push('SelectColumn')
        data.rows.forEach(element => {
          if (this.DataInTheTable[element.uniqueId] == null || this.DataInTheTable[element.uniqueId] == undefined) {
            this.DataInTheTable[element.uniqueId] = true;
            this.Data.push(element);
          }
          else {
            //let ind = this.Data.findIndex(e => e.uniqueId == element.uniqueId);
            //this.Data[ind] = element;
          }
        });

 
        this.DataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.Data);
        setTimeout(() => {
          this.DataSource.paginator = this.paginator;
          this.UpdateMatTables();
        }, 100);
      
      }

      this.ezStopService.RefreshEzStopTable().subscribe(data => {

        if (data && data.rows.length != 0 && data.columnNames.length != 0) {

          this.tableModelColNames = data.columnNames;
          this.tableModelColNamesWithActions = [...data.columnNames];
          this.ReviewtableModelColNames = [];
          this.ReviewtableModelColNames.push("Blind Number");
          this.ReviewtableModelColNames = this.ReviewtableModelColNames.concat(data.columnNames);


          this.tableModelColNamesWithActions.push('SelectColumn');


          data.rows.forEach(element => {
            if (this.DataInTheTable[element.uniqueId] == null || this.DataInTheTable[element.uniqueId] == undefined) {
              this.DataInTheTable[element.uniqueId] = true;
              this.Data.push(element);
            }
            else {
              //let ind = this.Data.findIndex(e => e.uniqueId == element.uniqueId);
              //this.Data[ind] = element;
            }
          });

          this.DataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.Data);


          setTimeout(() => {
            this.DataSource.paginator = this.paginator;
            this.UpdateMatTables();
          }, 100);

        }
        if (this.Data.length == 0)
          alert("No data found !");
        this.RefreshLoading = false;
        $("#LoadingDiv").fadeOut();
      });
    });



  }


  Hold() {

    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    var time = new Date();

    this.HoldLoading = true;
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();

    let RejectionModels: RejectionModel[] = [];
    this.ReviewData.forEach(element => {
      //alert(element.uniqueId)
      let RejectionModel: RejectionModel =
      {
        dateTime: time.toLocaleString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit', day: '2-digit', month: "2-digit", year: 'numeric', hour12: true }),
        forwardedToStation: "Admin",
        id: "",
        row: element,
        stationName: "EzStop",
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

      });

      this.DataSource = new MatTableDataSource(this.Data);
      setTimeout(() => {
        this.DataSource.paginator = this.paginator;
        this.UpdateMatTables();
      }, 100);

      this.ReviewData = [];
      this.ReviewDataWithBlindsNumbers = {};

      this.HoldLoading = false;
    });
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
      for (let i = Buttons.length - 1; i >= 0; i--) {
        if (Buttons[i].textContent == 'UnSelect') Buttons[i].click();
      }
    }

  }


  Delete() {

    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    if (tableName == 'DefaultTableName') {
      alert("Please Choose a valid Table Name");
      return;
    }



    this.ReviewData.forEach(element => {
      let ind = this.Data.findIndex(e => e.uniqueId == element.uniqueId);
      this.Data.splice(ind, 1);

    });

    let Model: FabricCutterCBDetailsModel = {
      columnNames: this.tableModelColNames,
      rows: this.ReviewData
    };

    this.ezStopService.ClearOrdersFromEzStop(Model, UserName, tableName).subscribe(res => {
      this.ReviewData = [];
      this.ReviewDataWithBlindsNumbers = {};
      
      this.DataSource = new MatTableDataSource(this.Data);
      setTimeout(() => {
        this.DataSource.paginator = this.paginator;
        this.UpdateMatTables();
      }, 100);

    });

  }

  InitAllVariables() {
    this.tableModelColNames = [];
    this.ReviewtableModelColNames = [];
    this.BlindNumbers = [];
    this.Data = [];
    this.ReviewData = [];
    this.ReviewDataWithBlindsNumbers = {}
    this.PrinterTableDictionary = {};
    this.SelectedRows = {};

  }

  UpdateMatTables() {
    try {
      this.Datatable.renderRows();
    }
    catch (e) { }

  }

  PageChange() {
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
}
