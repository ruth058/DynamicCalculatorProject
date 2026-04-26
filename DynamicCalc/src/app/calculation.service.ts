import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TargilResult {
  targilId: number;
  targil: string;
  timeSeconds: number;
}

export interface RunAllResponse {
  message: string;
  summaryV1: TargilResult[];
  summaryV2: TargilResult[];
  summaryV3: TargilResult[];
}
@Injectable({
  providedIn: 'root'
})
export class CalculationService {
  private apiUrl = 'https://bug-free-cod-5j5v5jvvgp934vrw-5003.app.github.dev/api/MainCalcController/run-all';

  constructor(private http: HttpClient) { }

  runAll(): Observable<RunAllResponse> {
    return this.http.get<RunAllResponse>(`${this.apiUrl}`);
  }
 
}