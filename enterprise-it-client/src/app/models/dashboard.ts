export interface DashboardSummary {
  scope: 'all' | 'assigned' | 'created';
  total: number;
  open: number;
  inProgress: number;
  closed: number;
  other: number;
  unassignedActiveCount: number | null;
  activeByPriority: { priority: string; count: number }[];
  recentTickets: { ticketId: number; title: string; status: string; priority: string; assignedTo: string | null; createdAt: string }[];
  generatedAt: string;
}
