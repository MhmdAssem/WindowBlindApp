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


@Component({
  selector: 'app-fabric-cutter',
  templateUrl: './fabric-cutter.component.html',
  styleUrls: ['./fabric-cutter.component.scss']
})


export class FabricCutterComponent implements OnInit, AfterViewInit {

  constructor(private dialog: MatDialog, private FBRservice: FabricCutterService, private settingService: SettingService, private apiService: ApiService, private authService: AuthService) { }

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
  Loading: boolean = false;
  ReviewDataWithBlindsNumbers: { [Key: string]: number } = {}
  ReviewDataWithBlindsObjects: { [Key: string]: FabricCutterCBDetailsModelTableRow } = {}
  newValue = "-1";
  PrinterTableDictionary = {};
  Printing = false;
  Creating = false;
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



  GetCBDetails() {
    let cb = (document.getElementById("CBNumber") as HTMLInputElement).value.trim();
    if (cb == "") { alert("Please enter a valid CB"); return; }
    this.Loading = true;
    this.FBRservice.getCBNumberDetails(cb.toString()).subscribe(data => {
      if (data && data.columnNames.length != 0) {
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
      this.Loading = false;

    });

  }

  SelectThisRow(ind) {
    (document.getElementById("TableViewReview") as HTMLElement).style.display = '';
    this.ReviewDataWithBlindsNumbers[this.Data[ind].uniqueId] = this.Data[ind].blindNumbers.length;
    this.ReviewDataWithBlindsObjects[this.Data[ind].uniqueId] = this.Data[ind];


    if (this.Data[ind].row['Quantity'] != null && this.Data[ind].row['Quantity'] != undefined) this.Data[ind].row['Quantity'] = '1';
    for (let i = 0; i < this.Data[ind].blindNumbers.length; i++) {
      let NewEntry = this.Data[ind];
      NewEntry.row['Blind Number'] = this.Data[ind].blindNumbers[i].toString();
      this.ReviewData.push(NewEntry);
      this.BlindNumbers.push(this.Data[ind].blindNumbers[i]);
    }
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

    setTimeout(() => {

      $("#Custom_Table_Info2").html("");
      $("#Custom_Table_Pagination2").html("");
      $("#DScenario-table_info").appendTo('#Custom_Table_Info2');
      $("#DScenario-table_paginate").appendTo('#Custom_Table_Pagination2');
      (document.getElementById('theSelectColumn2') as HTMLElement).scrollIntoView({
        behavior: 'smooth',
        block: 'start'
      });
    }, 100);
    console.log(this.ReviewData);
  }

  UnSelectThisRow(ind) {

    this.ReviewDataWithBlindsNumbers[this.ReviewData[ind].uniqueId]--;

    if (this.ReviewDataWithBlindsNumbers[this.ReviewData[ind].uniqueId] == 0) {
      this.Data.push(this.ReviewDataWithBlindsObjects[this.ReviewData[ind].uniqueId]);
    }

    let id = this.ReviewData[ind].uniqueId

    this.ReviewData.splice(ind, 1);

    this.updateTable();
    if (this.ReviewDataWithBlindsNumbers[id] == 0)
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

    setTimeout(() => {
      $("#Custom_Table_Pagination2").html("");
      $("#Custom_Table_Info2").html("");
      $("#DScenario-table_paginate").appendTo('#Custom_Table_Pagination2');
      $("#DScenario-table_info").appendTo('#Custom_Table_Info2');
      (document.getElementById('theSelectColumn2') as HTMLElement).scrollIntoView({
        behavior: 'smooth',
        block: 'start'
      });
    }, 100);

    if (this.ReviewData.length == 0)
      (document.getElementById("TableViewReview") as HTMLElement).style.display = 'none';
  }

  PrintLabelsOnly() {
    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    this.Printing = true;
    let Data: FabricCutterCBDetailsModel = {
      columnNames: this.tableModelColNames,
      rows: this.ReviewData
    };
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    this.FBRservice.PrintLabels(
      tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => {
        this.Printing = false;
        this.ReviewData = [];
      });

  }

  CreateFileLabels() {
    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    this.Creating = true;
    let Data: FabricCutterCBDetailsModel = {
      columnNames: this.tableModelColNames,
      rows: this.ReviewData
    };
    let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();
    this.FBRservice.CreateFilesAndLabels(
      tableName, this.PrinterTableDictionary[tableName], UserName, Data).subscribe(() => {
        this.Creating = false;
        this.ReviewData = [];
      });

  }

  ClearReview() {
    this.ReviewData.forEach(element => {
      this.Data.push(element);
    });
    this.ReviewData.splice(0);

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

    setTimeout(() => {
      $("#Custom_Table_Pagination2").html("");
      $("#Custom_Table_Info2").html("");
      $("#DScenario-table_paginate").appendTo('#Custom_Table_Pagination2');
      $("#DScenario-table_info").appendTo('#Custom_Table_Info2');
    }, 100);
    (document.getElementById("TableViewReview") as HTMLElement).style.display = 'none';

  }

  OpenDialog() {
    let diag = this.dialog.open(RollWidthDialogComponent);

    diag.afterClosed().subscribe(res => {
      if (res != null && res != "") {
        this.ReviewData.forEach(element => {
          element.row['Roll Width'] = res;
        });
        this.updateTable();
        setTimeout(() => {
          $("#Custom_Table_Pagination2").html("");
          $("#Custom_Table_Info2").html("");
          $("#DScenario-table_paginate").appendTo('#Custom_Table_Pagination2');
          $("#DScenario-table_info").appendTo('#Custom_Table_Info2');
          (document.getElementById('theSelectColumn2') as HTMLElement).scrollIntoView({
            behavior: 'smooth',
            block: 'start'
          });
        }, 100);
      }
    });
  }
}
