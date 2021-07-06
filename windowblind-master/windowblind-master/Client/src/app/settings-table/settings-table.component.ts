import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-settings-table',
  templateUrl: './settings-table.component.html',
  styleUrls: ['./settings-table.component.scss']
})
export class SettingsTableComponent implements OnInit {

  constructor() { }
  value: any[] = [];
  @Input() dataSource: any[] = [];
  @Input() valueType = 'string';
  @Output() dataChange = new EventEmitter<any[]>();
  get valueIsNumber(): boolean {
    return this.valueType === 'number';
  }


  ngOnInit(): void {
    this.value = [...this.dataSource];
    this.value.push({editing: true, isEmpty: true, key: '', value: ''});
  }

  editRow(row: any): void{
    this.dataSource.forEach(d => d.editing = false);
    row.editing = true;
  }
  saveRow(row: any): void{
    row.editing = row.isEmpty = false;
    if (!this.value.some(r => r.isEmpty)) {
      this.value = [...this.value, {editing: true, isEmpty: true, key: '', value: ''}];
    }
    this.dataChange.emit(this.value.filter(r => !r.isEmpty));
  }
  deleteRow(row: any): void{
    this.value = this.value.filter(r => r !== row);
    this.dataChange.emit(this.value.filter(r => !r.isEmpty));
  }

  onChange(event: any, row: any, field: string): void{
    let value = event.srcElement.value;
    if (field === 'value' && this.valueIsNumber) {
      value = parseInt(value, 10);
    }
    row[field] = value;
  }

}
