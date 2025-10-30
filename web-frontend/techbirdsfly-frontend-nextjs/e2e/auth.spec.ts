import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should navigate to login page', async ({ page }) => {
    await page.goto('/login');
    
    await expect(page).toHaveTitle(/TechBirdsFly/);
    await expect(page.getByRole('heading', { name: /welcome back/i })).toBeVisible();
  });

  test('should show validation errors for empty login form', async ({ page }) => {
    await page.goto('/login');

    // Click submit without filling form
    await page.getByRole('button', { name: /sign in/i }).click();

    // Should see validation errors
    await expect(page.getByText(/email is required/i)).toBeVisible();
    await expect(page.getByText(/password is required/i)).toBeVisible();
  });

  test('should show error for invalid email format', async ({ page }) => {
    await page.goto('/login');

    // Fill with invalid email
    await page.getByLabel(/email/i).fill('invalid-email');
    await page.getByLabel(/password/i).fill('password123');
    await page.getByRole('button', { name: /sign in/i }).click();

    // Should see validation error
    await expect(page.getByText(/invalid email/i)).toBeVisible();
  });

  test('should navigate to register page from login', async ({ page }) => {
    await page.goto('/login');

    // Click on sign up link
    await page.getByRole('link', { name: /sign up/i }).click();

    // Should be on register page
    await expect(page).toHaveURL('/register');
    await expect(page.getByRole('heading', { name: /create account/i })).toBeVisible();
  });

  test('should show validation errors on register page', async ({ page }) => {
    await page.goto('/register');

    // Click submit without filling form
    await page.getByRole('button', { name: /sign up|create account/i }).click();

    // Should see validation errors
    await expect(page.getByText(/name is required/i)).toBeVisible();
    await expect(page.getByText(/email is required/i)).toBeVisible();
  });

  test('should validate password requirements on register', async ({ page }) => {
    await page.goto('/register');

    // Fill with weak password
    await page.getByLabel(/full name/i).fill('Test User');
    await page.getByLabel(/email/i).fill('test@example.com');
    await page.getByLabel(/^password$/i).fill('123');
    await page.getByLabel(/confirm password/i).fill('123');
    
    await page.getByRole('button', { name: /sign up|create account/i }).click();

    // Should see password validation error
    await expect(page.getByText(/password must be at least 8 characters/i)).toBeVisible();
  });
});

test.describe('Homepage', () => {
  test('should load homepage successfully', async ({ page }) => {
    await page.goto('/');

    await expect(page).toHaveTitle(/TechBirdsFly/);
    await expect(page.getByRole('heading', { name: /TechBirdsFly/i })).toBeVisible();
  });

  test('should have navigation links', async ({ page }) => {
    await page.goto('/');

    // Check for navigation items
    await expect(page.getByRole('link', { name: /home/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /login|sign in/i })).toBeVisible();
  });

  test('should be responsive', async ({ page }) => {
    // Test mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');

    await expect(page).toHaveTitle(/TechBirdsFly/);
    
    // Test tablet viewport
    await page.setViewportSize({ width: 768, height: 1024 });
    await expect(page.getByRole('heading', { name: /TechBirdsFly/i })).toBeVisible();
    
    // Test desktop viewport
    await page.setViewportSize({ width: 1920, height: 1080 });
    await expect(page.getByRole('heading', { name: /TechBirdsFly/i })).toBeVisible();
  });
});
