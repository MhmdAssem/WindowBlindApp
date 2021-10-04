import { Component, OnInit } from '@angular/core';
import { PreEzStopService } from './pre-ez-stop.service';

@Component({
  selector: 'app-pre-ez-stop',
  templateUrl: './pre-ez-stop.component.html',
  styleUrls: ['./pre-ez-stop.component.scss']
})
export class PreEzStopComponent implements OnInit {
  Generating: boolean;

  constructor(private PreEzStopService:PreEzStopService) { }

  ngOnInit(): void {


  }

  Generate() {

    let LineNumber = (document.getElementById("LineNumber") as HTMLInputElement).value.toString();
    if (LineNumber == '') return;
    this.Generating = true;
    this.PreEzStopService.getCBNumberDetails(LineNumber).subscribe(data =>{
      if(data == true)
      {
        alert("Your File is created successfully!");
        
      }
      this.Generating = false;
    })
    
  }

}
