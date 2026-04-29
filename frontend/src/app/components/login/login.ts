import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../models/login-request';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent {
  username = '';
  password = '';
  message = '';

  constructor(private authService: AuthService) {}

  login() {
    const request: LoginRequest = {
      username: this.username,
      password: this.password,
    };

    this.authService.login(request).subscribe({
      next: (res) => {
        this.message = res.message;
      },
      error: (err) => {
        this.message = err.error;
      },
    });
  }
}
