export interface Comment {
  id: number;
  userName: string;
  email: string;
  homePage?: string;
  text: string;
  createdAt: Date;
  parentId?: number;
  replies: Comment[];
  imagePath?: string;
  textFilePath?: string;
}

export interface CommentDto {
  userName: string;
  email: string;
  homePage?: string;
  text: string;
  parentId?: number;
  captchaCode: string;
  image?: File;
  textFile?: File;
}

export interface Captcha {
  id: string;
  image: string;
}

export interface CommentsResponse {
  comments: Comment[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
}