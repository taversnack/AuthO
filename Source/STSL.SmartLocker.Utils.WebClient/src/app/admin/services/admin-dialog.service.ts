import { Injectable, TemplateRef, inject } from '@angular/core';
import { Observable, Subject, bufferCount, concatMap, filter, from, identity, map, of, takeUntil, tap } from 'rxjs';
import { AuthService } from 'src/app/auth/services/auth.service';
import { DragAndDropListDialogComponent, IDragAndDropListDialogData } from 'src/app/dialog/components/drag-and-drop-list-dialog/drag-and-drop-list-dialog.component';
import { IOpenFileDialogData } from 'src/app/dialog/components/open-file-dialog/open-file-dialog.component';
import { DialogService, IAlertDialogSettings } from 'src/app/dialog/services/dialog.service';
import { checkEntityIdEquality, getFirstLineValuesFromCsv, includesAll, parseSimpleCsvDataAsObjects, parseSimpleCsvLinesAsObjects, populatedArray, splitAndTrimCsv, streamAsTextLineArrayObservable } from 'src/app/shared/lib/utilities';
import { ICardCredentialDTO } from 'src/app/shared/models/card-credential';
import { ICardHolderAndCardCredentialsDTO } from 'src/app/shared/models/card-credential-and-card-holder';
import { ICardHolderDTO } from 'src/app/shared/models/card-holder';
import { CustomError } from 'src/app/shared/models/error';
import { ILocationDTO } from 'src/app/shared/models/location';
import { ILockDTO } from 'src/app/shared/models/lock';
import { ILockerDTO } from 'src/app/shared/models/locker';
import { ILockerBankDTO, LockerBankBehaviour } from 'src/app/shared/models/locker-bank';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';
import { IViewReferenceImageDTO } from 'src/app/shared/models/reference-image';
import { EditCardCredentialDialogComponent, IEditCardCredentialDialogData } from '../admin-common/dialogs/edit-entities/edit-card-credential-dialog/edit-card-credential-dialog.component';
import { EditCardHolderDialogComponent, IEditCardHolderDialogData } from '../admin-common/dialogs/edit-entities/edit-card-holder-dialog/edit-card-holder-dialog.component';
import { EditLocationDialogComponent, IEditLocationDialogData } from '../admin-common/dialogs/edit-entities/edit-location-dialog/edit-location-dialog.component';
import { EditLockDialogComponent, IEditLockDialogData } from '../admin-common/dialogs/edit-entities/edit-lock-dialog/edit-lock-dialog.component';
import { EditLockerBankDialogComponent, IEditLockerBankDialogData } from '../admin-common/dialogs/edit-entities/edit-locker-bank-dialog/edit-locker-bank-dialog.component';
import { EditLockerDialogComponent, IEditLockerDialogData } from '../admin-common/dialogs/edit-entities/edit-locker-dialog/edit-locker-dialog.component';
import { EnrolCardCredentialDialogComponent, ICardCredentialCsnHidPair, IEnrolCardCredentialDialogData } from '../admin-common/dialogs/edit-entities/enrol-card-credential-dialog/enrol-card-credential-dialog.component';
import { ISelectCardHoldersDialogData, SelectCardHoldersDialogComponent } from '../admin-common/dialogs/view-entities/select-card-holders-dialog/select-card-holders-dialog.component';
import { ISelectCardHoldersOrCardCredentialsDialogData, SelectCardHoldersOrCardCredentialsDialogComponent } from '../admin-common/dialogs/view-entities/select-card-holders-or-card-credentials-dialog/select-card-holders-or-card-credentials-dialog.component';
import { ISelectCardsFromListDialogData, SelectCardsFromListDialogComponent } from '../admin-common/dialogs/view-entities/select-cards-from-list-dialog/select-cards-from-list-dialog.component';
import { ISelectLockerBanksDialogData, SelectLockerBanksDialogComponent } from '../admin-common/dialogs/view-entities/select-locker-banks-dialog/select-locker-banks-dialog.component';
import { ISelectLockersDialogData, SelectLockersDialogComponent } from '../admin-common/dialogs/view-entities/select-lockers-dialog/select-lockers-dialog.component';
import { ISelectLocksDialogData, SelectLocksDialogComponent } from '../admin-common/dialogs/view-entities/select-locks-dialog/select-locks-dialog.component';
import { IViewLockerAuditsDialogData, ViewLockerAuditsDialogComponent } from '../admin-common/dialogs/view-entities/view-locker-audits-dialog/view-locker-audits-dialog.component';
import { IViewLockerLeasesDialogData, ViewLockerLeasesDialogComponent } from '../admin-common/dialogs/view-entities/view-locker-leases-dialog/view-locker-leases-dialog.component';
import { AdminApiService } from './admin-api.service';
import { EditReferenceImageDialogComponent, IEditReferenceImageDialogData } from '../admin-common/dialogs/edit-entities/edit-reference-image-dialog/edit-reference-image-dialog.component';
import { IHasReferenceImage, IUsesGuidId } from 'src/app/shared/models/common';
import { IViewReferenceImageDialogData, ViewReferenceImageDialogComponent } from '../admin-common/dialogs/view-entities/view-reference-image-dialog/view-reference-image-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class AdminDialogService {

  private readonly authService = inject(AuthService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly dialogService = inject(DialogService);

  // VIEW ENTITIES
  openViewLockerLeasesDialog(dialogSettings: IViewLockerLeasesDialogData) {
    this.dialogService.openLarge<undefined, ViewLockerLeasesDialogComponent, IViewLockerLeasesDialogData>(ViewLockerLeasesDialogComponent, 'Locker Lease History', dialogSettings);
  }

  openViewLockerAuditsDialog(lockerStatus: ILockerStatusDTO) {
    this.dialogService.openLarge<undefined, ViewLockerAuditsDialogComponent, IViewLockerAuditsDialogData>(
      ViewLockerAuditsDialogComponent,
      'View Locker Events',
      { currentlySelectedLocker: lockerStatus }
    );
  }
  
  viewReferenceImage(blobSASToken?: string) {
    this.dialogService.openLarge<undefined, ViewReferenceImageDialogComponent, IViewReferenceImageDialogData>(
      ViewReferenceImageDialogComponent,
      'Reference Image',
      {blobSASToken}
    );
  }

  // SELECT ENTITIES
  openSelectCardHoldersOrCardCredentialsDialog(dialogSettings: ISelectCardHoldersOrCardCredentialsDialogData, titleOverride?: string): Observable<ICardHolderAndCardCredentialsDTO[]> {
    const title = titleOverride ?? 'Assign ' + this.authService.cardHolderAliasPlural;
    return this.dialogService.openLarge<ICardHolderAndCardCredentialsDTO[], SelectCardHoldersOrCardCredentialsDialogComponent, ISelectCardHoldersOrCardCredentialsDialogData>(
      SelectCardHoldersOrCardCredentialsDialogComponent,
      title,
      dialogSettings
    ).pipe(filter((x): x is ICardHolderAndCardCredentialsDTO[] => x !== undefined && x !== null));
  }

  openSelectCardHoldersDialog(dialogSettings: ISelectCardHoldersDialogData = {}): Observable<ICardHolderDTO[]> {
    return this.dialogService.openLarge<ICardHolderDTO[], SelectCardHoldersDialogComponent, ISelectCardHoldersDialogData>(
      SelectCardHoldersDialogComponent,
      'Select ' + this.authService.cardHolderAliasPlural,
      dialogSettings,
    ).pipe(filter((x): x is ICardHolderDTO[] => x !== undefined && x !== null));
  }

  openSelectCardCredentialsFromList(dialogSettings: ISelectCardsFromListDialogData): Observable<ICardCredentialDTO[]> {
    return this.dialogService.openMedium<ICardCredentialDTO[], SelectCardsFromListDialogComponent, ISelectCardsFromListDialogData>(SelectCardsFromListDialogComponent,
      'Select Cards',
      dialogSettings
    ).pipe(filter((x): x is ICardCredentialDTO[] => x !== undefined && x !== null));
  }

  openSelectLockerBanksDialog(dialogSettings: ISelectLockerBanksDialogData = {}, title: string = 'Select Locker Banks'): Observable<ILockerBankDTO[]> {
    return this.dialogService.openLarge<ILockerBankDTO[], SelectLockerBanksDialogComponent, ISelectLockerBanksDialogData>(
      SelectLockerBanksDialogComponent,
      title,
      dialogSettings
    ).pipe(filter((x): x is ILockerBankDTO[] => x !== undefined));
  }

  openSelectLockersDialog(dialogSettings: ISelectLockersDialogData = {}, title: string = 'Select Lockers'): Observable<ILockerDTO[]> {
    return this.dialogService.openLarge<ILockerDTO[], SelectLockersDialogComponent, ISelectLockersDialogData>(
      SelectLockersDialogComponent,
      title,
      dialogSettings
    ).pipe(filter((x): x is ILockerDTO[] => x !== undefined));
  }

  openSelectLocksDialog(dialogSettings: ISelectLocksDialogData = {}, title: string = 'Select Locks'): Observable<ILockDTO[]> {
    return this.dialogService.openLarge<ILockDTO[], SelectLocksDialogComponent, ISelectLocksDialogData>(
      SelectLocksDialogComponent,
      title,
      dialogSettings
    ).pipe(filter((x): x is ILockDTO[] => x !== undefined));
  }

  // EDIT ENTITIES
  editLocation(dialogSettings: IEditLocationDialogData = {}): Observable<ILocationDTO> {
    return this.dialogService.open<ILocationDTO, EditLocationDialogComponent, IEditLocationDialogData>(
      EditLocationDialogComponent,
      dialogSettings.entity ? 'Edit Location ' + dialogSettings.entity.name : 'Create Location',
      dialogSettings
    ).pipe(filter((x): x is ILocationDTO => x !== undefined));
  }

  editReferenceImage<T extends IUsesGuidId & IHasReferenceImage>(dialogSettings: IEditReferenceImageDialogData<T> = {}) {
    return this.dialogService
    .openLarge<IViewReferenceImageDTO, EditReferenceImageDialogComponent<T>, IEditReferenceImageDialogData<T>>(
      EditReferenceImageDialogComponent<T>,
      "Reference Image",
      dialogSettings
    ).pipe(filter((x): x is IViewReferenceImageDTO => x !== undefined));
  }

  editLockerBank(dialogSettings: IEditLockerBankDialogData): Observable<ILockerBankDTO> {
    return this.dialogService.open<ILockerBankDTO, EditLockerBankDialogComponent, IEditLockerBankDialogData>(
      EditLockerBankDialogComponent,
      dialogSettings.entity ? 'Edit Locker Bank ' + dialogSettings.entity.name : 'Create Locker Bank',
      dialogSettings
    ).pipe(filter((x): x is ILockerBankDTO => x !== undefined && x !== null));
  }

  editLocker(dialogSettings: IEditLockerDialogData): Observable<ILockerDTO> {
    return this.dialogService.open<ILockerDTO, EditLockerDialogComponent, IEditLockerDialogData>(
      EditLockerDialogComponent,
      dialogSettings.entity ? 'Edit Locker ' + dialogSettings.entity.label : 'Create Locker',
      dialogSettings
    ).pipe(filter((x): x is ILockerDTO => x !== undefined && x !== null));
  }

  editLock(dialogSettings: IEditLockDialogData = {}): Observable<ILockDTO> {
    return this.dialogService.open<ILockDTO, EditLockDialogComponent, IEditLockDialogData>(
      EditLockDialogComponent,
      dialogSettings.lock ? 'Edit Lock ' + dialogSettings.lock.serialNumber : 'Create Lock',
      dialogSettings
    ).pipe(filter((x): x is ILockDTO => x !== undefined && x !== null));
  }

  editCardHolder(dialogSettings: IEditCardHolderDialogData): Observable<ICardHolderDTO> {
    const cardHolderAliasSingular = this.authService.cardHolderAliasSingular;
    return this.dialogService.open<ICardHolderDTO, EditCardHolderDialogComponent, IEditCardHolderDialogData>(
      EditCardHolderDialogComponent,
      (dialogSettings.entity ? 'Edit ' : 'Create ') + cardHolderAliasSingular,
      dialogSettings
    ).pipe(filter((x): x is ICardHolderDTO => x !== undefined && x !== null));
  }

  editCardCredential(dialogSettings: IEditCardCredentialDialogData): Observable<ICardCredentialDTO> {
    return this.dialogService.open<ICardCredentialDTO, EditCardCredentialDialogComponent, IEditCardCredentialDialogData>(
      EditCardCredentialDialogComponent,
      dialogSettings.cardCredential ? 'Edit Card Credential' : 'Create Card Credential',
      dialogSettings
    ).pipe(filter((x): x is ICardCredentialDTO => x !== undefined && x !== null));
  }

  // MISC ENTITY SAVING UTILITY DIALOGS
  moveLockersFromOneBankToAnother(originLockerBank?: ILockerBankDTO): Observable<{ originLockerBank?: ILockerBankDTO, destinationLockerBank?: ILockerBankDTO, movedLockers: ILockerDTO[] }> {
    const dialogSettings: ISelectLockerBanksDialogData = {
      allowMultipleLockerBanks: false,
    };

    let destinationLockerBank: ILockerBankDTO | undefined;
    let movedLockers: ILockerDTO[];

    const startingDialog = originLockerBank ? of([originLockerBank]) : this.openSelectLockerBanksDialog(dialogSettings, 'Choose origin locker bank')
      .pipe(tap(selectedOriginLockerBank => originLockerBank = selectedOriginLockerBank[0]));

    return startingDialog.pipe(
      concatMap(() => this.openSelectLockerBanksDialog(dialogSettings, 'Choose destination locker bank')),
      concatMap(lockerBanks => {
        destinationLockerBank = lockerBanks[0];
        if(!originLockerBank || checkEntityIdEquality(destinationLockerBank.id, originLockerBank.id)) {
          throw new CustomError('Cannot move lockers from one bank to itself. Origin and destination must be different');
        }
        return this.openSelectLockersDialog({
          currentlySelectedLockerBank$: of(originLockerBank),
          allowMultipleLockers: true,
          hideLocations: true,
          hideLockerBanks: true,
          allowSelectingNonSmartLockers: destinationLockerBank.behaviour !== LockerBankBehaviour.Temporary
        });
      }),
      concatMap(lockers => {
        if(!originLockerBank || !destinationLockerBank?.id) {
          throw new CustomError('Something went wrong, could not use the destination or origin locker bank');
        }
        movedLockers = lockers;
        return this.adminApiService.LockerBanks.Lockers.Move(originLockerBank.id, {
          destination: destinationLockerBank.id,
          lockerIds: lockers.map(({ id }) => id)
        });
      }),
      filter(success => success),
      map(() => ({ originLockerBank, destinationLockerBank, movedLockers })),
    );
  }

  // MISC DIALOGS
  openEnrolCardCredentialDialog(dialogSettings: IEnrolCardCredentialDialogData = {}): Observable<ICardCredentialCsnHidPair> {
    return this.dialogService.open<ICardCredentialCsnHidPair, EnrolCardCredentialDialogComponent, IEnrolCardCredentialDialogData>(
      EnrolCardCredentialDialogComponent,
      'Enrol Card',
      dialogSettings,
    ).pipe(filter((x): x is ICardCredentialCsnHidPair => x !== undefined && x !== null));
  }

  openDragAndDropListDialog(dialogSettings: IDragAndDropListDialogData, title: string): Observable<string[]> {
    return this.dialogService.open<string[], DragAndDropListDialogComponent, IDragAndDropListDialogData>(
      DragAndDropListDialogComponent,
      title,
      dialogSettings
    ).pipe(filter((x): x is string[] => x !== undefined));
  }

  // NOTE: It would be wise to examine file size and use stream + chunking of data for large files
  getCsvContents<T extends Record<string, string>, K extends keyof T & string>(columnHeaders: K[]): Observable<Record<string, string>[]> {
    return this.openCsvFileUpload().pipe(
      concatMap(file => from(file.text()).pipe(filter(x => x !== ''), concatMap(text => {
          const firstLine = getFirstLineValuesFromCsv(text);

          if(includesAll(firstLine, columnHeaders)) {
            // If the CSV has correct column headers in the first line, parse the file
            return of(parseSimpleCsvDataAsObjects(text, { firstLineIsFieldNames: true }));
          } else {
            // Otherwise arrange the CSV columns, then parse the file
            // Check we have enough columns,
            const numberOfColumnsDifference = firstLine.length - columnHeaders.length;
            if(numberOfColumnsDifference < 0) {
              throw new CustomError('The CSV file does not have enough columns');
            }

            return this.openArrangeCsvColumnsDialogs(columnHeaders, numberOfColumnsDifference).pipe(
              map(({ columnsInOrder, ignoreFirstLine }) => {
                // Redundant columns if any filtered out
                const parsedObjects = parseSimpleCsvDataAsObjects<T>(text, { keys: columnsInOrder });
                return ignoreFirstLine ? parsedObjects?.slice(1) : parsedObjects;
              }),
            );
          }
        }))
      )
    );
  }

  getCsvContentsStream<T extends Record<string, string>, K extends keyof T & string>(columnHeaders: K[]): Observable<Record<string, string>[]> {
    let columnsInOrder: K[] = columnHeaders;

    return this.openCsvFileUpload().pipe(
      concatMap(file => streamAsTextLineArrayObservable(file.stream())),
      filter(lines => lines.length > 0),
      concatMap((lines, index) => {
        if(index === 0) {
          // Get the first line of the CSV, check if it has headings matching our columnHeaders,
          // if so, we open a dialog to arrange the columns correctly
          const firstLine = splitAndTrimCsv(lines[0]);
          if(!includesAll(firstLine, columnHeaders)) {
            const numberOfColumnsDifference = firstLine.length - columnHeaders.length;
            if(numberOfColumnsDifference < 0) {
              throw new CustomError('The CSV file does not have enough columns');
            }

            return this.openArrangeCsvColumnsDialogs(columnHeaders, numberOfColumnsDifference).pipe(
              map(({ columnsInOrder: headersInOrder, ignoreFirstLine }) => {
                // Redundant columns if any filtered out
                columnsInOrder = headersInOrder as K[];
                const parsedObjects = parseSimpleCsvLinesAsObjects<T>(lines, { keys: columnsInOrder });
                return ignoreFirstLine ? parsedObjects?.slice(1) : parsedObjects;
              }),
            );
          }
        }

        return of(parseSimpleCsvLinesAsObjects<T>(lines, { keys: columnsInOrder }));
      }),
      // Example of how you could auto map to remove any redundant columns and only include columnHeaders but limitation is string values only
      // One way to get around the string value limitation is actually to extend the columnHeaders to be string | IParsedColumn with name and type
      // i.e. if type is declared to be 'number' or 'boolean', use parseInt or cast TRUE | FaLsE
      // map(objects => objects.map(object => {
      //   Object.entries(object).filter(([key]) => columnHeaders.includes(key as K)).reduce((obj, [key, value]) => ({ ...obj, [key]: value }), {})
      // }))
    );
  }

  openAndParseCsvAndSendToEndpointInChunks<T>(settings: ICsvChunkUploaderSettings<T>): Observable<IChunkUploadResult> {
    return this.getCsvContentsStream(settings.columnHeaders).pipe(
      // split line by line,
      concatMap(lines => from(lines)),
      // map & where mapper returns undefined filter out,
      map(line => settings.rowMapper(line)),
      filter((x): x is T => x !== undefined),
      // chunk,
      bufferCount(settings.chunkSize),
      // send to endpoint,
      concatMap(lines =>
        // Add catchError pipe which returns caughtError & completedSuccessfully: false
        settings.endpoint(lines).pipe(map(completedSuccessfully => ({ completedSuccessfully, chunkSize: lines.length })))
      ),
      // show in dialog progress bar with chunks completed / errors etc
      map(({ completedSuccessfully, chunkSize }, chunkIndex) => ({ chunkIndex, chunkSize, completedSuccessfully, })),
      // maybe move this above the endpoint? Depends where cancellation should happen in pipeline
      settings.cancellationSubject ? takeUntil(settings.cancellationSubject) : identity,
    );
  }

  // TODO: Create a nice alert-like dialog that can display continually updating info
  // openAndParseCsvAndSendToEndpointInChunksWithProgressDialog<T>(settings: ICsvChunkUploaderSettings<T>): Observable<IChunkUploadResult> {
  //   this.dynamicAlert()
  //   this.openAndParseCsvAndSendToEndpointInChunks<T>(settings).pipe(

  //   )
  // }

  private openCsvFileUpload(): Observable<File> {
    const accept = ['text/csv', 'text/plain', 'application/vnd.ms-excel'];
    // NOTE: Requires i18n
    const title = 'Select CSV File';
    const maxFileCount = 1;

    return this.dialogService.openFileUpload({ accept, maxFileCount }, title).pipe(
      filter((files): files is File[] => files?.length !== undefined && files.length > 0),
      map(files => files[0])
    );
  }

  private openArrangeCsvColumnsDialogs(requiredColumnHeaders: string[], numberOfRedundantColumns: number): Observable<{ columnsInOrder: string[], ignoreFirstLine: boolean }> {
    const redundantFieldName = 'Redundant Column ';
    const dragAndDropDialogTitle = 'Order your CSV columns';
    const [ignoreHeadingsDialogTitle, ignoreHeadingsDialogMessage] = ['Are you using headings?', 'Do you want to ignore the first line / row?'];
    const [confirmActionLabel, cancelActionLabel] = ['Yes', 'No'];
    const listItems: string[] = [...requiredColumnHeaders, ...populatedArray(numberOfRedundantColumns, x => redundantFieldName + x)];

    return this.openDragAndDropListDialog({ listItems }, dragAndDropDialogTitle).pipe(
      concatMap(columnsInOrder =>
        this.dialogService.confirm(ignoreHeadingsDialogMessage, ignoreHeadingsDialogTitle, { confirmActionLabel, cancelActionLabel }).pipe(
          map(ignoreFirstLine => ({ columnsInOrder, ignoreFirstLine }))
        )
      )
    );
  }

  // Convenience functions to allow underlying dialogService functions
  alert(message?: string | string[], title: string = 'Alert', settings?: IAlertDialogSettings): Observable<undefined> {
    return this.dialogService.alert(message, title, settings);
  }

  error(message?: string | string[], title: string = 'Error', settings?: IAlertDialogSettings): Observable<undefined> {
    return this.dialogService.error(message, title, settings);
  }

  confirm(message?: string | string[], title: string = 'Are you sure?'): Observable<boolean> {
    return this.dialogService.confirm(message, title);
  }

  openFileUpload(settings: IOpenFileDialogData = {}, title: string = 'Select File(s)'): Observable<File[] | undefined> {
    return this.dialogService.openFileUpload(settings, title);
  }

  /**
   * @deprecated Currently unused - not implemented
   */
  openTemplate<T = any>(template: TemplateRef<T>, title: string) {
    return this.dialogService.openTemplate(template, title);
  }

  showIsLoading(isLoading$: Observable<boolean>, useAlertMessage?: string) {
    this.dialogService.showIsLoading(isLoading$, useAlertMessage);
  }
}

// TODO: Split some CSV functionality up perhaps into a dedicated CSV service
// which depends on (injects) dialogService (doesn't need to be adminDialogService, not using any admin specific dialogs)
export interface ICsvChunkUploaderSettings<T> {
  chunkSize: number;
  columnHeaders: string[];
  endpoint: (dtoList: T[]) => Observable<boolean>;
  rowMapper: (csvRow: Record<string, string>) => T | undefined;
  cancellationSubject?: Subject<ICancellationToken>;
}

export interface IChunkUploadResult {
  chunkIndex: number;
  chunkSize: number;
  completedSuccessfully: boolean;
  // Retries handled by api service usually, probably not practical to have retryAttempts here..
  // retryAttempts?: number;
  caughtError?: Error;
}


// Put all this into utils / shared models
const enum CancellationSource {
  Unknown,
  User,
  Application,
  Network,
}

export interface ICancellationToken {
  source: CancellationSource;
  reason?: string;
}

export class CancellationToken implements ICancellationToken {
  constructor(public reason?: string, public source: CancellationSource = CancellationSource.Unknown) {}
}
