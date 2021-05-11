import { Component, OnInit } from '@angular/core';
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

  displayedColumns: string[] = [ 'pincode','name', 'address', 'available'];
  lastRefresh: Date = new Date();

  constructor(private vaccineService: VaccineService) { }

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
    setInterval(() => {
      this.GetByDistrict();
      this.lastRefresh = new Date();
    }, 5000);
    this.GetByDistrict();
  }

  GetByDistrict(): void {
    this.vaccineService.GetByDistrict(this.date).subscribe(
      result => {
        console.log(result);
        this.response = result;
        this.filterData();
      },
      error => { console.log(error);
      }
    );
    // alert('Sent OTP on number ' + this.mobileNumber);
  }
  filterData() {
    this.centers = this.response.centers.filter(center => {
      return center.sessions.length > 1 && center.sessions.some(s => s.min_age_limit == this.age && (!this.showAvailable || s.available_capacity > 0));
    });
  }

  Submit(): void {
    alert(this.otp);
  }

}
