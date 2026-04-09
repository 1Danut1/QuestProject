import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  message = 'Loading...';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.http
      .get('http://localhost:5261/api/test', { responseType: 'text' })
      .subscribe({
        next: (res) => {
          this.message = res;
        },
        error: (err) => {
          console.error(err);
          this.message = 'Could not connect to backend.';
        }
      });
  }
}