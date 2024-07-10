import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FeatureFlagComponent } from './components/feature-flag.component';

@NgModule({
    declarations: [
        FeatureFlagComponent,
    ],
    exports: [
        FeatureFlagComponent,
    ],
    imports: [
        CommonModule
    ]
})
export class FeatureFlagModule { }
