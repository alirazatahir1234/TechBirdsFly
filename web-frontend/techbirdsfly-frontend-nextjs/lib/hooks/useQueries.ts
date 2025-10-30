'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useAuthStore } from '@/lib/store/authStore';

const API_BASE = 'http://localhost:5000/api';

// ============ PROJECTS ============

export interface Project {
  id: string;
  name: string;
  description: string;
  type: string;
  status: 'draft' | 'published' | 'archived';
  createdAt: string;
  updatedAt: string;
  userId: string;
}

export const useProjects = () => {
  const token = useAuthStore((state: any) => state.token);

  return useQuery({
    queryKey: ['projects'],
    queryFn: async () => {
      const response = await fetch(`${API_BASE}/projects`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (!response.ok) throw new Error('Failed to fetch projects');
      return response.json() as Promise<Project[]>;
    },
    enabled: !!token,
  });
};

export const useCreateProject = () => {
  const queryClient = useQueryClient();
  const token = useAuthStore((state: any) => state.token);

  return useMutation({
    mutationFn: async (data: Partial<Project>) => {
      const response = await fetch(`${API_BASE}/projects`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) throw new Error('Failed to create project');
      return response.json() as Promise<Project>;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });
};

export const useUpdateProject = () => {
  const queryClient = useQueryClient();
  const token = useAuthStore((state: any) => state.token);

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: Partial<Project> }) => {
      const response = await fetch(`${API_BASE}/projects/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) throw new Error('Failed to update project');
      return response.json() as Promise<Project>;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });
};

export const useDeleteProject = () => {
  const queryClient = useQueryClient();
  const token = useAuthStore((state: any) => state.token);

  return useMutation({
    mutationFn: async (id: string) => {
      const response = await fetch(`${API_BASE}/projects/${id}`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (!response.ok) throw new Error('Failed to delete project');
      return response.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });
};

// ============ USER PROFILE ============

export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  avatar?: string;
  subscription: {
    plan: 'free' | 'pro' | 'enterprise';
    status: 'active' | 'cancelled';
    renewalDate: string;
  };
  stats: {
    projectCount: number;
    totalStorage: number;
    generationCount: number;
  };
}

export const useUserProfile = () => {
  const token = useAuthStore((state: any) => state.token);

  return useQuery({
    queryKey: ['user-profile'],
    queryFn: async () => {
      const response = await fetch(`${API_BASE}/users/me`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (!response.ok) throw new Error('Failed to fetch profile');
      return response.json() as Promise<UserProfile>;
    },
    enabled: !!token,
  });
};

export const useUpdateProfile = () => {
  const queryClient = useQueryClient();
  const token = useAuthStore((state: any) => state.token);

  return useMutation({
    mutationFn: async (data: Partial<UserProfile>) => {
      const response = await fetch(`${API_BASE}/users/me`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) throw new Error('Failed to update profile');
      return response.json() as Promise<UserProfile>;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user-profile'] });
    },
  });
};

// ============ WEBSITE GENERATION ============

export interface GenerationRequest {
  projectId: string;
  prompt: string;
  template?: string;
  style?: string;
}

export const useGenerateWebsite = () => {
  const queryClient = useQueryClient();
  const token = useAuthStore((state: any) => state.token);

  return useMutation({
    mutationFn: async (data: GenerationRequest) => {
      const response = await fetch(`${API_BASE}/projects/generate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) throw new Error('Failed to generate website');
      return response.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });
};

// ============ TEMPLATES ============

export interface Template {
  id: string;
  name: string;
  description: string;
  category: string;
  thumbnail: string;
  components: string[];
  previewUrl: string;
}

export const useTemplates = (category?: string) => {
  return useQuery({
    queryKey: ['templates', category],
    queryFn: async () => {
      const url = new URL(`${API_BASE}/templates`);
      if (category) url.searchParams.append('category', category);

      const response = await fetch(url);
      if (!response.ok) throw new Error('Failed to fetch templates');
      return response.json() as Promise<Template[]>;
    },
  });
};

// ============ IMAGES ============

export interface ImageUploadResponse {
  id: string;
  url: string;
  size: number;
  format: string;
  createdAt: string;
}

export const useUploadImage = () => {
  const token = useAuthStore((state: any) => state.token);

  return useMutation({
    mutationFn: async (file: File) => {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetch(`${API_BASE}/images/upload`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
        },
        body: formData,
      });

      if (!response.ok) throw new Error('Failed to upload image');
      return response.json() as Promise<ImageUploadResponse>;
    },
  });
};
