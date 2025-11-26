// API Response Types
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  timestamp?: string;
  errors?: Record<string, string[]>;
}

// Website Generation Types
export interface GenerateWebsitePayload {
  projectName: string;
  description: string;
  industry: string;
  features: string[];
  colorScheme: string;
  includeContactForm: boolean;
}

export interface Section {
  id: string;
  type: string;
  html: string;
  css: string;
  js: string;
  order: number;
}

export interface GeneratedWebsiteDto {
  projectId: string;
  projectName: string;
  htmlContent: string;
  cssContent: string;
  jsContent: string;
  generatedAt: string;
  status: string;
  sections?: Section[];
}

// Project Types
export interface Project {
  id: string;
  name: string;
  description: string;
  industry: string;
  createdAt: string;
  updatedAt: string;
  html?: string;
  css?: string;
  js?: string;
}

// Form State Types
export interface CreateFormState {
  projectName: string;
  description: string;
  industry: string;
  features: string[];
  colorScheme: string;
  includeContactForm: boolean;
}

// Generate Response
export interface GenerateResponse {
  htmlContent: string;
  cssContent: string;
  jsContent: string;
  projectId: string;
  projectName: string;
  generatedAt: string;
}
