import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { TagsService, TagDto } from '../../services/tags.service';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'app-tags-list',
  templateUrl: './tags-list.component.html',
  styleUrls: ['./tags-list.component.scss']
})
export class TagsListComponent implements OnInit, AfterViewInit {
  columns = ['name', 'count', 'share'];
  rows: TagDto[] = [];
  totalItems = 0;
  pageSize = 10;
  pageIndex = 0;
  sortBy = 'name';
  order: 'asc' | 'desc' = 'asc';
  loading = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(private api: TagsService) {}

  ngOnInit(): void {
    this.fetch(1, this.pageSize, this.sortBy, this.order);
  }

  ngAfterViewInit(): void {
    this.paginator.page.pipe(debounceTime(100)).subscribe((event: PageEvent) => {
      const newPage = event.pageIndex + 1;
      const newSize = event.pageSize;

      this.fetch(newPage, newSize, this.sortBy, this.order);
    });

    this.sort.sortChange.subscribe(() => {
      this.sortBy = this.sort.active || 'name';
      this.order = (this.sort.direction as 'asc' | 'desc') || 'asc';
      this.pageIndex = 0;
      this.paginator.pageIndex = 0;
      this.fetch(1, this.pageSize, this.sortBy, this.order);
    });
  }

  fetch(page: number, pageSize: number, sortBy: string, order: string) {
  this.loading = true;
  this.api.getTags(page, pageSize, sortBy, order).subscribe({
    next: res => {
      this.rows = res.items;
      this.totalItems = res.totalItems ?? 0;
      this.pageIndex = res.page - 1;
      this.pageSize = res.pageSize;
      this.loading = false;
    },
    error: err => {
      console.error('API error', err);
      this.loading = false;
    }
  });
}

  refreshFromSO() {
    this.loading = true;
    this.api.refresh().subscribe({
      next: () => this.fetch(1, this.pageSize, this.sortBy, this.order),
      error: err => {
        console.error('Refresh error', err);
        this.loading = false;
      }
    });
  }
}
