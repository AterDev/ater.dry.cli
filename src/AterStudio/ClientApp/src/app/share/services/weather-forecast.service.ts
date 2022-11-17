import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { WeatherForecast } from '../models/weather-forecast/weather-forecast.model';

/**
 * WeatherForecast
 */
@Injectable({ providedIn: 'root' })
export class WeatherForecastService extends BaseService {
  /**
   * get
   */
  get(): Observable<WeatherForecast[]> {
    const url = `/WeatherForecast`;
    return this.request<WeatherForecast[]>('get', url);
  }

}
