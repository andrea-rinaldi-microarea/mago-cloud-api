import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { ConnectionService } from './services/connection.service';
import { MenuComponent } from './menu/menu.component';
import { GetContactsComponent } from './get-contacts/get-contacts.component';
import { PostContactComponent } from './post-contact/post-contact.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    MenuComponent,
    GetContactsComponent,
    PostContactComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule
  ],
  providers: [ConnectionService],
  bootstrap: [AppComponent]
})
export class AppModule { }
