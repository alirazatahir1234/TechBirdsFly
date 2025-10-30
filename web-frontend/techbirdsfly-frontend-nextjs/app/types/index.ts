// Shared types for the dashboard
export interface User {
  id: string;
  email: string;
  fullName: string;
  role: string;
  createdAt: string;
  updatedAt: string;
  subscription?: Subscription;
}

export interface Subscription {
  id: string;
  userId: string;
  plan: 'Free' | 'Starter' | 'Pro' | 'Enterprise';
  startDate: string;
  endDate: string;
  isActive: boolean;
  monthlyLimit: number;
  monthlyUsed: number;
}

export interface Image {
  id: string;
  userId: string;
  prompt: string;
  url: string;
  thumbnailUrl?: string;
  size: string;
  quality: string;
  source: string;
  cost: number;
  generationTimeMs: number;
  createdAt: string;
  isDeleted: boolean;
}

export interface GenerationStatistics {
  totalGenerated: number;
  totalCost: number;
  averageTimeMs: number;
  mostCommonSize: string;
  mostCommonQuality: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

export interface ApiResponse<T> {
  data?: T;
  error?: string;
  message?: string;
  statusCode: number;
}
