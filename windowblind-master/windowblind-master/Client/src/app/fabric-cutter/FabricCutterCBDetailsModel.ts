export class FabricCutterCBDetailsModel {

  columnNames: string[] = [];
  rows: FabricCutterCBDetailsModelTableRow[] = [];
}

export class FabricCutterCBDetailsModelTableRow {
  uniqueId:string ="";
  row: { [key: string]: string } = {};
  blindNumbers: number[] = [];
  PackingType:string;
  rows_AssociatedIds:[] = [];
}


export class ResultModel{
  data:FabricCutterCBDetailsModel;
  status:number;
  message:string;
  stackTrace:string;
}
