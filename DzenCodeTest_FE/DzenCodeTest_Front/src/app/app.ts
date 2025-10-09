import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { Component, signal, ViewChild, AfterViewInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommentList } from './components/comment-list/comment-list';
import { CommentForm } from './components/comment-form/comment-form';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, HttpClientModule, CommentList, CommentForm],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App implements AfterViewInit {
  @ViewChild('commentsList') private commentsList!: CommentList;
  public replyToCommentId?: number;
  public replyToUserName?: string;

  public ngAfterViewInit(): void {
    console.log('App: ngAfterViewInit called, commentsList:', this.commentsList);
  }

  public onCommentAdded(): void {
    console.log('App: onCommentAdded called');
    console.log('App: commentsList reference:', this.commentsList);
    
    setTimeout((): void => {
      if (this.commentsList) {
        console.log('App: calling commentsList.loadCommentsFromStart()');
        this.commentsList.loadCommentsFromStart();
      } else {
        console.error('App: commentsList reference is still null after timeout');
      }
    }, 100);
    
    this.replyToCommentId = undefined;
    this.replyToUserName = undefined;
  }

  public onCancelReply(): void {
    this.replyToCommentId = undefined;
    this.replyToUserName = undefined;
  }

  public onReplyToComment(event: {commentId: number, userName: string}): void {
    this.replyToCommentId = event.commentId;
    this.replyToUserName = event.userName;
  }
}