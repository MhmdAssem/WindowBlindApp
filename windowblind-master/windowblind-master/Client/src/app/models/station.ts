import { FabricCutterSettings } from './fabricCutterSettings';
import { Table } from './table';

export class Station {
  name = '';
  id = '';
  tables: Table[] = [];
  settings = new FabricCutterSettings();
}
