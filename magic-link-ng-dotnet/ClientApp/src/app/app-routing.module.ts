import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GetContactsComponent } from './get-contacts/get-contacts.component';
import { LoginComponent } from './login/login.component';
import { MenuComponent } from './menu/menu.component';
import { PostContactComponent } from './post-contact/post-contact.component';

const routes: Routes = [
  {
    path: '',
    component: LoginComponent,
  },
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'menu',
    component: MenuComponent,
  },
  {
    path: 'get-contacts',
    component: GetContactsComponent,
  },
  {
    path: 'post-contact',
    component: PostContactComponent,
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
