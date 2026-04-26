import { Component } from '@angular/core';
import {CalculationService,RunAllResponse} from '../calculation.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-calculation',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './calculation.html',
  styleUrl: './calculation.css',
})
export class Calculation {
  data: any[] = [];
  loading = false;

    constructor(private CalculationService: CalculationService) {}

   runAll() {
    console.log("הריצה החלה")
    this.loading = true;

    this.CalculationService.runAll().subscribe({

      next: (res: RunAllResponse) => {
          console.log("חזרו נתונים --",res)

        this.data = this.transformData(res);
                  console.log("הרשימה   --",this.data)

        this.loading = false;
      },
      error: (e) => {
          console.log("יש שגיאה ", e)

        this.loading = false;
      }
    });
  }

  transformData(res: RunAllResponse) {
    const map = new Map<number, any>();

    const addToMap = (list: any[], version: string) => {
      list.forEach(item => {
        if (!map.has(item.targilId)) {
          map.set(item.targilId, {
            targil: item.targil,
            v1: null,
            v2: null,
            v3: null
          });
        }

        map.get(item.targilId)[version] = item.timeSeconds;
      });
    };

    addToMap(res.summaryV1, 'v1');
    addToMap(res.summaryV2, 'v2');
    addToMap(res.summaryV3, 'v3');

    return Array.from(map.values());
  }
}
