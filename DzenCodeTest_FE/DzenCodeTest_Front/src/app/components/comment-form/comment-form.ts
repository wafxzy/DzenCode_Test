import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Captcha, CommentDto } from '../../models/comment.model';
import { SharedService } from '../../services/shared';

@Component({
  selector: 'app-comment-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './comment-form.html',
  styleUrls: ['./comment-form.css']
})

export class CommentForm implements OnInit {
  @Input() public parentId?: number;
  @Input() public replyToUser?: string;
  @Output() public commentAdded: EventEmitter<void> = new EventEmitter<void>();
  @Output() public cancelReplyEvent: EventEmitter<void> = new EventEmitter<void>();

  public commentForm: FormGroup;
  public captcha: Captcha | null = null;
  public selectedImage: File | null = null;
  public selectedTextFile: File | null = null;
  public isSubmitting: boolean = false;
  public errorMessage: string = '';
  public previewHtml: string = '';

  constructor(
    private readonly fb: FormBuilder,
    private readonly commentService: SharedService
  ) {
    this.commentForm = this.fb.group({
      userName: ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9]+$/)]],
      email: ['', [Validators.required, Validators.email]],
      homePage: ['', [this.urlValidator]],
      text: ['', Validators.required],
      captchaCode: ['', Validators.required]
    });
  }

  public ngOnInit(): void {
    console.log('CommentFormComponent initialized');
    this.loadCaptcha();
  }

  private urlValidator(control: any): {[key: string]: any} | null {
    if (!control.value) {
      return null;
    }
    const urlPattern: RegExp = /^https?:\/\/.+/;
    return urlPattern.test(control.value) ? null : { url: true };
  }

  private loadCaptcha(): void {
    console.log('Loading CAPTCHA...');
    this.commentService.generateCaptcha().subscribe({
      next: (captcha: Captcha): void => {
        this.captcha = captcha;
        console.log('CAPTCHA loaded successfully:', captcha);
      },
      error: (error: any): void => {
        console.error('Error loading CAPTCHA:', error);
        this.errorMessage = 'Failed to load CAPTCHA. Please refresh the page.';
        setTimeout((): void => {
          console.log('Retrying CAPTCHA load...');
          this.loadCaptcha();
        }, 2000);
      }
    });
  }

  public refreshCaptcha(): void {
    this.loadCaptcha();
    this.commentForm.patchValue({ captchaCode: '' });
  }

  public onImageSelect(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      const allowedTypes: string[] = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
      if (allowedTypes.includes(file.type)) {
        this.selectedImage = file;
      } else {
        alert('Please select a valid image file (JPG, PNG, GIF)');
        event.target.value = '';
      }
    }
  }

  public onTextFileSelect(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      if (file.type === 'text/plain' && file.size <= 100 * 1024) {
        this.selectedTextFile = file;
      } else {
        alert('Please select a valid text file (TXT, max 100KB)');
        event.target.value = '';
      }
    }
  }

  public insertTag(tag: string): void {
    const textArea: HTMLTextAreaElement = document.getElementById('text') as HTMLTextAreaElement;
    const start: number = textArea.selectionStart;
    const end: number = textArea.selectionEnd;
    const text: string = textArea.value;
    const selectedText: string = text.substring(start, end);

    let newText: string = '';
    if (tag === 'a') {
      newText = `<a href="" title="">${selectedText}</a>`;
    } else {
      newText = `<${tag}>${selectedText}</${tag}>`;
    }

    const newValue: string = text.substring(0, start) + newText + text.substring(end);
    this.commentForm.patchValue({ text: newValue });
    
    setTimeout((): void => {
      textArea.focus();
      if (tag === 'a') {
        textArea.setSelectionRange(start + 9, start + 9);
      } else {
        textArea.setSelectionRange(start + newText.length, start + newText.length);
      }
    });
  }

  public previewText(): void {
    const text: string | null = this.commentForm.get('text')?.value;
    if (text) {
      this.commentService.previewComment(text).subscribe({
        next: (html: string): void => {
          this.previewHtml = html;
        },
        error: (error: any): void => {
          console.error('Error previewing text:', error);
        }
      });
    }
  }

  public onSubmit(): void {
    if (this.commentForm.valid && this.captcha) {
      this.isSubmitting = true;
      this.errorMessage = '';

      const commentDto: CommentDto = {
        userName: this.commentForm.get('userName')?.value,
        email: this.commentForm.get('email')?.value,
        homePage: this.commentForm.get('homePage')?.value || undefined,
        text: this.commentForm.get('text')?.value,
        captchaCode: this.commentForm.get('captchaCode')?.value,
        parentId: this.parentId,
        image: this.selectedImage || undefined,
        textFile: this.selectedTextFile || undefined
      };

      this.commentService.createComment(commentDto, this.captcha.id).subscribe({
        next: (): void => {
          console.log('Comment created successfully');
          this.resetForm();
          this.commentAdded.emit();
          console.log('Comment added event emitted');
        },
        error: (error: any): void => {
          this.isSubmitting = false;
          this.errorMessage = error.error?.message || 'An error occurred while submitting the comment';
          this.refreshCaptcha();
        }
      });
    }
  }

  public resetForm(): void {
    this.commentForm.reset();
    this.selectedImage = null;
    this.selectedTextFile = null;
    this.isSubmitting = false;
    this.errorMessage = '';
    this.previewHtml = '';
    this.loadCaptcha();
    
    // Reset file inputs
    const imageInput: HTMLInputElement = document.getElementById('image') as HTMLInputElement;
    const textFileInput: HTMLInputElement = document.getElementById('textFile') as HTMLInputElement;
    if (imageInput) imageInput.value = '';
    if (textFileInput) textFileInput.value = '';
  }

  public cancelReply(): void {
    this.cancelReplyEvent.emit();
  }

}
