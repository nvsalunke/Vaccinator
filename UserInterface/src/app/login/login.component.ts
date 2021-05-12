import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Login } from '../models/login.model';
import { VaccineService } from '../services/vaccine.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  centers: any;
  response: any;

  displayedColumns: string[] = ['pincode', 'name', 'address', 'available'];
  lastRefresh: Date = new Date();
  interval: number = 5000;

  constructor(private vaccineService: VaccineService, private route: ActivatedRoute) { }

  mobileNumber: string = "8956594649";
  otp: string = "";

  powers = ['Really Smart', 'Super Flexible',
    'Super Hot', 'Weather Changer'];

  model = new Login(18, 'Dr IQ', this.powers[0], 'Chuck Overstreet');

  submitted = false;
  showAvailable = false;

  date: any = new Date();
  age = 45;

  onSubmit() { this.submitted = true; }

  newHero() {
    this.model = new Login(42, '', '');
  }

  ngOnInit(): void {

    this.route.queryParams.subscribe(params => {
      console.log(params);
      this.interval = params['interval'] ? params['interval'] : 3000;

      setInterval(() => {
        this.GetByDistrict();
        this.lastRefresh = new Date();
      }, this.interval);
    });
    this.GetByDistrict();
  }

  GetByDistrict(): void {
    this.vaccineService.GetByDistrict(this.date).subscribe(
      result => {
        console.log(result);
        this.response = result;
        this.filterData();
      },
      error => {
        alert('OpenAPI limit reached! Please try after 5 minutes');
      }
    );
    // alert('Sent OTP on number ' + this.mobileNumber);
  }
  filterData() {
    this.centers = this.response.centers.filter(center => {
      return center.sessions.length > 0 && center.sessions.some(s => s.min_age_limit == this.age && (!this.showAvailable || s.available_capacity > 0));
    });
  }

  Submit(): void {
    alert(this.otp);
  }

}
