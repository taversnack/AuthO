import * as moment from "moment";
import { BehaviorSubject, Observable, catchError, defer, finalize, from } from "rxjs";
import { DateTime, EntityId, NewlineCharacter } from "../models/common";

/**
 * Creates an array of length using a fill value or generator function
 * @param count The length of the array
 * @param element A static element to fill the array with or a function to generate elements
 * @returns An array of length `count`
 */
export const populatedArray = <T>(
  count: number,
  element: T | ((index: number) => T)
): T[] => {
  if (element instanceof Function) {
    return [...Array(count)].map((_, index) => element(index));
  }
  return Array(count).fill(element);
}

/**
 * Gets an array of elements that are contained in both arrays.
 * If you want to check if one array contains all elements of another
 * use {@link includesAll} instead.
 *
 * @see {@link includesAll}
 * @param first The first array
 * @param second The second array
 * @param comparator The optional comparator function used to check element equality
 * @returns An array containing the subset of matching elements from first and second
 */
export const intersection = <T>(first: T[], second: T[], comparator: ((lhs: T, rhs: T) => boolean) = (lhs, rhs) => lhs === rhs): T[] =>
  first.filter(x => second.some(y => comparator(x, y)));

/**
 * Checks an array to see if it includes all the subset elements. \
 * Can be more efficient than using {@link intersection}:
 * ```
 * intersection(larger, potentialSubset).length === potentialSubset.length
 * ```
 * @see {@link intersection}
 * @param larger The larger array for which to check contains the subset
 * @param potentialSubset The subset for which larger is checked to include
 * @param comparator The optional comparator function used to check element equality
 * @returns boolean indicating if larger contains all potentialSubset elements
 */
export const includesAll = <T>(larger: T[], potentialSubset: T[], comparator: ((lhs: T, rhs: T) => boolean) = (lhs, rhs) => lhs === rhs): boolean => {
  if(!potentialSubset.length) {
    return true;
  }

  const remaining = [...potentialSubset];

  // loop through remaining elements to check
  for (let index = 0; index < remaining.length; index++) {
    // loop through this array
    for (const largerIterator of larger) {
      // if we find a matching element pair
      if(comparator(largerIterator, remaining[index])) {
        // remove matching element from remaining then decrement index since array shrinks by 1
        remaining.splice(index--);
        // break back to outer loop
        break;
      }
    }
  }
  // did we match all elements
  return remaining.length === 0;
}

/**
 * Creates a range of numbers \
 * stop is not relative to start i.e. start: 6, stop: 7, step: 1 would only be 1 step not 7
 *
 * @param start Where to start the range
 * @param stop Where to stop the range
 * @param step The incremental step from start to stop, 1 by default
 * @returns An array of numbers between start and stop going up by step \
 * \
 * If (stop - start) isn't divisible by step exactly then the range is exclusive of the stop value \
 * If start is less than stop you must provide a negative step, otherwise this function will throw
 */
export const absoluteRange = (start: number, stop: number, step: number = 1): number[] => {
  step ||= 1;
  if((step > 0 && start < stop) || (step < 0 && start < stop)) {
      return absoluteRange(stop, start, step);
  }

  return populatedArray((stop - start) / step, x => (x * step) + start);
}

/**
* Creates a range of numbers \
* stop is relative to start i.e. start: 6, stop: 7, step: 1 would be 7 steps (ending at 13)
*
* @param start Where to start the range
* @param range The end point relative to start
* @param step The incremental step from start to stop, 1 by default
* @returns An array of numbers between start and start + stop going up by step \
* \
* If range isn't divisible by step exactly then the range is exclusive of the stop value \
* If range is negative you must provide a negative step, otherwise this function will throw
*/
export const relativeRange = (start: number, range: number, step: number = 1): number[] =>
  absoluteRange(start, start + range, step);

/**
 * Checks EntityId types for value equality
 *
 * @param first The first EntityId
 * @param second The second EntityId
 * @returns boolean result of whether EntityIds are considered equal
 */
export const checkEntityIdEquality = (first: EntityId, second: EntityId) => first.toLowerCase() === second.toLowerCase();

export const camelCaseToSpacedPascalCase = (value: string): string => {
  const valueWithSpaces = value.replace(/([a-z])([A-Z])/g, "$1 $2");
  return valueWithSpaces[0].toUpperCase() + valueWithSpaces.slice(1);
}

/**
 * Returns a new array with an item removed without mutating the original array
 *
 * @param arr The input array
 * @param index The index to remove
 * @returns A new array with the item at the specified index removed
 */
export const arrayWithRemovedIndex = <T>(arr: T[], index: number): T[] =>
  index === 0 ? arr.slice(index + 1) : [...arr.slice(0, index), ...arr.slice(index + 1)];

/**
 * Returns a new array with an item replaced at a specific index without mutating the original array
 *
 * @param arr The input array
 * @param index The index to remove
 * @param replacement The item to add in place of the removed index
 * @returns A new array with the item at the specified index removed
 */
export const arrayWithReplacedIndex = <T>(arr: T[], index: number, replacement: T): T[] =>
  index === 0 ? [replacement, ...arr.slice(index + 1)] : [...arr.slice(0, index), replacement, ...arr.slice(index + 1)];

