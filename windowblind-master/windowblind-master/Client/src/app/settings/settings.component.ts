import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
import { MatGridTileHeaderCssMatStyler } from '@angular/material/grid-list';
import { MatRadioButton } from '@angular/material/radio';
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
  AutoUpload: boolean;
  LogCutAutoUpload: boolean;

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
  AssemblyStationSelectedColumnNames: string[];
  HoistStationSelectedColumnNames: string[];
  PackingStationSelectedColumnNames: string[];
  FabricSelectedColumnNamesId: string;
  LogCutSelectedColumnNamesId: string;
  EzStopSelectedColumnNamesId: string;
  AssemblyStationSelectedColumnNamesId: string;
  HoistStationSelectedColumnNamesId: string;
  PackingStationSelectedColumnNamesId: string;
  FabricCutterTableNumberId: string;
  LogCutterTableNumberId: string;
  EzStopTableNumberId: string;
  AssemblyStationTableNumberId: string;
  HoistStationTableNumberId: string;
  PackingStationTableNumberId: string;

  SelectedPrinter = "";
  SelectedPrinterId = "";

  Columns: string[] = []; //for the printe tables
  CommentColumns: string[] = []; //for the printe tables
  CommentsId: string;
  PrinterTableArray: TablePrinterModel[] = [];
  CommentsTableArray: string[] = [];

  @ViewChildren(DataTableDirective)
  dtElements: QueryList<DataTableDirective>;
  dtTrigger: Subject<any> = new Subject();
  dtOptions: DataTables.Settings = {};
  dtTriggerComments: Subject<any> = new Subject();
  dtOptionsComments: DataTables.Settings = {};

  AutoUploadPath: string;
  ViewedUploadsPath: string;

  LogCutAutoUploadPath: string;
  LogCutViewedUploadsPath: string;

  FabricCBSearch: boolean;
  LogCutCBSearch: boolean;
  PackingStationSelected:boolean;
  
  SelectFirstPrinter = "";
  SelectSecondPrinter = ""

  ApplicationMap = {
    "Fabric Cutter": "FabricCutterTable",
    "Log Cut": "LogCutterTable",
    "EzStop": "EzStopTable",
    "AssemblyStation": "AssemblyStationTable",
    "HoistStation": "HoistStationTable",
    "PackingStation": "PackingStationTable"
  }

  ngOnInit(): void {
    this.FabricCBSearch = true;
    this.LogCutCBSearch = true;
    this.PackingStationSelected = false;
    this.FabricSelectedColumnNames = [];
    this.LogCutSelectedColumnNames = [];
    this.EzStopSelectedColumnNames = [];
    this.AssemblyStationSelectedColumnNames = [];
    this.HoistStationSelectedColumnNames = [];
    this.PackingStationSelectedColumnNames = [];
    this.LogCutAutoUpload = true;
    this.AutoUpload = true;
    this.SelectFirstPrinter = "Select A Printer";
    this.SelectSecondPrinter = "Select Large Label printer"
    this.dtOptions = {
      pagingType: 'full_numbers',
      lengthChange: false,
      searching: false,
      destroy: true,
      ordering: false,
      pageLength: 4,

    };
    this.dtOptionsComments = {
      pagingType: 'full_numbers',
      lengthChange: false,
      searching: false,
      destroy: true,
      ordering: false,
      pageLength: 4,

    };

    /// load all settings From BackEnd
    this.settingService.getSettings().subscribe(data => {
      this.ListOfFiles = data;

      //#region  getting selected columns

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


      let indAssemblyStation = this.ListOfFiles.findIndex(e => e.settingName == "SelectedColumnsNames" && e.applicationSetting == "AssemblyStation");

      if (this.ListOfFiles[indAssemblyStation].settingPath != "")
        this.AssemblyStationSelectedColumnNames = (this.ListOfFiles[indAssemblyStation].settingPath.split("@@@") as []);

      this.AssemblyStationSelectedColumnNamesId = this.ListOfFiles[indAssemblyStation].id;
      this.ListOfFiles.splice(indAssemblyStation, 1);

      let indHoistStation = this.ListOfFiles.findIndex(e => e.settingName == "SelectedColumnsNames" && e.applicationSetting == "HoistStation");

      if (this.ListOfFiles[indHoistStation].settingPath != "")
        this.HoistStationSelectedColumnNames = (this.ListOfFiles[indHoistStation].settingPath.split("@@@") as []);

      this.HoistStationSelectedColumnNamesId = this.ListOfFiles[indHoistStation].id;
      this.ListOfFiles.splice(indHoistStation, 1);


      let indPackingStation = this.ListOfFiles.findIndex(e => e.settingName == "SelectedColumnsNames" && e.applicationSetting == "PackingStation");

      if (this.ListOfFiles[indPackingStation].settingPath != "")
        this.PackingStationSelectedColumnNames = (this.ListOfFiles[indPackingStation].settingPath.split("@@@") as []);

      this.PackingStationSelectedColumnNamesId = this.ListOfFiles[indPackingStation].id;
      this.ListOfFiles.splice(indPackingStation, 1);

      //#endregion


      //#region getting Tables

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

      ind = this.ListOfFiles.findIndex(e => e.settingName == "AssemblyStationTable");
      this.AssemblyStationTableNumberId = this.ListOfFiles[ind].id;
      let AssemblyTables = this.ListOfFiles[ind].settingPath;
      this.ListOfFiles.splice(ind, 1);

      ind = this.ListOfFiles.findIndex(e => e.settingName == "HoistStationTable");
      this.HoistStationTableNumberId = this.ListOfFiles[ind].id;
      let HoistTables = this.ListOfFiles[ind].settingPath;
      this.ListOfFiles.splice(ind, 1);

      ind = this.ListOfFiles.findIndex(e => e.settingName == "PackingStationTable");
      this.PackingStationTableNumberId = this.ListOfFiles[ind].id;
      let PackingTables = this.ListOfFiles[ind].settingPath;
      this.ListOfFiles.splice(ind, 1);

      if (FabricCutterTables.indexOf("@@@@@") != -1) {
        let FabricCutterEntries = FabricCutterTables.split("#####");
        FabricCutterEntries.forEach(element => {
          let entry = element.split("@@@@@");
          let model: TablePrinterModel =
          {
            applicationName: "Fabric Cutter",
            printerName: entry[0],
            tableName: entry[1],
            OutputPath: entry[2],
            printerName2nd:entry.length>3?entry[3]:'-'
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
            tableName: entry[1],
            OutputPath: entry[2],
            printerName2nd:entry.length>3?entry[3]:'-'
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
            tableName: entry[1],
            OutputPath: entry[2],
            printerName2nd:entry.length>3?entry[3]:'-'
          };
          this.PrinterTableArray.push(model);
        });
      }

      if (AssemblyTables.indexOf("@@@@@") != -1) {
        let AssemblyTablesEntries = AssemblyTables.split("#####");
        AssemblyTablesEntries.forEach(element => {
          let entry = element.split("@@@@@");
          let model: TablePrinterModel =
          {
            applicationName: "AssemblyStation",
            printerName: entry[0],
            tableName: entry[1],
            OutputPath: entry[2],
            printerName2nd:entry.length>3?entry[3]:'-'
          };
          this.PrinterTableArray.push(model);
        });
      }

      if (HoistTables.indexOf("@@@@@") != -1) {
        let HoistTablesEntries = HoistTables.split("#####");
        HoistTablesEntries.forEach(element => {
          let entry = element.split("@@@@@");
          let model: TablePrinterModel =
          {
            applicationName: "HoistStation",
            printerName: entry[0],
            tableName: entry[1],
            OutputPath: entry[2],
            printerName2nd:entry.length>3?entry[3]:'-'
          };
          this.PrinterTableArray.push(model);
        });
      }

      if (PackingTables.indexOf("@@@@@") != -1) {
        let PackingTablesEntries = PackingTables.split("#####");
        PackingTablesEntries.forEach(element => {
          let entry = element.split("@@@@@");
          let model: TablePrinterModel =
          {
            applicationName: "PackingStation",
            printerName: entry[0],
            tableName: entry[1],
            OutputPath: entry[2],
            printerName2nd:entry.length>3?entry[3]:'-'
          };
          this.PrinterTableArray.push(model);
        });
      }

      this.Columns.push('Application Name');
      this.Columns.push('Printer Name');
      this.Columns.push('2nd Printer Name');
      this.Columns.push('Table Name');
      this.Columns.push('Output Path');

      this.CommentColumns.push('Comment')


      //#endregion

      /// get the Search Type For Fabric Cutter

      let SearchTypeIndex = this.ListOfFiles.findIndex(file => file.applicationSetting == '_FabricCutter' && file.settingName == 'SearchType');
      let SearchTypeValue = this.ListOfFiles[SearchTypeIndex].settingPath;

      let AutoUploadDir = this.ListOfFiles.findIndex(file => file.applicationSetting == '_FabricCutter' && file.settingName == 'AutoUploadDir');
      let AutoUploadDirValue = this.ListOfFiles[AutoUploadDir].settingPath;

      let ViewedUploadsDir = this.ListOfFiles.findIndex(file => file.applicationSetting == '_FabricCutter' && file.settingName == 'ViewedUploadsDir');
      let ViewedUploadsDirValue = this.ListOfFiles[ViewedUploadsDir].settingPath;

      this.RadioOn(SearchTypeValue, 'FabricCut');

      this.FabricCBSearch = (SearchTypeValue == 'CB_Line_Number_Search');
      this.AutoUploadPath = AutoUploadDirValue;
      this.ViewedUploadsPath = ViewedUploadsDirValue;

      ///get search type for logcut

      let LogCutSearchTypeIndex = this.ListOfFiles.findIndex(file => file.applicationSetting == '_LogCut' && file.settingName == 'SearchType');
      let LogCutSearchTypeValue = this.ListOfFiles[LogCutSearchTypeIndex].settingPath;

      let LogCutAutoUploadDir = this.ListOfFiles.findIndex(file => file.applicationSetting == '_LogCut' && file.settingName == 'AutoUploadDir');
      let LogCutAutoUploadDirValue = this.ListOfFiles[LogCutAutoUploadDir].settingPath;

      let LogCutViewedUploadsDir = this.ListOfFiles.findIndex(file => file.applicationSetting == '_LogCut' && file.settingName == 'ViewedUploadsDir');
      let LogCutViewedUploadsDirValue = this.ListOfFiles[LogCutViewedUploadsDir].settingPath;

      this.RadioOn(LogCutSearchTypeValue, 'LogCut');

      this.LogCutCBSearch = (LogCutSearchTypeValue == 'LogCut_CB_Line_Number_Search');

      this.LogCutAutoUploadPath = LogCutAutoUploadDirValue;
      this.LogCutViewedUploadsPath = LogCutViewedUploadsDirValue;



      let CommentsIndex = this.ListOfFiles.findIndex(file => file.settingName == 'Comments');
      let CommentsValue = this.ListOfFiles[CommentsIndex].settingPath.split("@@@");


      CommentsValue.forEach(element => {
        this.CommentsTableArray.push(element);
      });
      this.CommentsId = this.ListOfFiles[CommentsIndex].id;
      this.ListOfFiles.splice(CommentsIndex, 1);
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

        this.AssemblyStationSelectedColumnNames.forEach(element => {
          (document.getElementById('AssemblyStationCheckBox_' + element) as HTMLInputElement).checked = true;
        });

        this.HoistStationSelectedColumnNames.forEach(element => {
          (document.getElementById('HoistStationCheckBox_' + element) as HTMLInputElement).checked = true;
        });

        this.PackingStationSelectedColumnNames.forEach(element => {
          (document.getElementById('PackingStationCheckBox_' + element) as HTMLInputElement).checked = true;
        });


      }, 10);
    });


    this.settingService.GetPrinterNames().subscribe(data => {

      this.PrinterNames = data;
      setTimeout(() => {

        (document.getElementById('Printers') as HTMLSelectElement).value = this.SelectedPrinter;
        (document.getElementById('Printers2') as HTMLSelectElement).value = this.SelectedPrinter;
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
      this.dtTriggerComments.next();
      console.log("Catch");
    }
  }

  SaveSettings() {
    this.Saving = true;

    for (let i = 0; i < this.ListOfFiles.length; i++) {
      if ((document.getElementById("ID_" + this.ListOfFiles[i].settingName) as HTMLInputElement) != null && (document.getElementById("ID_" + this.ListOfFiles[i].settingName) as HTMLInputElement) != undefined)
        this.ListOfFiles[i].settingPath = (document.getElementById("ID_" + this.ListOfFiles[i].settingName) as HTMLInputElement).value;
    }

    let fabricObjects = document.getElementsByClassName('Fabric-custom-control-input');
    let logcutObjects = document.getElementsByClassName('LogCut-custom-control-input');
    let ezStopObjects = document.getElementsByClassName('EzStop-custom-control-input');
    let AssemblyStationObjects = document.getElementsByClassName('AssemblyStation-custom-control-input');
    let HoistStationObjects = document.getElementsByClassName('HoistStation-custom-control-input');
    let PackingStationObjects = document.getElementsByClassName('PackingStation-custom-control-input');
    this.FabricSelectedColumnNames = [];
    this.LogCutSelectedColumnNames = [];
    this.EzStopSelectedColumnNames = [];
    this.AssemblyStationSelectedColumnNames = [];
    this.HoistStationSelectedColumnNames = [];
    this.PackingStationSelectedColumnNames = [];

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

    for (let index = 0; index < AssemblyStationObjects.length; index++) {
      if ((AssemblyStationObjects[index] as HTMLInputElement).checked) {
        this.AssemblyStationSelectedColumnNames.push((AssemblyStationObjects[index] as HTMLInputElement).title);
      }
    }

    for (let index = 0; index < HoistStationObjects.length; index++) {
      if ((HoistStationObjects[index] as HTMLInputElement).checked) {
        this.HoistStationSelectedColumnNames.push((HoistStationObjects[index] as HTMLInputElement).title);
      }
    }

    for (let index = 0; index < PackingStationObjects.length; index++) {
      if ((PackingStationObjects[index] as HTMLInputElement).checked) {
        this.PackingStationSelectedColumnNames.push((PackingStationObjects[index] as HTMLInputElement).title);
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

    newEntry = {
      settingName: "SelectedColumnsNames",
      settingPath: this.EzStopSelectedColumnNames.join("@@@"),
      id: this.EzStopSelectedColumnNamesId,
      applicationSetting: "EzStop"
    }
    newList.push(newEntry);

    newEntry = {
      settingName: "SelectedColumnsNames",
      settingPath: this.AssemblyStationSelectedColumnNames.join("@@@"),
      id: this.AssemblyStationSelectedColumnNamesId,
      applicationSetting: "AssemblyStation"
    }
    newList.push(newEntry);

    newEntry = {
      settingName: "SelectedColumnsNames",
      settingPath: this.HoistStationSelectedColumnNames.join("@@@"),
      id: this.HoistStationSelectedColumnNamesId,
      applicationSetting: "HoistStation"
    }
    newList.push(newEntry);

    newEntry = {
      settingName: "SelectedColumnsNames",
      settingPath: this.PackingStationSelectedColumnNames.join("@@@"),
      id: this.PackingStationSelectedColumnNamesId,
      applicationSetting: "PackingStation"
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

    let AssemblyEntry: FileSettings = {
      settingName: "AssemblyStationTable",
      settingPath: "",
      id: this.AssemblyStationTableNumberId,
      applicationSetting: ""

    }

    let HoistEntry: FileSettings = {
      settingName: "HoistStationTable",
      settingPath: "",
      id: this.HoistStationTableNumberId,
      applicationSetting: ""

    }

    let PackingEntry: FileSettings = {
      settingName: "PackingStationTable",
      settingPath: "",
      id: this.PackingStationTableNumberId,
      applicationSetting: ""
    }



    this.PrinterTableArray.forEach(element => {
      if (element.applicationName == 'Fabric Cutter') {
        if (FabricCutterEntry.settingPath != "") {
          FabricCutterEntry.settingPath += "#####" + element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;
        }
        else {
          FabricCutterEntry.settingPath = element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;;

        }
      }
      else if (element.applicationName == 'Log Cut') {
        if (LogCutEntry.settingPath != "") {
          LogCutEntry.settingPath += "#####" + element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;;
        }
        else {
          LogCutEntry.settingPath = element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;;

        }
      }
      else if (element.applicationName == 'EzStop') {
        if (EzStopEntry.settingPath != "") {
          EzStopEntry.settingPath += "#####" + element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;;
        }
        else {
          EzStopEntry.settingPath = element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;;
        }
      }
      else if (element.applicationName == 'AssemblyStation') {
        if (AssemblyEntry.settingPath != "") {
          AssemblyEntry.settingPath += "#####" + element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;;
        }
        else {
          AssemblyEntry.settingPath = element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;;
        }
      }
      else if (element.applicationName == 'HoistStation') {
        if (HoistEntry.settingPath != "") {
          HoistEntry.settingPath += "#####" + element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;;
        }
        else {
          HoistEntry.settingPath = element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;;
        }
      }
      else if (element.applicationName == 'PackingStation') {
        if (PackingEntry.settingPath != "") {
          PackingEntry.settingPath += "#####" + element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;;
        }
        else {
          PackingEntry.settingPath = element.printerName + "@@@@@" + element.tableName + "@@@@@" + element.OutputPath + "@@@@@" + element.printerName2nd;;
        }
      }

      this.settingService.InsertTableNameWithThePath(element.tableName, element.OutputPath).subscribe();
    });

    newList.push(FabricCutterEntry);

    newList.push(EzStopEntry);

    newList.push(LogCutEntry);

    newList.push(AssemblyEntry);

    newList.push(HoistEntry);

    newList.push(LogCutEntry);

    newList.push(PackingEntry);


    let SearchTypeIndex = this.ListOfFiles.findIndex(file => file.applicationSetting == '_FabricCutter' && file.settingName == 'SearchType');
    let SearchTypeValue = (this.AutoUpload) ? 'AutoUpload' : 'CB_Line_Number_Search';
    this.ListOfFiles[SearchTypeIndex].settingPath = SearchTypeValue;
    let SearchTypeEntry = this.ListOfFiles[SearchTypeIndex];

    if ((document.getElementById('ID_AutoUploadDir') as HTMLInputElement) != null && (document.getElementById('ID_AutoUploadDir') as HTMLInputElement) != undefined) {
      let AutoUploadDirIndex = this.ListOfFiles.findIndex(file => file.applicationSetting == '_FabricCutter' && file.settingName == 'AutoUploadDir');
      let AutoUploadDirValue = (document.getElementById('ID_AutoUploadDir') as HTMLInputElement).value
      this.ListOfFiles[AutoUploadDirIndex].settingPath = AutoUploadDirValue;
      let AutoUploadDirEntry = this.ListOfFiles[AutoUploadDirIndex];
      newList.push(AutoUploadDirEntry);

    }
    if ((document.getElementById('ID_ViewedUploadDir') as HTMLInputElement) != null && (document.getElementById('ID_ViewedUploadDir') as HTMLInputElement) != undefined) {

      let ViewedUploadsDir = this.ListOfFiles.findIndex(file => file.applicationSetting == '_FabricCutter' && file.settingName == 'ViewedUploadsDir');
      let ViewedUploadsDirValue = (document.getElementById('ID_ViewedUploadDir') as HTMLInputElement).value
      this.ListOfFiles[ViewedUploadsDir].settingPath = ViewedUploadsDirValue;
      let ViewedUploadsDirEntry = this.ListOfFiles[ViewedUploadsDir];
      newList.push(ViewedUploadsDirEntry);

    }



    let LogCutSearchTypeIndex = this.ListOfFiles.findIndex(file => file.applicationSetting == '_LogCut' && file.settingName == 'SearchType');
    let LogCutSearchTypeValue = (this.LogCutAutoUpload) ? 'AutoUpload' : 'LogCut_CB_Line_Number_Search';
    this.ListOfFiles[LogCutSearchTypeIndex].settingPath = LogCutSearchTypeValue;
    let LogCutSearchTypeEntry = this.ListOfFiles[LogCutSearchTypeIndex];

    if ((document.getElementById('ID_LogCutAutoUploadDir') as HTMLInputElement) != null && (document.getElementById('ID_LogCutAutoUploadDir') as HTMLInputElement) != undefined) {
      let AutoUploadDirIndex = this.ListOfFiles.findIndex(file => file.applicationSetting == '_LogCut' && file.settingName == 'AutoUploadDir');
      let AutoUploadDirValue = (document.getElementById('ID_LogCutAutoUploadDir') as HTMLInputElement).value
      this.ListOfFiles[AutoUploadDirIndex].settingPath = AutoUploadDirValue;
      let AutoUploadDirEntry = this.ListOfFiles[AutoUploadDirIndex];
      newList.push(AutoUploadDirEntry);

    }
    if ((document.getElementById('ID_LogCutViewedUploadDir') as HTMLInputElement) != null && (document.getElementById('ID_LogCutViewedUploadDir') as HTMLInputElement) != undefined) {

      let ViewedUploadsDir = this.ListOfFiles.findIndex(file => file.applicationSetting == '_LogCut' && file.settingName == 'ViewedUploadsDir');
      let ViewedUploadsDirValue = (document.getElementById('ID_LogCutViewedUploadDir') as HTMLInputElement).value
      this.ListOfFiles[ViewedUploadsDir].settingPath = ViewedUploadsDirValue;
      let ViewedUploadsDirEntry = this.ListOfFiles[ViewedUploadsDir];
      newList.push(ViewedUploadsDirEntry);

    }




    newList.push(SearchTypeEntry);
    newList.push(LogCutSearchTypeEntry);

    if (!this.localCalling && this.FabricSelectedColumnNames.length == 0) {
      this.Saving = false;
      alert("You Have To Select atleast one column from Fabric Cutter columns");
      return;
    }

    if (!this.localCalling && this.LogCutSelectedColumnNames.length == 0) {
      this.Saving = false;
      alert("You Have To Select atleast one column from Log Cut columns");
      return;
    }
    if (!this.localCalling && this.EzStopSelectedColumnNames.length == 0) {
      this.Saving = false;
      alert("You Have To Select atleast one column from EzStop columns");
      return;
    }
    if (!this.localCalling && this.AssemblyStationSelectedColumnNames.length == 0) {
      this.Saving = false;
      alert("You Have To Select atleast one column from Assembly columns");
      return;
    }
    if (!this.localCalling && this.HoistStationSelectedColumnNames.length == 0) {
      this.Saving = false;
      alert("You Have To Select atleast one column from Hoist columns");
      return;
    }
    if (!this.localCalling && this.PackingStationSelectedColumnNames.length == 0) {
      this.Saving = false;
      alert("You Have To Select atleast one column from Packing columns");
      return;
    }

    let CommentsEntry: FileSettings = {
      settingName: "Comments",
      settingPath: this.CommentsTableArray.join('@@@'),
      id: this.CommentsId,
      applicationSetting: ""
    }
    newList.push(CommentsEntry);
    this.settingService.UpdateSettings(newList).subscribe(data => {
      this.Saving = false;
      if (!this.localCalling)
        alert("Saved Successfuly !!")
      this.localCalling = false;
    });

  }
  localCalling = false;
  Delete(i) {
    this.settingService.DeleteTable(this.PrinterTableArray[i].tableName).subscribe(
      bool => {
        if (bool) {
          this.localCalling = true;
          this.PrinterTableArray.splice(i, 1);
          this.updateTable();
          this.SaveSettings();

        }
        else {
          alert("Error Happened");
        }
      }
    );

  }

  AddToTheTable() {
    let selectedPrinter = (document.getElementById("Printers") as HTMLSelectElement).value;
    let selectedPrinter2nd = this.PackingStationSelected == true?(document.getElementById("Printers2") as HTMLSelectElement).value:'-';
    let selectedApp = (document.getElementById("Application") as HTMLSelectElement).value;
    let TableName = (document.getElementById("TableName") as HTMLInputElement).value;
    let OutputPath = (document.getElementById("OutputPath") as HTMLInputElement).value;
    if (TableName == '') {
      alert("Enter A Table Name"); return;
    }
    if (OutputPath == '') {
      alert("Enter An Output Path"); return;
    }
    if (selectedApp == 'EzStop' && this.PrinterTableArray.findIndex(table => table.applicationName == 'EzStop') != -1) {
      alert("You can only create 1 EzStop table !"); return;

    }
    this.settingService.CheckTableExists(TableName).subscribe(res => {

      if (res && this.PrinterTableArray.findIndex(t => t.tableName == TableName) == -1) {
        let model: TablePrinterModel =
        {
          applicationName: selectedApp,
          printerName: selectedPrinter,
          tableName: TableName,
          OutputPath: OutputPath,
          printerName2nd:selectedPrinter2nd

        };
        this.PrinterTableArray.push(model);
        this.updateTable();
        $('#Printers').val('');
        $('#Application').val('');
        $('#TableName').val('');
        $('#OutputPath').val('');
        $('#Printers2').val('');
      }
      else {
        alert("Table Name already exists, please enter another one!!"); return;
      }
    })

  }


  RadioOn(radioType, Type) {
    switch (Type) {
      case "FabricCut":
        this.AutoUpload = (radioType == 'AutoUpload');
        break;
      case "LogCut":
        this.LogCutAutoUpload = (radioType == 'AutoUpload');
        break;

    }

  }


  AddNewComment() {
    var Comment = (document.getElementById("NewComment") as HTMLInputElement).value;

    let ind = this.CommentsTableArray.findIndex(e => e == Comment);
    if (ind == -1) {
      this.CommentsTableArray.push(Comment);
    }
    (document.getElementById("NewComment") as HTMLInputElement).value = "";
    this.updateTable();

  }

  DeleteComment(i) {

    this.CommentsTableArray.splice(i, 1);
    this.updateTable();
  }
  
  
  ApplicationSelected()
  {
    let selectedApp = (document.getElementById("Application") as HTMLSelectElement).value;
    
    this.PackingStationSelected = (selectedApp == "PackingStation");
    this.SelectFirstPrinter = (selectedApp == "PackingStation")? "Select Pink Label Printer":"Select A Printer";
  }

}
