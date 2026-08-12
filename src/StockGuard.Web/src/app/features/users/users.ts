import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UserService, RegisterUserPayload, ALL_ROLES } from '../../core/user-service';

const EMPTY_FORM: RegisterUserPayload = { email: '', password: '', fullName: '', role: 'WarehouseEmployee' };

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './users.html',
  styleUrl: './users.scss'
})
export class Users {
  roles = ALL_ROLES;
  form: RegisterUserPayload = { ...EMPTY_FORM };
  saving = signal(false);
  error = signal<string | null>(null);
  registered = signal<string[]>([]);

  constructor(private userService: UserService) {}

  register(): void {
    this.error.set(null);
    this.saving.set(true);

    this.userService.register(this.form).subscribe({
      next: (user) => {
        this.saving.set(false);
        this.registered.update(list => [`${user.fullName} (${user.email}) — ${user.role}`, ...list]);
        this.form = { ...EMPTY_FORM };
      },
      error: (err) => {
        this.saving.set(false);
        const message = Array.isArray(err?.error) ? err.error.join(' ') : (err?.error ?? 'Could not create the user.');
        this.error.set(message);
      }
    });
  }
}
