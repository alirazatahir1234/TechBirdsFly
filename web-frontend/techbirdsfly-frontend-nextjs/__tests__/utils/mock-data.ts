import { WebsiteComponent } from '@/lib/store/builderStore';

export const mockUser = {
  id: 'user-123',
  email: 'test@example.com',
  fullName: 'Test User',
  accessToken: 'mock-access-token',
  refreshToken: 'mock-refresh-token',
};

export const mockSession = {
  user: mockUser,
  expires: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
};

export const mockHeroComponent: WebsiteComponent = {
  id: 'hero-1',
  type: 'hero',
  name: 'Hero Section',
  properties: {
    title: 'Welcome to TechBirdsFly',
    subtitle: 'Build amazing websites',
  },
};

export const mockSectionComponent: WebsiteComponent = {
  id: 'section-1',
  type: 'section',
  name: 'Content Section',
  properties: {
    title: 'Features',
    content: 'Amazing features here',
  },
};

export const mockComponents: WebsiteComponent[] = [
  mockHeroComponent,
  mockSectionComponent,
  {
    id: 'cta-1',
    type: 'cta',
    name: 'Call to Action',
    properties: {
      headline: 'Ready to get started?',
      buttonText: 'Sign Up Now',
    },
  },
];

export const mockProject = {
  id: 'project-123',
  name: 'Test Website',
  description: 'A test website project',
  userId: 'user-123',
  components: mockComponents,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
};

export const mockProjects = [
  mockProject,
  {
    id: 'project-456',
    name: 'Another Project',
    description: 'Another test project',
    userId: 'user-123',
    components: [],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];
