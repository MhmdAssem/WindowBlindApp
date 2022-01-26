import { R3FactoryTarget } from '@angular/compiler';
import { splitAtColon } from '@angular/compiler/src/util';
import { Component, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatOption } from '@angular/material/core';
import { MatSelect } from '@angular/material/select';
import { faThumbsDown, faTintSlash } from '@fortawesome/free-solid-svg-icons';
import { DataTableDirective } from 'angular-datatables';
import { rejects } from 'assert';
import { data, noConflict } from 'jquery';
import { Subject } from 'rxjs';
import { textChangeRangeIsUnchanged } from 'typescript';
import { AuthService } from '../auth.service';
import { FabricCutterCBDetailsModelTableRow, FabricCutterCBDetailsModel } from '../fabric-cutter/FabricCutterCBDetailsModel';
import { PackingStationService } from '../packing-station/packing-station.service';
import { SettingService } from '../settings/setting.service';
import { HoldingStationService } from './holding-station.service';
import { OrdersApprovalModel } from './OrdersApprovalModel';
import { ReasonModel } from './ReasonModel';
import { RejectionModel } from './RejectionModel';

@Component({
  selector: 'app-holding-station',
  templateUrl: './holding-station.component.html',
  styleUrls: ['./holding-station.component.scss']
})
export class HoldingStationComponent implements OnInit {

  ReasonsSelect: FormControl[];


  ReasonsList: string[] = [];


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
  hideRows: boolean[] = [];

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
    this.hideRows = [];
    this.StationName.push('FabricCut');
    this.StationName.push('LogCut');
    this.StationName.push('EzStop');
    this.StationName.push('Assembly');
    this.StationName.push('Hoist');
    this.StationName.push('Packing');

    //#endregion

    this.HoldingService.GetAllRejectedOrders().subscribe(data => {
      this.Data = data;

    });

