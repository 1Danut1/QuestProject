import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../models/login-request';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent {
  username = '';
  password = '';
  message = '';
  private returnUrl = '/';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
  ) {
    this.returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/';
  }

  login() {
    const request: LoginRequest = {
      username: this.username,
      password: this.password,
    };

    this.authService.login(request).subscribe({
      next: (res) => {
        this.message = res.message;
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err) => {
        this.message = typeof err?.error === 'string' ? err.error : 'Login failed.';
      },
    });
  }
}
