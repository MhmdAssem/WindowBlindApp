import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
import { faTintSlash } from '@fortawesome/free-solid-svg-icons';
import { DataTableDirective } from 'angular-datatables';
import { data } from 'jquery';
import { Subject } from 'rxjs';
import { AuthService } from '../auth.service';
import { FabricCutterCBDetailsModelTableRow, FabricCutterCBDetailsModel } from '../fabric-cutter/FabricCutterCBDetailsModel';
import { PackingStationService } from '../packing-station/packing-station.service';
import { SettingService } from '../settings/setting.service';
import { HoldingStationService } from './holding-station.service';
import { OrdersApprovalModel } from './OrdersApprovalModel';
import { RejectionModel } from './RejectionModel';

@Component({
  selector: 'app-holding-station',
  templateUrl: './holding-station.component.html',
  styleUrls: ['./holding-station.component.scss']
})
export class HoldingStationComponent implements OnInit {
  LineLoading: boolean;
  HoldLoading: boolean = false;
  CBLoading: boolean;
  DataInTheTable: any = {};
  constructor(private HoldingService: HoldingStationService, private PackingService: PackingStationService, private settingService: SettingService, private authService: AuthService) { }

  NumberOfTables: number = 0;
  StationName: string[] = [];
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
  Data: RejectionModel[] = [];
  ReviewData: RejectionModel[] = [];
  RefreshLoading: boolean = false;
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

    //#region applications

    this.StationName.push('Fabric');
    this.StationName.push('LogCut');
    this.StationName.push('EzStop');
    this.StationName.push('Assembly');
    this.StationName.push('Hoist');
    this.StationName.push('Packing');

    //#endregion

    this.HoldingService.GetAllRejectedOrders().subscribe(data => {
      this.Data = data;
      console.log(data);
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


  SelectThisRow(ind) {

    if ((document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent == "Select") {
      (document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent = "UnSelect";

      this.ReviewDataWithBlindsNumbers[this.Data[ind].id] = this.ReviewData.length;
      this.ReviewData.push(this.Data[ind]);
    }
    else {
      this.UnSelectThisRow(ind);
    }
  }

  UnSelectThisRow(ind) {

    (document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent = "Select";

    this.ReviewData.splice(this.ReviewDataWithBlindsNumbers[this.Data[ind].id], 1);
    this.ReviewDataWithBlindsNumbers[this.Data[ind].id] = -1;

  }

  Send() {
    this.SendLoading = true;
    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";

    let ForwardStation = (document.getElementById("ForwardStation") as HTMLSelectElement).value;
    if (ForwardStation == '...') {
      alert("Please Choose a station !!");
      return;
    }
    let TableName = (document.getElementById("ForwardTable") as HTMLSelectElement).value;
    
    if (TableName == '...') {
      alert("Please Choose a Table !!");
      return;
    }
    this.ReviewData.forEach(element => {
    element.tableName = TableName;
    });
    let Data: OrdersApprovalModel = {
      forwardStation: ForwardStation,
      data: this.ReviewData
    };

    this.HoldingService.ApproveThisOrders(Data).subscribe(res => {

      this.ReviewData = [];
      let keys = Object.keys(this.ReviewDataWithBlindsNumbers);

      keys.forEach(key => {
        let ind = this.Data.findIndex(d => d.id == key);
        if (ind != -1 && this.ReviewDataWithBlindsNumbers[key] != -1) {
          this.DataInTheTable[this.Data[ind]['Line No']] = null;
          this.Data.splice(ind, 1);
        }
      });

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
      }, 500);

      this.SendLoading = false;
    });

  }

  GetTablesBasedOnStation() {
    let ForwardStation = (document.getElementById("ForwardStation") as HTMLSelectElement).value;
    
    this.settingService.GetTablesBasedOnApplication(ForwardStation).subscribe(res=>{
      this.TableNames = [];
      res.forEach(element => {
        this.TableNames.push(element);
      });
    });
    
    
    
  }

}
