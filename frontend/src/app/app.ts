import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AsyncPipe, NgIf } from '@angular/common';
import { map } from 'rxjs';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NgIf, AsyncPipe],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private authService = inject(AuthService);
  protected authUser$ = this.authService.authUser$;
  protected isAuthenticated$ = this.authService.isAuthenticated$;
  protected username$ = this.authUser$.pipe(map((user) => user?.username ?? ''));

  logout(): void {
    this.authService.logout();
  }
}