export const arrayWithRemovedElement = <T extends string | number | boolean | object | symbol>(arr: T[], element: T | ((elem: T, index: number) => boolean)): T[] => {
  const predicate = typeof element === 'function' ? element : (x: T) => x === element;
  const found = arr.findIndex(predicate);
  return found >= 0 ? arrayWithRemovedIndex(arr, found) : arr;
}

export const arrayWithReplacedElement = <T extends string | number | boolean | object | symbol>(arr: T[], oldElement: T | ((elem: T, index: number) => boolean), newElement: T): T[] => {
  const predicate = typeof oldElement === 'function' ? oldElement : (x: T) => x === oldElement;
  const found = arr.findIndex(predicate);
  return found >= 0 ? arrayWithReplacedIndex(arr, found, newElement) : arr;
}

export const localToUtc = (dateTime: DateTime) => moment(dateTime).utc();

export const utcToLocal = (dateTime: DateTime) => moment.utc(dateTime).local();

export const reverseHexStringEndianness = (hexString: string): string => [...(hexString.length % 2 ? hexString.padStart(hexString.length + 1, '0') : hexString).matchAll(/\w{2}/g)].reverse().join('');

export const decimalCsnToLockFormat = (csn: string): string => reverseHexStringEndianness(BigInt(csn).toString(16)).padEnd(16, '0');

export const lockFormatCsnToDecimal = (csn: string): string => {
  const reversedCsn = reverseHexStringEndianness(csn);
  return BigInt(reversedCsn.startsWith('0x') ? reversedCsn : '0x' + reversedCsn).toString();
}

// TODO: Create a class CsvReader / TsvReader which takes in settings e.g. newLineCharacters (CR | LF | CRLF), separatorCharacter etc
// Could use a TabularDataReader base class and Csv, Tsv are specialized classes with special default settings e.g. separatorCharacter
// Makes more extensible as new instances can be created with common settings
export const readSimpleCsvData = (text: string, firstLineIsFieldNames = false): string[][] => {
  const lines = text.split(NewlineCharacter);

  if(!lines.length || (firstLineIsFieldNames && lines.length < 2)) {
    return [];
  }

  const firstLine = lines[0].split(',');
  const fieldCount = firstLine.length;

  let records: string[][] = [];

  if(firstLineIsFieldNames) {
    lines.splice(0, 1);
  }

  for (const line of lines) {
    const values = line.split(',');
    if(fieldCount === values.length) {
      records.push(values);
    }
  }

  return records;
}

export interface IUsesFirstLineAsHeadingsToParseCsv {
  firstLineIsFieldNames: true;
}

export interface IUsesStringKeysToParseCsv<T> {
  keys: (string & keyof T)[];
}

export interface ICsvObjectReaderSettings<T> extends Partial<IUsesStringKeysToParseCsv<T>>, Partial<IUsesFirstLineAsHeadingsToParseCsv> {}

// export function parseSimpleCsvLinesAsObjects<T extends { [key: string]: string }>(lines: string[], { keys }: IUsesStringKeysToParseCsv<T>): { [key: string]: string }[];
// export function parseSimpleCsvLinesAsObjects<T extends { [key: string]: string }>(lines: string[], { firstLineIsFieldNames }: IUsesFirstLineAsHeadingsToParseCsv): { [key: string]: string }[];
export function parseSimpleCsvLinesAsObjects<T extends { [key: string]: string } = { [key: string]: string }>(lines: string[], { firstLineIsFieldNames, keys }: ICsvObjectReaderSettings<T>): { [key: string]: string }[] {
  if(!lines.length || (firstLineIsFieldNames && lines.length < 2)) {
    return [];
  }

  const firstLine = splitAndTrimCsv(lines[0]);
  const fieldCount = firstLine.length;

  let records: { [key: string]: string }[] = [];

  if(firstLineIsFieldNames) {
    lines.splice(0, 1);
  }

  const fieldNames = keys?.length ? keys: firstLine;

  for (const line of lines) {
    const values = line.split(',');
    if(fieldCount === values.length) {
      let record: { [key: string]: string } = {};
      for(let i = 0; i < fieldCount; i++) {
        record[fieldNames[i]] = values[i];
      }
      records.push(record);
    }
  }
  return records;
}

export const parseSimpleCsvDataAsObjects = <T extends { [key: string]: string } = { [key: string]: string }>(text: string, options: ICsvObjectReaderSettings<T>): { [key: string]: string }[] =>
  parseSimpleCsvLinesAsObjects<T>(text.split(NewlineCharacter), options);

export const getFirstLineValuesFromCsv = (text: string): string[] => text.match(/.*/)?.shift()?.split(',').map(x => x.trim()) ?? [];

export const splitAndTrimCsv = (text: string): string[] => text.split(',').map(x => x.trim());

