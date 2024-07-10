import { Component, inject } from '@angular/core';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
})
export class FooterComponent {

  private readonly dialogService = inject(DialogService);

  private readonly appVersion = environment.appVersion;

  private readonly latestReleaseNotes = `App Version: ${this.appVersion}`;

  showAppInfo() {
    this.dialogService.alert(this.latestReleaseNotes, '© 2023 - STSL - All rights reserved')
  }
}
