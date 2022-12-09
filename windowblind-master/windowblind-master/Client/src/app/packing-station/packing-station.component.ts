import { Component, DebugElement, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatTable, MatTableDataSource } from '@angular/material/table';
import { DataTableDirective } from 'angular-datatables';
import { element } from 'protractor';
import { Subject } from 'rxjs';
import { count } from 'rxjs/operators';
import { AuthService } from '../auth.service';
import { FabricCutterService } from '../fabric-cutter/fabric-cutter.service';
import { FabricCutterCBDetailsModelTableRow, FabricCutterCBDetailsModel } from '../fabric-cutter/FabricCutterCBDetailsModel';
import { HiostStationService } from '../hoist-station/hiost-station.service';
import { HoldingStationService } from '../holding-station/holding-station.service';
import { RejectionModel } from '../holding-station/RejectionModel';
import { LogCutService } from '../Log-Cut/log-cut.service';
import { SettingService } from '../settings/setting.service';
import { PackingStationService } from './packing-station.service';

@Component({
  selector: 'app-packing-station',
  templateUrl: './packing-station.component.html',
  styleUrls: ['./packing-station.component.scss']
})
export class PackingStationComponent implements OnInit {
  LineLoading: boolean;
  PrintLoading: boolean;
  HoldLoading: boolean = false;
  CBLoading: boolean;
  DataInTheTable: any = {};
  constructor(private HoldingService: HoldingStationService, private PackingService: PackingStationService, private settingService: SettingService, private authService: AuthService) { }

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
  PrinterTableDictionary2nd = {};

  SelectedRows = {};


