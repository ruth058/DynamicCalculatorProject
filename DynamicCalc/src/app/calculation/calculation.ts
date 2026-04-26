import { Component,ChangeDetectorRef } from '@angular/core';
import { CalculationService } from '../calculation.service';
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
  currentStep: number = 1; // 1: DynamicExpresso, 2: Sql, 3: DataTable, 4: Finished
completedSteps = new Set<number>();
  // מפה פנימית לשמירת הנתונים המצטברים
  private resultsMap = new Map<number, any>();
showReport: boolean = false;
  constructor(private calculationService: CalculationService,  private cdr: ChangeDetectorRef
) {}

 

  runStep(step: number) {
  this.loading = true;
  
  let obs$;
  if (step === 1) obs$ = this.calculationService.runDynamicExpresso();
  else if (step === 2) obs$ = this.calculationService.runSqlDynamic();
  else obs$ = this.calculationService.runDataTable();

  obs$.subscribe({
    next: (res: any) => {
              console.log("הרשימה שחזרה ==", res.details)

      const list = res.details || res;
      
      if (Array.isArray(list)) {
        this.updateDataMap(list, `v${step}`);
        
        // עדכון הנתונים
        this.data = Array.from(this.resultsMap.values());
        console.log("הרשימה לתצוגה ==", this.data)

    this.completedSteps.add(step);
this.currentStep = step + 1;
this.loading = false;
this.cdr.detectChanges()
      } else {
        this.loading = false;
      }
    },
    error: (e) => {
      console.error(e);
      this.loading = false;
    }
  });
}

  private updateDataMap(list: any[], versionKey: string) {
    list.forEach(item => {
      if (!this.resultsMap.has(item.targilId)) {
        this.resultsMap.set(item.targilId, {
          targil: item.targil,
          v1: null,
          v2: null,
          v3: null
        });
      }
      this.resultsMap.get(item.targilId)[versionKey] = item.timeSeconds;
    });
  }

  printReport() {
    console.log("מפיק דוח...", this.data);
    this.showReport=true
  }
}