import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CalculationDetail {
  targilId: number;
  targil: string;
  timeSeconds: number;
}

export interface CalculationResponse {
  success: boolean;
  message: string;
  details: CalculationDetail[];
}
@Injectable({
  providedIn: 'root'
})
export class CalculationService {

  private apiUrl = 'https://psychic-carnival-rqw7wq777x93x76r-5179.app.github.dev/api'; 

  constructor(private http: HttpClient) { }

  // שיטה 1: Dynamic Expresso
  runDynamicExpresso(): Observable<CalculationResponse> {
    return this.http.get<CalculationResponse>(`${this.apiUrl}/v1/run-calc/run-dynamic-expresso`);
  }

  // שיטה 2: SQL Dynamic
  runSqlDynamic(): Observable<CalculationResponse> {
    return this.http.get<CalculationResponse>(`${this.apiUrl}/v2/run-calc/run-sql-dynamic`);
  }

  // שיטה 3: NCalc / Data Table
  runDataTable(): Observable<CalculationResponse> {
    return this.http.get<CalculationResponse>(`${this.apiUrl}/v3/run-calc/RunDataTableCalculation`);
  }

  runAll(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/RunAll`);
  }
}