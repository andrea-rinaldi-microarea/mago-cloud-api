import { BrowserModule } from '@angular/platform-browser';
import { ConnectionService } from './services/connection.service';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { MenuComponent } from './menu/menu.component';
import { LoginComponent } from './login/login.component';
import { GetContactsComponent } from './get-contacts/get-contacts.component';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
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
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    AppRoutingModule
  ],
  providers: [ConnectionService],
  bootstrap: [AppComponent]
})
export class AppModule { }
