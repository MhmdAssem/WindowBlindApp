import { Component, OnInit } from '@angular/core';
import { PreEzStopService } from './pre-ez-stop.service';

@Component({
  selector: 'app-pre-ez-stop',
  templateUrl: './pre-ez-stop.component.html',
  styleUrls: ['./pre-ez-stop.component.scss']
})
export class PreEzStopComponent implements OnInit {
  Generating: boolean;

  constructor(private PreEzStopService: PreEzStopService) { }

  ngOnInit(): void {


    let Ts = this;
    document.addEventListener("keyup", function (event) {
      if (event.keyCode === 13) {
        event.preventDefault();
        Ts.Generate();
      }
    });
  }

  Generate() {

    let LineNumber = (document.getElementById("LineNumber") as HTMLInputElement).value.toString();
    if (LineNumber == '') return;
    this.Generating = true;
    this.PreEzStopService.getCBNumberDetails(LineNumber).subscribe(data => {

      (document.getElementById("LineNumber") as HTMLInputElement).value = "";
      this.Generating = false;
    })

  }

}
