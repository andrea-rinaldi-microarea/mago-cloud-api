import { Component } from '@angular/core';
import { ConnectionService } from '../services/connection.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  constructor( public connection: ConnectionService) {

  }
}
