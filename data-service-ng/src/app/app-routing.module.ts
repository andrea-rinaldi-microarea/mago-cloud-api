import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ContactsListComponent } from './contacts-list/contacts-list.component';
import { ItemsComponent } from './items/items.component';
import { LoginComponent } from './login/login.component';
import { MenuComponent } from './menu/menu.component';

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
    path: 'contacts-list',
    component: ContactsListComponent,
  },
  {
    path: 'items',
    component: ItemsComponent,
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
