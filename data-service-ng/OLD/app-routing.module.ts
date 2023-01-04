import { ContactsListComponent } from './contacts-list/contacts-list.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { MenuComponent } from './menu/menu.component';
import { ItemsComponent } from './items/items.component';


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
    path: 'contacts-list',
    component: ContactsListComponent,
  },
  {
    path: 'menu',
    component: MenuComponent,
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
