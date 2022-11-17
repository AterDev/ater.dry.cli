import { DayOfWeek } from './enum/day-of-week.model';
export interface DateOnly {
  year: number;
  month: number;
  day: number;
  dayOfWeek?: DayOfWeek | null;
  dayOfYear: number;
  dayNumber: number;

}
