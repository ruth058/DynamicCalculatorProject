import { Routes } from '@angular/router';
import { Calculation } from './calculation/calculation';

export const routes: Routes = [
     { 
    path: '', 
    component: Calculation // מגדיר שזו הקומפוננטה שתעלה מיד בטעינת הדף
  }
];