export const useLoading = <T>(observable: Observable<T>): { isLoading$: Observable<boolean>, source$: Observable<T>, finishLoading: () => void } => {
  const isLoading$ = new BehaviorSubject<boolean>(false);

  const finishLoading = () => {
    isLoading$.next(false);
    isLoading$.complete();
  };

  return {
    isLoading$: isLoading$.asObservable(),
    source$: defer(() => {
      isLoading$.next(true);
      return observable.pipe(
        catchError((err, caught) => {
          finishLoading();
          throw err;
        }),
        finalize(() => finishLoading())
      );
    }),
    finishLoading
  };
}

export const sortByString = (a: string, b: string, descending: boolean = false): number => descending ? a > b ? -1 : a < b ? 1 : 0 : a < b ? -1 : a > b ? 1 : 0;

export const sortByStringProperty = <T extends { [key: string | number]: string }>(a: T, b: T, getterOrKey: keyof T | ((x: T) => string), descending: boolean = false): number =>
  typeof getterOrKey === 'function' ? sortByString(getterOrKey(a), getterOrKey(b), descending) : sortByString(a[getterOrKey], b[getterOrKey], descending);

export const streamAsAsyncIterable = async function* <T = Uint8Array>(stream: ReadableStream<T>): AsyncGenerator<T> {
  const reader = stream.getReader();
  try {
    while (true) {
      const { done, value } = await reader.read();
      if (done) return;
      yield value;
    }
  } finally {
    reader.releaseLock();
  }
}

export const streamAsObservable = <T = Uint8Array>(stream: ReadableStream<T>): Observable<T> => from(streamAsAsyncIterable(stream));

export const streamAsTextObservable = (stream: ReadableStream): Observable<string> => streamAsObservable(stream.pipeThrough(new TextDecoderStream()));

export const streamAsTextLineArrayObservable = (stream: ReadableStream): Observable<string[]> => streamAsObservable(stream.pipeThrough(new TextDecoderStream()).pipeThrough(new TransformStream(new LineBreakTransformer())));

/*
export const streamAsTextLineArrayObservable = (stream: ReadableStream): Observable<string[]> => {

  const lastUnfinishedLineSubject = new BehaviorSubject<string | undefined>(undefined);

  return streamAsTextObservable(stream).pipe(
    withLatestFrom(lastUnfinishedLineSubject.asObservable()),
    map(([chunk, lastUnfinishedLine]) => {

      const firstCharIsNewline = NewlineCharacter.test(chunk[0]);
      const lastCharIsNewline = NewlineCharacter.test(chunk[chunk.length - 1]);

      const chunkLines = chunk.split(NewlineCharacter);

      if(lastUnfinishedLine !== undefined) {
        if(firstCharIsNewline || !chunkLines.length) {
          chunkLines.unshift(lastUnfinishedLine);
        } else {
          chunkLines[0] = lastUnfinishedLine + chunkLines[0];
        }
      }

      if(lastCharIsNewline) {
        // Need to check if last value in chunkLines is empty if split on last character?
        const lastLine = chunkLines.pop();
        lastUnfinishedLineSubject.next(lastCharIsNewline ? undefined : lastLine ?? chunk);
      }

      return chunkLines;
    })
  );
}
*/

class LineBreakTransformer implements Transformer<string, string[]> {
  remainder: string = '';

  transform(chunk: string | number, controller: { enqueue: (_: string[]) => void; }) {
    this.remainder += chunk;
    const lines = this.remainder.split(NewlineCharacter);
    this.remainder = lines.pop() ?? '';
    controller.enqueue(lines);
  }

  flush(controller: { enqueue: (_: string[]) => void; }) {
    controller.enqueue([this.remainder]);
  }
}

async function* makeTextFileLineIterator(stream: ReadableStream) {
  let reader = stream.pipeThrough(new TextDecoderStream()).getReader();
  let { value: chunk, done } = await reader.read();

  let newLineRegex = new RegExp(NewlineCharacter, 'gm');
  let startIndex = 0;

  for (;;) {
    chunk ??= '';
    let result = newLineRegex.exec(chunk);
    if (!result) {
      if (done) {
        break;
      }
      let remainder = chunk.substring(startIndex);
      ({ value: chunk, done } = await reader.read());
      chunk = remainder + chunk;
      startIndex = newLineRegex.lastIndex = 0;
      continue;
    }
    yield chunk.substring(startIndex, result.index);
    startIndex = newLineRegex.lastIndex;
  }
  if (startIndex < chunk.length) {
    // last line didn't end in a newline char
    yield chunk.substring(startIndex);
  }
}

export function convertBatteryVoltageToPercentage(voltage: number): string {
  // Coefficients
  const c1 = 188.7230365;
  const c2 = -3836.230;
  const c3 = 30973.71818;
  const c4 = -124152.295;
  const c5 = 247116.5623;
  const c6 = -195479.5779;

  // Apply formula
  const percentage = c1 * Math.pow(voltage, 5) + c2 * Math.pow(voltage, 4) + c3 * Math.pow(voltage, 3) + c4 * Math.pow(voltage, 2) + c5 * voltage + c6;
  
  // Round the result and ensure it's within 0-100%
  const roundedPercentage = Math.min(Math.max(Math.round(percentage), 0), 100);
  return `${roundedPercentage}%`;
}
