import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TagDto { name: string; count: number; share: number; }

export interface PagedResult<T> {
  items: T[];
  totalItems: number;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class TagsService {
  private base = '/api/tags';

  constructor(private http: HttpClient) {}

  getTags(page=1, pageSize=10, sortBy='name', order='asc'): Observable<PagedResult<TagDto>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('sortBy', sortBy)
      .set('order', order);
    return this.http.get<PagedResult<TagDto>>(this.base, { params });
  }

  refresh() {
    return this.http.post<{ imported: number }>('/api/admin/refresh', {});
  }
}