  //// for the normal table
  @ViewChild('Datatable', { static: false }) Datatable: MatTable<any>;
  DataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.Data);
  tableModelColNamesWithActions: string[] = [];
  @ViewChild('paginator', { static: false }) paginator: MatPaginator;


  ngOnInit(): void {

    this.PrinterTableDictionary = {};
    this.PrinterTableDictionary2nd = {};

    this.settingService.getTableNumber('PackingStationTable').subscribe(data => {
      if ((data as string).indexOf("@@@@@") != -1) {
        let entries = (data as string).split("#####");

        entries.forEach(element => {

          let data = element.split("@@@@@");

          this.TableNames.push(data[1]);
          this.PrinterTableDictionary[data[1]] = data[0];
          this.PrinterTableDictionary2nd[data[1]] = data[3];

        });
        console.log(this.PrinterTableDictionary2nd);
      }


    });



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
      this.UnSelectThisRow(ind);
      ind += this.paginator.pageIndex * this.paginator.pageSize;
      this.SelectedRows[this.Data[ind].uniqueId] = 'UnSelected';
    }
  }

  UnSelectThisRow(ind) {

    (document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent = "Select";
    ind += this.paginator.pageIndex * this.paginator.pageSize;
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
    if (tableName == '-') {
      alert("Please select a table name");
      return;
    }

    this.PackingService.pushLinesNoToHoistStation(
      tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => {
        let keys = Object.keys(this.ReviewDataWithBlindsNumbers);

        keys.forEach(key => {
          let ind = this.Data.findIndex(d => d.uniqueId == key);
          if (ind != -1 && this.ReviewDataWithBlindsNumbers[key] != -1) {
            this.DataInTheTable[this.Data[ind]['Line No']] = null;
            this.Data.splice(ind, 1);
          }
        });
        this.SendLoading = false;
        this.ReviewData = [];
        this.ReviewDataWithBlindsNumbers = {};
        this.DataSource = new MatTableDataSource(this.Data);
        setTimeout(() => {
          this.DataSource.paginator = this.paginator;
          this.UpdateMatTables();
        }, 100);

      });
  }

  GetCBDetails() {
    let input = (document.getElementById("CBNumber") as HTMLInputElement).value.trim();
    this.CBLoading = true;

    this.PackingService.GetReadyToPack(input).subscribe(data => {

      if (data && data.rows.length != 0 && data.columnNames.length != 0) {

        this.tableModelColNames = data.columnNames;
        this.tableModelColNamesWithActions = [...data.columnNames];
        this.ReviewtableModelColNames = [];
        this.ReviewtableModelColNames.push("Blind Number");
        this.ReviewtableModelColNames = this.ReviewtableModelColNames.concat(data.columnNames);

        this.tableModelColNamesWithActions.push('SelectColumn')

        let OrdersByCBNumbers = {};
        let PackedOrdersByCBNumbers = {};
        let CountOfOrdersByCBNumber = {};
        this.Data = [];
        data.rows.forEach(element => {
          /// getting the data without any duplicates
          if (this.DataInTheTable[element.uniqueId] == null || this.DataInTheTable[element.uniqueId] == undefined) {
            this.DataInTheTable[element.uniqueId] = true;
            this.Data.push(element);
          }
          else {
            let ind = this.Data.findIndex(e => e.uniqueId == element.uniqueId);
            this.Data[ind] = element;
          }
        });
        /// know each Total and packed orders for the same CB number
        this.Data.forEach(element => {
          if (element.row['Department'] != '') {
            OrdersByCBNumbers[element.row['CB Number']] = element.row['Total'];


            PackedOrdersByCBNumbers[element.row['CB Number']] = element.row['Packed'];

            if (CountOfOrdersByCBNumber[element.row['CB Number']] == null)
              CountOfOrdersByCBNumber[element.row['CB Number']] = 0;

            CountOfOrdersByCBNumber[element.row['CB Number']]++;
          }
        });

        this.Data.forEach(element => {

          let Total: number = +OrdersByCBNumbers[element.row['CB Number']];
          let Packed: number = +PackedOrdersByCBNumbers[element.row['CB Number']];
          let Count: number = +CountOfOrdersByCBNumber[element.row['CB Number']];

          console.log("Total: " + Total.toString());
          console.log("Packed: " + Packed.toString());
          console.log("Count: " + Count.toString());
          if (element.row['Department'] == '') element.row['status'] = 'NONE';
          else if (Count >= Total) element.row['Status'] = 'Dispatch';
          else if (Count + Packed >= Total) element.row['Status'] = 'Dispatch via holding bay';
          else element.row['Status'] = 'Holding bay';

        });
        this.DataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.Data);

        setTimeout(() => {
          this.DataSource.paginator = this.paginator;
          this.UpdateMatTables();
        }, 100);

      }
      if (this.Data.length == 0)
        alert("This CB or Line number is not found !");
      this.CBLoading = false;
      (document.getElementById("CBNumber") as HTMLInputElement).value = "";
    });
  }


  Hold() {

    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    var time = new Date();

    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();

    let RejectionModels: RejectionModel[] = [];
    this.ReviewData.forEach(element => {
      let RejectionModel: RejectionModel =
      {
        dateTime: time.toLocaleString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit', day: '2-digit', month: "2-digit", year: 'numeric', hour12: true }),
        forwardedToStation: "Admin",
        id: "",
        row: element,
        stationName: "Packing",
        tableName: tableName,
        userName: UserName,
        rejectionReasons: []
      };
      RejectionModels.push(RejectionModel);
    });



    this.HoldingService.RejectThisRow(RejectionModels).subscribe(() => {
      let keys = Object.keys(this.ReviewDataWithBlindsNumbers);

      keys.forEach(key => {
        let ind = this.Data.findIndex(d => d.uniqueId == key);
        if (ind != -1 && this.ReviewDataWithBlindsNumbers[key] != -1) {
          this.DataInTheTable[this.Data[ind]['Line No']] = null;
          this.Data.splice(ind, 1);
        }
      });

      this.SendLoading = false;
      this.ReviewData = [];
      this.ReviewDataWithBlindsNumbers = {};
      this.DataSource = new MatTableDataSource(this.Data);
      setTimeout(() => {
        this.DataSource.paginator = this.paginator;
        this.UpdateMatTables();
      }, 100);


    });
  }

  GetHeldBasedOnTable() {
    this.InitAllVariables();

    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    this.PackingService.GetHeldObjects(tableName).subscribe(
      data => {
        if (data && data.columnNames.length != 0) {

          this.tableModelColNames = data.columnNames;
          this.tableModelColNamesWithActions = [...data.columnNames];
          this.ReviewtableModelColNames = [];
          this.ReviewtableModelColNames.push("Blind Number");
          this.ReviewtableModelColNames = this.ReviewtableModelColNames.concat(data.columnNames);

          this.tableModelColNamesWithActions.push('SelectColumn')
          data.rows.forEach(element => {

            if (this.DataInTheTable[element.uniqueId] == null || this.DataInTheTable[element.uniqueId] == undefined) {
              this.DataInTheTable[element.uniqueId] = true;
              this.Data.unshift(element);
            }
          });

          this.DataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.Data);

          setTimeout(() => {
            this.DataSource.paginator = this.paginator;
            this.UpdateMatTables();
          }, 100);

        }
      }
    );
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

    this.PackingService.ClearOrdersFromPacking(Model, UserName, tableName).subscribe(res => {
      this.ReviewData = [];
      this.ReviewDataWithBlindsNumbers = {};
      this.DataSource = new MatTableDataSource<FabricCutterCBDetailsModelTableRow>(this.Data);

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

  Print() {
    this.PrintLoading = true;
    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";


    let Data: FabricCutterCBDetailsModel = {
      columnNames: this.tableModelColNames,
      rows: this.ReviewData
    };
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    this.PackingService.PackingSend(
      tableName, this.PrinterTableDictionary[tableName], this.PrinterTableDictionary2nd[tableName], UserName, Data).subscribe(() => {
        this.PrintLoading = false;


        this.DataSource = new MatTableDataSource(this.Data);
        setTimeout(() => {
          this.DataSource.paginator = this.paginator;
          this.UpdateMatTables();
        }, 100);
        //this.ReviewData = [];
        //this.ReviewDataWithBlindsNumbers = {};
      });


  }

}
