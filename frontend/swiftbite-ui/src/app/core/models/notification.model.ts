export interface Notification {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  referenceId?: string;
  imageUrl?: string;
  createdAt: Date;
}

export interface NotificationList {
  notifications: Notification[];
  unreadCount: number;
  totalCount: number;
}