import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ReadingSnapshot {
  type: string;
  name: string;
  numericValue?: number | null;
  boolValue?: boolean | null;
  payloadJson: string;
  lastEventId: string;
  updatedAtUtc: string;
}

export interface TypeAggregation {
  type: string;
  count: number;
  sumNumeric?: number | null;
  avgNumeric?: number | null;
}

export interface LocationAggregation {
  name: string;
  count: number;
  sumNumeric?: number | null;
  avgNumeric?: number | null;
}

export interface TimeBucket {
  type: string;
  name: string;
  bucketStartUtc: string;
  granularity: string;
  sampleCount: number;
  sumNumeric?: number | null;
  avgNumeric?: number | null;
}

@Injectable({ providedIn: 'root' })
export class StatisticsService {
  private readonly http = inject(HttpClient);

  getLatestSnapshots(): Observable<ReadingSnapshot[]> {
    return this.http
      .post<{ data: { latestSnapshots: ReadingSnapshot[] } }>(environment.graphqlUrl, {
        query: `query { latestSnapshots { type name numericValue boolValue payloadJson lastEventId updatedAtUtc } }`
      })
      .pipe(map((response) => response.data.latestSnapshots));
  }

  getAggregationsByType(): Observable<TypeAggregation[]> {
    return this.http
      .post<{ data: { aggregationsByType: TypeAggregation[] } }>(environment.graphqlUrl, {
        query: `query { aggregationsByType { type count sumNumeric avgNumeric } }`
      })
      .pipe(map((response) => response.data.aggregationsByType));
  }

  getAggregationsByLocation(): Observable<LocationAggregation[]> {
    return this.http
      .post<{ data: { aggregationsByLocation: LocationAggregation[] } }>(environment.graphqlUrl, {
        query: `query { aggregationsByLocation { name count sumNumeric avgNumeric } }`
      })
      .pipe(map((response) => response.data.aggregationsByLocation));
  }

  getTimeBuckets(fromUtc: string, toUtc: string): Observable<TimeBucket[]> {
    return this.http
      .post<{ data: { readingTimeBuckets: TimeBucket[] } }>(environment.graphqlUrl, {
        query: `query($from: DateTime!, $to: DateTime!) {
          readingTimeBuckets(fromUtc: $from, toUtc: $to) {
            type name bucketStartUtc sampleCount avgNumeric sumNumeric
          }
        }`,
        variables: { from: fromUtc, to: toUtc }
      })
      .pipe(map((response) => response.data.readingTimeBuckets));
  }
}
