export interface NotificationItem {
  notificationId: number;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationPage {
  items: NotificationItem[];
  page: number;
  hasMore: boolean;
  unreadCount: number;
}
