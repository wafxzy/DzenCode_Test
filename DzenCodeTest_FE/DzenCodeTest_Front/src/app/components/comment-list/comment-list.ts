import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment.development';
import { CommentsResponse, Comment } from '../../models/comment.model';
import { SharedService } from '../../services/shared';

@Component({
  selector: 'app-comment-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './comment-list.html',
  styleUrls: ['./comment-list.css']
})
export class CommentList implements OnInit {
  @Output() public replyRequested: EventEmitter<{commentId: number, userName: string}> = new EventEmitter<{commentId: number, userName: string}>();
  
  public comments: Comment[] = [];
  public currentPage: number = 1;
  public pageSize: number = 10;
  public totalPages: number = 1;
  public sortBy: string = 'CreatedAt';
  public sortOrder: string = 'desc';
  public lightboxImage: string | null = null;
  public isLoading: boolean = false;

  constructor(private readonly commentService: SharedService) { }

  public ngOnInit(): void {
    this.loadComments();
  }

  public loadComments(): void {
    console.log('Loading comments...', {
      page: this.currentPage,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortOrder: this.sortOrder
    });
    
    this.isLoading = true;
    this.commentService.getComments(this.currentPage, this.pageSize, this.sortBy, this.sortOrder)
      .subscribe({
        next: (response: CommentsResponse): void => {
          console.log('Comments loaded successfully:', response);
          this.comments = response.comments;
          this.currentPage = response.currentPage;
          this.totalPages = response.totalPages;
          this.isLoading = false;
          console.log('Comments array:', this.comments);
        },
        error: (error: any): void => {
          console.error('Error loading comments:', error);
          this.isLoading = false;
        }
      });
  }

  public loadCommentsFromStart(): void {
    this.currentPage = 1;
    this.loadComments();
  }

  public goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadComments();
    }
  }

  public goToFirstPage(): void {
    this.goToPage(1);
  }

  public goToLastPage(): void {
    this.goToPage(this.totalPages);
  }

  public goToPreviousPage(): void {
    this.goToPage(this.currentPage - 1);
  }

  public goToNextPage(): void {
    this.goToPage(this.currentPage + 1);
  }

  public getPageNumbers(): number[] {
    const pages: number[] = [];
    const startPage: number = Math.max(1, this.currentPage - 2);
    const endPage: number = Math.min(this.totalPages, this.currentPage + 2);
    
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  public replyToComment(parentId: number): void {
    const userName: string = this.findUserNameById(parentId, this.comments);
    this.replyRequested.emit({ commentId: parentId, userName });
  }

  private findUserNameById(id: number, comments: Comment[]): string {
    for (const comment of comments) {
      if (comment.id === id) {
        return comment.userName;
      }
      if (comment.replies && comment.replies.length > 0) {
        const found: string = this.findUserNameById(id, comment.replies);
        if (found) return found;
      }
    }
    return '';
  }

  public getImageUrl(imagePath: string): string {
    if (imagePath.startsWith('http://') || imagePath.startsWith('https://')) {
      console.log('getImageUrl: Full URL detected:', imagePath);
      return imagePath;
    }
    
    const cleanPath: string = imagePath.startsWith('/') ? imagePath.substring(1) : imagePath;
    const fullUrl: string = `${environment.baseUrl}/${cleanPath}`;
    console.log('getImageUrl:', { imagePath, cleanPath, fullUrl, baseUrl: environment.baseUrl });
    return fullUrl;
  }

  public getFileUrl(filePath: string): string {
    if (filePath.startsWith('http://') || filePath.startsWith('https://')) {
      console.log('getFileUrl: Full URL detected:', filePath);
      return filePath;
    }
    
    const cleanPath: string = filePath.startsWith('/') ? filePath.substring(1) : filePath;
    const fullUrl: string = `${environment.baseUrl}/${cleanPath}`;
    console.log('getFileUrl:', { filePath, cleanPath, fullUrl, baseUrl: environment.baseUrl });
    return fullUrl;
  }

  public openLightbox(imagePath: string): void {
    this.lightboxImage = this.getImageUrl(imagePath);
  }

  public closeLightbox(): void {
    this.lightboxImage = null;
  }

  public onImageError(event: any, imagePath: string): void {
    console.error('Image failed to load:', { imagePath, event });
    console.error('Attempted URL:', this.getImageUrl(imagePath));
  }

  public onImageLoad(event: any, imagePath: string): void {
    console.log('Image loaded successfully:', { imagePath, event });
    console.log('Loaded URL:', this.getImageUrl(imagePath));
  }
}