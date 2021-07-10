export class FabricCutterCBDetailsModel {

  columnNames: string[] = [];
  rows: FabricCutterCBDetailsModelTableRow[] = [];
}

export class FabricCutterCBDetailsModelTableRow {
  uniqueId:string ="";
  row: { [key: string]: string } = {};
  blindNumbers: number[] = [];
  PackingType:string;
}
