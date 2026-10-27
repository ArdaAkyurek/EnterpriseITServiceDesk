import { Asset } from './asset';

export type TicketListView = 'all' | 'assigned' | 'unassigned' | 'created';
export type TicketTargetStatus = 'In Progress' | 'Closed';

export interface Ticket {
  ticketId: number;
  title: string;
  description: string;
  createdBy: string;
  assignedTo: string | null;
  assignedToUserId: number | null;
  updatedAt: string | null;
  category: string;
  priority: string;
  status: string;
  asset: string | null;
  createdAt: string;
  closedAt: string | null;
}

export interface CreateTicket {
  title: string;
  description: string;
  categoryId: number;
  priorityId: number;
  assetId: number | null;
}

export interface TicketFormOptions {
  categories: { ticketCategoryId: number; ticketCategoryName: string }[];
  priorities: { ticketPriorityId: number; priorityName: string }[];
  assets: Asset[];
}

export interface TicketAssignee { userId: number; fullName: string; }

export interface TicketComment {
  commentId: number;
  ticketId: number;
  comment: string;
  user: string;
  ticketTitle: string;
  createdAt: string;
}
