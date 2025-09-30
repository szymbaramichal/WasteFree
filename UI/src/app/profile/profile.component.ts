import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../pipes/translate.pipe';
import { ProfileService } from '../services/profile.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  profileSvc = inject(ProfileService);
  editMode = false;
  draftDescription = '';
  saving = false;
  saveOk = false;
  saveError = false;

  editBank = false;
  draftBank = '';
  savingBank = false;
  saveBankOk = false;
  saveBankError = false;
  ibanInvalid = false;

  ngOnInit(): void {
    this.profileSvc.refresh();
  }

  startEdit(current: string | undefined | null) {
    this.saveOk = false;
    this.saveError = false;
    this.draftDescription = current || '';
    this.editMode = true;
  }

  cancel() {
    this.editMode = false;
    this.saving = false;
    this.saveOk = false;
    this.saveError = false;
  }

  save() {
    const value = this.draftDescription ?? '';
    this.saving = true;
    this.saveOk = false;
    this.saveError = false;
    this.profileSvc.updateDescription(value).subscribe({
      next: () => {
        this.saving = false;
        this.saveOk = true;
        this.editMode = false;
        this.profileSvc.refresh();
      },
      error: () => {
        this.saving = false;
        this.saveError = true;
      }
    });
  }

  startEditBank(current: string | undefined | null) {
    this.saveBankOk = false;
    this.saveBankError = false;
    this.draftBank = current || '';
    this.editBank = true;
  }

  cancelBank() {
    this.editBank = false;
    this.savingBank = false;
    this.saveBankOk = false;
    this.saveBankError = false;
  }

  saveBank() {
    const value = (this.draftBank ?? '').replace(/\s+/g, '').toUpperCase();
    // Basic IBAN validation: length and checksum
    this.ibanInvalid = !this.validateIban(value);
    if (this.ibanInvalid) {
      return;
    }
    this.savingBank = true;
    this.saveBankOk = false;
    this.saveBankError = false;
    this.profileSvc.updateProfile({ bankAccountNumber: value }).subscribe({
      next: () => {
        this.savingBank = false;
        this.saveBankOk = true;
        this.editBank = false;
        this.draftBank = value;
        this.profileSvc.refresh();
      },
      error: () => {
        this.savingBank = false;
        this.saveBankError = true;
      }
    });
  }

  private validateIban(iban: string): boolean {
    const raw = (iban ?? '').toUpperCase();
    const basicOk = /^[A-Z0-9]{15,34}$/.test(raw);
    const rearranged = raw.slice(4) + raw.slice(0, 4);
    const expanded = rearranged.replace(/[A-Z]/g, c => (c.charCodeAt(0) - 55).toString());
    let remainder = 0;
    for (let i = 0; i < expanded.length; i++) {
      remainder = (remainder * 10 + (expanded.charCodeAt(i) - 48)) % 97;
    }
    return basicOk && remainder === 1;
  }
}

