import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { throwError, Observable, catchError, tap } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { CommentsResponse, CommentDto, Captcha } from '../models/comment.model';

@Injectable({
  providedIn: 'root'
})

export class SharedService {
  private readonly apiUrl: string = environment.apiUrl;

  constructor(private readonly http: HttpClient) { }

  private handleError(error: HttpErrorResponse): Observable<never> {
    console.error('API Error:', error);
    if (error.status === 0) {
      console.error('Network error - check if backend is running');
    }
    return throwError(() => error);
  }

  public getComments(page: number = 1, pageSize: number = 25, sortBy: string = 'CreatedAt', sortOrder: string = 'desc'): Observable<CommentsResponse> {
    console.log('Service: getComments called with params:', { page, pageSize, sortBy, sortOrder });
    
    const params: HttpParams = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('sortBy', sortBy)
      .set('sortOrder', sortOrder);

    const url: string = `${this.apiUrl}/comments`;
    console.log('Service: making request to:', url, 'with params:', params.toString());

    return this.http.get<CommentsResponse>(url, { params })
      .pipe(
        catchError(this.handleError),
        tap((response: CommentsResponse): void => console.log('Service: getComments response:', response))
      );
  }

  public getComment(id: number): Observable<Comment> {
    return this.http.get<Comment>(`${this.apiUrl}/comments/${id}`)
      .pipe(catchError(this.handleError));
  }

  public createComment(commentDto: CommentDto, captchaId: string): Observable<Comment> {
    console.log('Service: createComment called with:', commentDto, 'captchaId:', captchaId);
    
    const formData: FormData = new FormData();
    formData.append('userName', commentDto.userName);
    formData.append('email', commentDto.email);
    formData.append('text', commentDto.text);
    formData.append('captchaCode', commentDto.captchaCode);
    formData.append('captchaId', captchaId);
    
    if (commentDto.homePage) {
      formData.append('homePage', commentDto.homePage);
    }
    
    if (commentDto.parentId) {
      formData.append('parentId', commentDto.parentId.toString());
    }
    
    if (commentDto.image) {
      formData.append('image', commentDto.image);
    }
    
    if (commentDto.textFile) {
      formData.append('textFile', commentDto.textFile);
    }

    return this.http.post<Comment>(`${this.apiUrl}/comments`, formData)
      .pipe(
        catchError(this.handleError),
        tap((response: Comment) => console.log('Service: createComment response:', response))
      );
  }

  public previewComment(text: string): Observable<string> {
    return this.http.post(`${this.apiUrl}/comments/preview`, JSON.stringify(text), {
      headers: { 'Content-Type': 'application/json' },
      responseType: 'text'
    }).pipe(catchError(this.handleError));
  }

  public generateCaptcha(): Observable<Captcha> {
    console.log('Requesting CAPTCHA from:', `${this.apiUrl}/captcha`);
    return this.http.get<Captcha>(`${this.apiUrl}/captcha`)
      .pipe(catchError(this.handleError));
  }
}