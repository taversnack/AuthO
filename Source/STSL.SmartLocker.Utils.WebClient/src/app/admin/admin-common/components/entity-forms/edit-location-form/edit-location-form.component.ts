import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, inject } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { Observable, Subscription, of } from 'rxjs';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { textControlFromSettings } from 'src/app/shared/components/text-input/text-input.component';
import { ICreateLocationDTO, ILocationDTO, IUpdateLocationDTO } from 'src/app/shared/models/location';

@Component({
  selector: 'admin-edit-location-form',
  templateUrl: './edit-location-form.component.html',
})
export class EditLocationFormComponent implements OnInit, OnDestroy {

  private readonly dialogService = inject(DialogService);

  @Input() location$: Observable<ILocationDTO | undefined> = of();

  @Output() locationSubmitted = new EventEmitter<ICreateLocationDTO | IUpdateLocationDTO>();

  locationSubscription?: Subscription;

  hasLocation = false;

  readonly locationNameSettings = {
    name: 'name',
    required: true,
    maxLength: 256,
  };

  readonly locationDescriptionSettings = {
    name: 'description',
    maxLength: 256,
  };

  locationForm = new FormGroup({});

  locationNameControl = textControlFromSettings(this.locationNameSettings, this.locationForm);
  locationDescriptionControl = textControlFromSettings(this.locationDescriptionSettings, this.locationForm);

  ngOnInit(): void {
    this.locationSubscription = this.location$.subscribe(location => {
      if(location) {
        this.hasLocation = true;
        this.locationForm.patchValue(location);
      } else {
        this.hasLocation = false;
        this.locationForm.patchValue({ name: '', description: '' });
      }
    });
  }

  ngOnDestroy(): void {
    this.locationSubscription?.unsubscribe();
  }

  submitForm() {
    if(this.locationForm.invalid) {
      this.dialogService.error("Ensure all required properties are filled in and valid", "Validation Error");
      return;
    }

    const location = this.locationForm.value;
    this.locationSubmitted.emit(location as ICreateLocationDTO | IUpdateLocationDTO);
  }
}
