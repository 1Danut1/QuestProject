import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { RegisterRequest } from '../../models/register-request';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class RegisterComponent {
  username = '';
  password = '';
  message = '';

  constructor(private authService: AuthService) {}

  register() {
    const request: RegisterRequest = {
      username: this.username,
      password: this.password,
    };

    this.authService.register(request).subscribe({
      next: (res) => {
        this.message = res.message;
      },
      error: (err) => {
        this.message = err.error;
      },
    });
  }
}
