import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class VaccineService {

  url = "https://cdn-api.co-vin.in/api"
  constructor(private httpClient: HttpClient) { }

  GenerateOTP(mobileNumber: string){
    return this.httpClient.get(this.url + "/v2/appointment/sessions/public/calendarByPin?pincode=411033&date=11-05-2021");
  }

  GetByDistrict(date: Date, districtId: number = 363){
    let dateString = `${date.getDate()}-${date.getMonth()+1}-${date.getFullYear()}`;
    return this.httpClient.get(`${this.url}/v2/appointment/sessions/public/calendarByDistrict?district_id=${districtId}&date=${dateString}`);
  }
}