    /// getting the Reasons
    this.settingService.getSettings().subscribe(data => {

      let CommentsIndex = data.findIndex(file => file.settingName == 'Comments');
      this.ReasonsList = data[CommentsIndex].settingPath.split("@@@");
      this.ReasonsSelect = [];
      for (let i = 0; i < data.length; i++)
        this.ReasonsSelect.push(new FormControl());

      setTimeout(() => {
        /// selecting already checked before reasons
        var checkList = document.getElementsByClassName("dropdown-check-list");

        for (let i = 0; i < checkList.length; i++) {
          //  alert(document.getElementById('anchor_' + i))
          document.getElementById('anchor_' + i)?.addEventListener("click", function () {
            if (document.getElementById('items_' + i)?.style.display == 'none')
              (document.getElementById('items_' + i) as HTMLElement).style.display = ''
            else
              (document.getElementById('items_' + i) as HTMLElement).style.display = 'none'
          });
        }


        /// For the Filter
        document.getElementById('ReasonFilteranchor_ReasonFilter')?.addEventListener("click", function () {
          if (document.getElementById('ReasonFilteritems_ReasonFilter')?.style.display == 'none')
            (document.getElementById('ReasonFilteritems_ReasonFilter') as HTMLElement).style.display = ''
          else
            (document.getElementById('ReasonFilteritems_ReasonFilter') as HTMLElement).style.display = 'none'
        });
        for (let i = 0; i < this.Data.length; i++) {
          this.hideRows.push(false);
          for (let j = 0; j < this.Data[i].rejectionReasons.length; j++) {
            (document.getElementById(this.Data[i].rejectionReasons[j] + "__" + i) as HTMLInputElement).checked = true;
            let Span = document.getElementById("anchor_" + i) as HTMLSpanElement;
            if (Span.textContent == 'Select Reasons')
              Span.textContent = '';
            if (Span.textContent == '')
              Span.textContent += this.Data[i].rejectionReasons[j].trim()
            else
              Span.textContent += "," + this.Data[i].rejectionReasons[j].trim();

          }
        }

      }, 50);
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

    this.settingService.GetTablesBasedOnApplication(ForwardStation).subscribe(res => {
      this.TableNames = [];
      res.forEach(element => {
        this.TableNames.push(element);
      });
    });



  }

  UpdateReasons(reason: string, i) {
    i = +this.Data.findIndex(e => e.row.uniqueId == i);
    let OrderId = this.Data[i].id;
    reason = reason.trim();
    let model: ReasonModel = {
      orderid: OrderId,
      reason: reason,
      addOrRemove: (document.getElementById(reason + "__" + i) as HTMLInputElement).checked,
      originalStation: this.Data[i].stationName
    };

    let Span = document.getElementById("anchor_" + i) as HTMLSpanElement;
    if (model.addOrRemove) {
      if (Span.textContent == 'Select Reasons')
        Span.textContent = '';
      if (Span.textContent == '')
        Span.textContent += reason.trim()
      else
        Span.textContent += "," + reason.trim();

      this.Data[i].rejectionReasons.push(reason);
    }
    else {
      Span.textContent = Span.textContent ? Span.textContent.replace(reason.trim(), '') : '';
      Span.textContent = Span.textContent.replace(',,', ',');
      Span.textContent = Span.textContent.trim();
      if (Span.textContent.startsWith(','))
        Span.textContent = Span.textContent.substring(1);


      if (Span.textContent[Span.textContent.length - 1] == ',')
        Span.textContent = Span.textContent.substring(0, Span.textContent.length - 1);

      if (Span.textContent == '')
        Span.textContent = "Select Reasons";

      this.Data[i].rejectionReasons.splice(this.Data[i].rejectionReasons.findIndex(e => e == reason), 1);
    }

    this.HoldingService.UpdateReasonsForHeldObject(model).subscribe(data => { });

  }


  wheelHandler: any;

  FilterTableBasedOnReasons(Reason: string) {
    let checked = (document.getElementById(Reason + "__ReasonFilter") as HTMLInputElement).checked;
    let Span = document.getElementById("ReasonFilteranchor_ReasonFilter") as HTMLSpanElement;
    Reason = Reason.trim();
    let ListOfReasons = [];
    if (checked) {
      if (Span.textContent == 'Filter')
        Span.textContent = '';
      if (Span.textContent == '')
        Span.textContent += Reason.trim()
      else
        Span.textContent += "," + Reason.trim();
      ListOfReasons = Span.textContent?.split(",") as [];

    }
    else {
      Span.textContent = Span.textContent ? Span.textContent.replace(Reason.trim(), '') : '';
      Span.textContent = Span.textContent.replace(',,', ',');
      Span.textContent = Span.textContent.trim();
      if (Span.textContent.startsWith(','))
        Span.textContent = Span.textContent.substring(1);
      if (Span.textContent[Span.textContent.length - 1] == ',')
        Span.textContent = Span.textContent.substring(0, Span.textContent.length - 1);
      ListOfReasons = Span.textContent.split(",") as [];
      if (Span.textContent == '')
        Span.textContent = "Filter";
    }

    let cntr = 0;
    this.Data.forEach(element => {
      let found = false;
      ListOfReasons.forEach(res => {
        let ind = element.rejectionReasons.findIndex(e => e == res);
        found = found || ind != -1;

      });

      if (checked) {
        if (!found) {
          this.hideRows[cntr] = true;
        }
        else
          this.hideRows[cntr] = false;
      }
      else {
        if (found || Span.textContent == "Filter") {
          this.hideRows[cntr] = false;
        }
        else
          this.hideRows[cntr] = true;
      }

      cntr++;
    });


  }


  Delete() {

    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";
    
    this.HoldingService.ClearOrdersFromHoldingStation(this.ReviewData, UserName).subscribe(res => {
      this.ReviewData.forEach(element => {
        let ind = this.Data.findIndex(e => e.id == element.id);
        this.Data.splice(ind, 1);

      });

      this.updateTable();

    });

  }

}
