import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
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
  ReviewDataWithBlindsNumbers: { [Key: string]: number } = {}
  PrinterTableDictionary = {};
  ngOnInit(): void {
    this.tableModelColNames = [];
    this.ReviewtableModelColNames = [];
    this.BlindNumbers = [];
    this.Data = [];
    this.ReviewData = [];
    this.ReviewDataWithBlindsNumbers = {}
    this.PrinterTableDictionary = {};

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
    this.Refresh();
    setInterval(() => {
      this.Refresh();
    }
      ,
      30000);


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
    this.ezStopService.EzStopSend(
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

  Refresh() {
    this.RefreshLoading = true;

    setTimeout(() => {
      let tableName = (document.getElementById("TableNames") as HTMLSelectElement).value.toString();

      this.ezStopService.GetHeldObjects(tableName).subscribe(data=>{
        
        if (data && data.columnNames.length != 0) {
        
          setTimeout(() => {
            this.updateTable();
          }, 50);
  
          if (data.columnNames.length != 0)
            this.tableModelColNames = data.columnNames
  
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
  
          this.updateTable();
  
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
      
      
    }, 1000);
    this.ezStopService.RefreshEzStopTable().subscribe(data => {

      if (data && data.columnNames.length != 0) {
        
        setTimeout(() => {
          this.updateTable();
        }, 50);

        if (data.columnNames.length != 0)
          this.tableModelColNames = data.columnNames

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

        
        
        
        
        this.updateTable();

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
      this.RefreshLoading = false;
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


      this.ReviewData = [];
      this.ReviewDataWithBlindsNumbers = {};
      this.updateTable();

      this.HoldLoading = false;
    });
  }
  
  SelectAll() {
    let Buttons = document.getElementsByClassName("SelectAllTag") as unknown as HTMLButtonElement[];
    console.log(Buttons.length)
    let btn = document.getElementById("AllButton");

    if (btn?.textContent?.trim() == 'Select All') {

      btn.textContent = "UnSelect All";
      for (let i = Buttons.length-1; i >=0; i--) {
        if (Buttons[i].textContent == 'Select') Buttons[i].click();
      }
    }
    else {
      btn ? btn.textContent = "Select All" : null;
      for (let i = Buttons.length-1; i >=0; i--) {
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
      //this.updateTable();
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
      
    });

  }

}
