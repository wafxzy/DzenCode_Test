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
  public pageSize: number = 25;
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

  public goToPage(page: number): void {
    this.currentPage = page;
    this.loadComments();
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
    return `${environment.baseUrl}/${imagePath}`;
  }

  public getFileUrl(filePath: string): string {
    return `${environment.baseUrl}/${filePath}`;
  }

  public openLightbox(imagePath: string): void {
    this.lightboxImage = this.getImageUrl(imagePath);
  }

  public closeLightbox(): void {
    this.lightboxImage = null;
  }
}