export interface User {
    id?: number;
    name: string;
    email: string;
    password: string;
    role?: string;
}

export interface Task {
    id?: number;
    title: string;
    description: string;
    dueDate?: string;
    priority: string;
    status: 'Pending' | 'In Progress' | 'Completed';
    assignedToUserId: number;
    assignedToUser?: User;
    assignedToUserName?: string;
  }