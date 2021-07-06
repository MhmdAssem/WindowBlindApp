export class GridColumn {

  static fabricCutterColumns: GridColumn[] = [
    {name: 'lineNumber', title: 'Line Number'},
    {name: 'quantity', title: 'Quantity'},
    {name: 'measuredWidth', title: 'Measured Width'},
    {name: 'measuredDrop', title: 'Measured Drop'},
    {name: 'fabricType', title: 'Fabric Type'},
    {name: 'fabricColour', title: 'Fabric Colour'},
    {name: 'trimType', title: 'Trim Type'},
    {name: 'controlSide', title: 'Control Side'},
    {name: 'trackColour', title: 'Track Colour'},
    {name: 'rollWidth', title: 'Roll Width'},
    {name: 'pullColour', title: 'Pull Colour'},
    {name: 'barcode', title: 'Barcode'},
    {name: 'cutWidth', title: 'Cut Width'},
    {name: 'controlType', title: 'Control Type'},
  ];
  name = '';
  title = '';
}
