import { R3FactoryTarget } from '@angular/compiler';
import { splitAtColon } from '@angular/compiler/src/util';
import { Component, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatOption } from '@angular/material/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSelect } from '@angular/material/select';
import { MatTable, MatTableDataSource } from '@angular/material/table';
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
  OriginalData: RejectionModel[] = [];
  ReviewData: RejectionModel[] = [];
  RefreshLoading: boolean = false;
  SendLoading: boolean = false;
  ReviewDataWithBlindsNumbers: { [Key: string]: number } = {}
  PrinterTableDictionary = {};
  hideRows: boolean[] = [];


  @ViewChild('Datatable', { static: false }) Datatable: MatTable<any>;
  DataSource = new MatTableDataSource<RejectionModel>(this.Data);
  tableModelColNamesWithActions: string[] = [];
  @ViewChild('paginator', { static: false }) paginator: MatPaginator;

  SelectedRows = {};



  ngOnInit(): void {


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

      this.InitAllVariables();

      this.tableModelColNamesWithActions.push('CB Number');
      this.tableModelColNamesWithActions.push('Line No');
      this.tableModelColNamesWithActions.push('UserName');
      this.tableModelColNamesWithActions.push('TableName');
      this.tableModelColNamesWithActions.push('DateTime');
      this.tableModelColNamesWithActions.push('StationName');
      this.tableModelColNamesWithActions.push('Select Reasons');
      this.tableModelColNamesWithActions.push('Admin Notes');
      this.tableModelColNamesWithActions.push('SelectColumn');
      this.Data = data;
      this.DataSource = new MatTableDataSource<RejectionModel>(this.Data);

      setTimeout(() => {
        this.DataSource.paginator = this.paginator;
        this.UpdateMatTables();
      }, 100);


    });

    /// getting the Reasons
    this.GettingRejectionReasons();
  }

  GettingRejectionReasons() {
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
          let FirstTime = true;
          for (let j = 0; j < this.Data[i].rejectionReasons.length; j++) {
            (document.getElementById(this.Data[i].rejectionReasons[j] + "__" + i) as HTMLInputElement).checked = true;
            let Span = document.getElementById("anchor_" + i) as HTMLSpanElement;
            if (FirstTime)
              Span.textContent = '';
            if (Span.textContent == '')
              Span.textContent += this.Data[i].rejectionReasons[j].trim()
            else
              Span.textContent += "," + this.Data[i].rejectionReasons[j].trim();
            FirstTime = false;
          }
        }

      }, 50);
    });
  }

  SelectThisRow(ind) {

    if ((document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent == "Select") {
      (document.getElementById('SelectCol_' + ind) as HTMLButtonElement).textContent = "UnSelect";
      ind += this.paginator.pageIndex * this.paginator.pageSize;
      this.ReviewDataWithBlindsNumbers[this.Data[ind].id] = this.ReviewData.length;
      this.ReviewData.push(this.Data[ind]);
      this.SelectedRows[this.Data[ind].row.uniqueId] = 'Selected';
    }
    else {
      ind += this.paginator.pageIndex * this.paginator.pageSize;
      this.UnSelectThisRow(ind);
      this.SelectedRows[this.Data[ind].row.uniqueId] = 'UnSelected';
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
      this.DataSource = new MatTableDataSource<RejectionModel>(this.Data);

      setTimeout(() => {
        this.DataSource.paginator = this.paginator;
        this.UpdateMatTables();
        this.GettingRejectionReasons();
      }, 100);

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
    //console.log(i);
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

  FilterTableBasedOnReasons(Reason: string) {
    let checked = (document.getElementById(Reason + "__ReasonFilter") as HTMLInputElement).checked;
    let Span = document.getElementById("ReasonFilteranchor_ReasonFilter") as HTMLSpanElement;
    Reason = Reason.trim();
    let ListOfReasons = [];
    this.OriginalData = [...this.Data];
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
          let INDEX = this.OriginalData.findIndex(e => e.row.uniqueId == element.row.uniqueId);
          this.OriginalData.splice(INDEX, 1);
        }

      }
      else {
        if (found || Span.textContent == "Filter") { }
        else {
          let INDEX = this.OriginalData.findIndex(e => e.row.uniqueId == element.row.uniqueId);
          this.OriginalData.splice(INDEX, 1);
        }
      }

      cntr++;
    });

    this.DataSource = new MatTableDataSource<RejectionModel>(this.OriginalData);

    setTimeout(() => {
      this.DataSource.paginator = this.paginator;
      this.UpdateMatTables();
      this.GettingRejectionReasons();
    }, 100);
  }

  Delete() {

    let UserName: any = localStorage.getItem('UserName') != null ? localStorage.getItem('UserName')?.toString() : "";

    this.HoldingService.ClearOrdersFromHoldingStation(this.ReviewData, UserName).subscribe(res => {
      this.ReviewData.forEach(element => {
        let ind = this.Data.findIndex(e => e.id == element.id);
        this.Data.splice(ind, 1);
        this.DataSource = new MatTableDataSource<RejectionModel>(this.Data);

        setTimeout(() => {
          this.DataSource.paginator = this.paginator;
          this.UpdateMatTables();
          this.GettingRejectionReasons();
        }, 100);
      });


    });

  }

  InitAllVariables() {
    this.tableModelColNames = [];
    this.ReviewtableModelColNames = [];
    this.BlindNumbers = [];
    this.Data = [];
    this.ReviewData = [];
    this.tableModelColNamesWithActions = [];
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

        if (this.SelectedRows[ind != undefined ? this.Data[ind].id : 'Default'] == 'Selected') Buttons[i].textContent = 'UnSelect';

      }
    }, 10);


  }

  AppendOnly(i) {
    let textArea = (document.getElementById('Admin_Notes_' + i) as HTMLTextAreaElement).value;
    i += this.paginator.pageIndex * this.paginator.pageSize;

    var OldText = this.Data[i].row.row['Admin_Notes'];
    let ind = textArea.indexOf(OldText);
    if (ind != -1 && OldText != textArea) {

    }
    else if (ind == -1 || OldText == textArea) {
      console.log(OldText);
      (document.getElementById('Admin_Notes_' + i) as HTMLTextAreaElement).value = OldText + " ";
    }

  }

  SaveAdminNotes(i) {
    let textArea = (document.getElementById('Admin_Notes_' + i) as HTMLTextAreaElement).value;
    textArea = textArea.replace(/  +/g, ' ');
    i += this.paginator.pageIndex * this.paginator.pageSize;
    let temp = this.Data[i];
    temp.row.row['Admin_Notes'] = textArea;

    let Model: RejectionModel = {
      id: temp.id,
      userName: temp.userName,
      tableName: temp.tableName,
      stationName: temp.stationName,
      row: temp.row,
      rejectionReasons: temp.rejectionReasons,
      forwardedToStation: temp.forwardedToStation,
      dateTime: temp.dateTime
    };

    console.log(Model)
    this.HoldingService.SaveAdminNotes(Model).subscribe(data => {

    });

  }
}
