import { FabricCutterCBDetailsModelTableRow } from "../fabric-cutter/FabricCutterCBDetailsModel";

export class RejectionModel {
  id: string;
  stationName: string;
  dateTime: string;
  row: FabricCutterCBDetailsModelTableRow;
  userName: string;
  tableName: string
  forwardedToStation: string;
  rejectionReasons:string[];
  
}
