import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
import { MatGridTileHeaderCssMatStyler } from '@angular/material/grid-list';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DataTableDirective } from 'angular-datatables';
import { Subject } from 'rxjs';
import { ApiService } from '../api.service';
import { FabricCutterSettings } from '../models/fabricCutterSettings';
import { FileSettings } from '../models/FileSettings';
import { Station } from '../models/station';
import { StationListSelection } from '../models/stationListSelection';
import { Table } from '../models/table';
import { SettingService } from './setting.service';
import { TablePrinterModel } from './TablePrinterModel';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {

  constructor(private settingService: SettingService, private apiService: ApiService, private snackBar: MatSnackBar) { }
  private selection = new StationListSelection();


  ListOfFiles: FileSettings[];
  SettingName: string;
  SettingPath: string;
  Saving: boolean = false;

  ColumnNames = [];
  ColumnNamesApplications = [];
  PrinterNames = [];
  FabricSelectedColumnNames: string[];
  LogCutSelectedColumnNames: string[];
  EzStopSelectedColumnNames: string[];
  FabricSelectedColumnNamesId: string;
  LogCutSelectedColumnNamesId: string;
  EzStopSelectedColumnNamesId: string;
  FabricCutterTableNumberId: string;
  LogCutterTableNumberId: string;
  EzStopTableNumberId: string;

  SelectedPrinter = "";
  SelectedPrinterId = "";

  Columns: string[] = []; //for the printe tables
  PrinterTableArray: TablePrinterModel[] = [];

  @ViewChildren(DataTableDirective)
  dtElements: QueryList<DataTableDirective>;
  dtTrigger: Subject<any> = new Subject();
  dtOptions: DataTables.Settings = {};


  ngOnInit(): void {
    this.FabricSelectedColumnNames = [];
    this.LogCutSelectedColumnNames = [];
    this.EzStopSelectedColumnNames = [];
    this.dtOptions = {
      pagingType: 'full_numbers',
      lengthChange: false,
      searching: false,
      destroy: true,
      ordering: false,
      pageLength: 4,

    };

    /// load all settings From BackEnd
    this.settingService.getSettings().subscribe(data => {
      console.log(data);
      this.ListOfFiles = data;
      console.log(data);

      let indFabric = this.ListOfFiles.findIndex(e => e.settingName == "SelectedColumnsNames" && e.applicationSetting == "FabricCutter");
     

      if (this.ListOfFiles[indFabric].settingPath != "")
        this.FabricSelectedColumnNames = (this.ListOfFiles[indFabric].settingPath.split("@@@") as []);

      this.FabricSelectedColumnNamesId = this.ListOfFiles[indFabric].id;
      this.ListOfFiles.splice(indFabric, 1);
      let indLogCut = this.ListOfFiles.findIndex(e => e.settingName == "SelectedColumnsNames" && e.applicationSetting == "LogCut");

      if (this.ListOfFiles[indLogCut].settingPath != "")
        this.LogCutSelectedColumnNames = (this.ListOfFiles[indLogCut].settingPath.split("@@@") as []);

      this.LogCutSelectedColumnNamesId = this.ListOfFiles[indLogCut].id;
      this.ListOfFiles.splice(indLogCut, 1);

      let indEzStop = this.ListOfFiles.findIndex(e => e.settingName == "SelectedColumnsNames" && e.applicationSetting == "EzStop");

      if (this.ListOfFiles[indEzStop].settingPath != "")
        this.EzStopSelectedColumnNames = (this.ListOfFiles[indEzStop].settingPath.split("@@@") as []);

      this.EzStopSelectedColumnNamesId = this.ListOfFiles[indEzStop].id;
      this.ListOfFiles.splice(indEzStop, 1);


      let ind = this.ListOfFiles.findIndex(e => e.settingName == "FabricCutterTable");
      this.FabricCutterTableNumberId = this.ListOfFiles[ind].id;
      let FabricCutterTables = this.ListOfFiles[ind].settingPath;
      this.ListOfFiles.splice(ind, 1);

      ind = this.ListOfFiles.findIndex(e => e.settingName == "LogCutterTable");
      this.LogCutterTableNumberId = this.ListOfFiles[ind].id;
      let LogCutterTables = this.ListOfFiles[ind].settingPath;
      this.ListOfFiles.splice(ind, 1);

      ind = this.ListOfFiles.findIndex(e => e.settingName == "EzStopTable");
      this.EzStopTableNumberId = this.ListOfFiles[ind].id;
      let EzStopTables = this.ListOfFiles[ind].settingPath;
      this.ListOfFiles.splice(ind, 1);

      if (FabricCutterTables.indexOf("@@@@@") != -1) {
        let FabricCutterEntries = FabricCutterTables.split("#####");
        FabricCutterEntries.forEach(element => {
          let entry = element.split("@@@@@");
          let model: TablePrinterModel =
          {
            applicationName: "Fabric Cutter",
            printerName: entry[0],
            tableName: entry[1]
          };
          this.PrinterTableArray.push(model);
        });
      }

      if (LogCutterTables.indexOf("@@@@@") != -1) {
        let LogCutEntries = LogCutterTables.split("#####");
        LogCutEntries.forEach(element => {
          let entry = element.split("@@@@@");
          let model: TablePrinterModel =
          {
            applicationName: "Log Cut",
            printerName: entry[0],
            tableName: entry[1]
          };
          this.PrinterTableArray.push(model);
        });
      }

      if (EzStopTables.indexOf("@@@@@") != -1) {
        let EzStopEntries = EzStopTables.split("#####");
        EzStopEntries.forEach(element => {
          let entry = element.split("@@@@@");
          let model: TablePrinterModel =
          {
            applicationName: "EzStop",
            printerName: entry[0],
            tableName: entry[1]
          };
          this.PrinterTableArray.push(model);
        });
      }

      this.Columns.push('Application Name');
      this.Columns.push('Printer Name');
      this.Columns.push('Table Name');

      this.updateTable();
    });

    this.settingService.getColumnsNames().subscribe(data => {
      this.ColumnNames = data;
      console.log(data);
      setTimeout(() => {
        this.FabricSelectedColumnNames.forEach(element => {
          (document.getElementById('FabricCheckBox_' + element) as HTMLInputElement).checked = true;
        });
        
        this.LogCutSelectedColumnNames.forEach(element => {
          (document.getElementById('LogCutCheckBox_' + element) as HTMLInputElement).checked = true;
        });
        
        this.EzStopSelectedColumnNames.forEach(element => {
          (document.getElementById('EzStopCheckBox_' + element) as HTMLInputElement).checked = true;
        });
      }, 10);
    });


    this.settingService.GetPrinterNames().subscribe(data => {

      this.PrinterNames = data;
      setTimeout(() => {

        (document.getElementById('Printers') as HTMLSelectElement).value = this.SelectedPrinter;
      }, 50);
    });

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

  SaveSettings() {
    this.Saving = true;

    for (let i = 0; i < this.ListOfFiles.length; i++) {
      this.ListOfFiles[i].settingPath = (document.getElementById("ID_" + this.ListOfFiles[i].settingName) as HTMLInputElement).value;
    }

    let fabricObjects = document.getElementsByClassName('Fabric-custom-control-input');
    let logcutObjects = document.getElementsByClassName('LogCut-custom-control-input');
    let ezStopObjects = document.getElementsByClassName('EzStop-custom-control-input');
    this.FabricSelectedColumnNames = [];
    this.LogCutSelectedColumnNames = [];
    this.EzStopSelectedColumnNames = [];
    for (let index = 0; index < fabricObjects.length; index++) {
      if ((fabricObjects[index] as HTMLInputElement).checked) {
        this.FabricSelectedColumnNames.push((fabricObjects[index] as HTMLInputElement).title);
      }
    }
    
    for (let index = 0; index < logcutObjects.length; index++) {
      if ((logcutObjects[index] as HTMLInputElement).checked) {
        this.LogCutSelectedColumnNames.push((logcutObjects[index] as HTMLInputElement).title);
      }
    }
    
    
    for (let index = 0; index < ezStopObjects.length; index++) {
      if ((ezStopObjects[index] as HTMLInputElement).checked) {
        this.EzStopSelectedColumnNames.push((ezStopObjects[index] as HTMLInputElement).title);
      }
    }
    

    let newList = [...this.ListOfFiles];

    let newEntry: FileSettings = {
      settingName: "SelectedColumnsNames",
      settingPath: this.FabricSelectedColumnNames.join("@@@"),
      id: this.FabricSelectedColumnNamesId,
      applicationSetting: "FabricCutter"
    }
    newList.push(newEntry);
    
    newEntry = {
      settingName: "SelectedColumnsNames",
      settingPath: this.LogCutSelectedColumnNames.join("@@@"),
      id: this.LogCutSelectedColumnNamesId,
      applicationSetting: "LogCut"
    }
    newList.push(newEntry);
    
    newEntry= {
      settingName: "SelectedColumnsNames",
      settingPath: this.EzStopSelectedColumnNames.join("@@@"),
      id: this.EzStopSelectedColumnNamesId,
      applicationSetting: "EzStop"
    }
    newList.push(newEntry);

    let FabricCutterEntry: FileSettings = {
      settingName: "FabricCutterTable",
      settingPath: "",
      id: this.FabricCutterTableNumberId,
      applicationSetting: ""
    }

    let EzStopEntry: FileSettings = {
      settingName: "EzStopTable",
      settingPath: "",
      id: this.EzStopTableNumberId,
      applicationSetting: ""

    }

    let LogCutEntry: FileSettings = {
      settingName: "LogCutterTable",
      settingPath: "",
      id: this.LogCutterTableNumberId,
      applicationSetting: ""

    }
    this.PrinterTableArray.forEach(element => {
      if (element.applicationName == 'Fabric Cutter') {
        if (FabricCutterEntry.settingPath != "") {
          FabricCutterEntry.settingPath += "#####" + element.printerName + "@@@@@" + element.tableName;
        }
        else {
          FabricCutterEntry.settingPath = element.printerName + "@@@@@" + element.tableName;

        }
      }
      else if (element.applicationName == 'Log Cut') {
        if (LogCutEntry.settingPath != "") {
          LogCutEntry.settingPath += "#####" + element.printerName + "@@@@@" + element.tableName;
        }
        else {
          LogCutEntry.settingPath = element.printerName + "@@@@@" + element.tableName;

        }
      }
      else {
        if (EzStopEntry.settingPath != "") {
          EzStopEntry.settingPath += "#####" + element.printerName + "@@@@@" + element.tableName;
        }
        else {
          EzStopEntry.settingPath = element.printerName + "@@@@@" + element.tableName;
        }
      }
    });

    newList.push(FabricCutterEntry);

    newList.push(EzStopEntry);

    newList.push(LogCutEntry);

    this.settingService.UpdateSettings(newList).subscribe(data => {
      this.Saving = false;
    });

  }

  Delete(i) {
    this.PrinterTableArray.splice(i, 1);
    this.updateTable();
  }

  AddToTheTable() {
    let selectedPrinter = (document.getElementById("Printers") as HTMLSelectElement).value;
    let selectedApp = (document.getElementById("Application") as HTMLSelectElement).value;
    let TableName = (document.getElementById("TableName") as HTMLInputElement).value;

    let model: TablePrinterModel =
    {
      applicationName: selectedApp,
      printerName: selectedPrinter,
      tableName: TableName
    };
    this.PrinterTableArray.push(model);
    this.updateTable();
  }
}
