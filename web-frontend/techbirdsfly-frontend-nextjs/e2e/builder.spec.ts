import { test, expect } from '@playwright/test';

test.describe('Builder Canvas (when implemented)', () => {
  test.skip('should load builder page', async ({ page }) => {
    // Skip until /builder route is created
    await page.goto('/builder');

    await expect(page).toHaveTitle(/Builder|TechBirdsFly/);
  });

  test.skip('should add component to canvas', async ({ page }) => {
    await page.goto('/builder');

    // Click on "Add Hero" button (example)
    await page.getByRole('button', { name: /add hero/i }).click();

    // Should see component in canvas
    await expect(page.getByText(/hero section/i)).toBeVisible();
  });

  test.skip('should select component on click', async ({ page }) => {
    await page.goto('/builder');

    // Add a component
    await page.getByRole('button', { name: /add hero/i }).click();

    // Click the component
    await page.getByText(/hero section/i).click();

    // Should see selection handles
    await expect(page.locator('.selection-box-handle')).toBeVisible();
  });

  test.skip('should zoom in and out', async ({ page }) => {
    await page.goto('/builder');

    // Find zoom display
    const zoomDisplay = page.getByText(/100%/);
    await expect(zoomDisplay).toBeVisible();

    // Click zoom in
    await page.getByTitle(/zoom in/i).click();

    // Zoom should increase
    await expect(page.getByText(/110%|120%/)).toBeVisible();

    // Click zoom out
    await page.getByTitle(/zoom out/i).click();
    await page.getByTitle(/zoom out/i).click();

    // Should be back to lower zoom
    await expect(page.getByText(/100%|90%/)).toBeVisible();
  });

  test.skip('should toggle preview mode', async ({ page }) => {
    await page.goto('/builder');

    // Add a component
    await page.getByRole('button', { name: /add hero/i }).click();

    // Toggle preview
    await page.getByTitle(/toggle preview/i).click();

    // Should be in preview mode (no handles)
    await expect(page.locator('.selection-box-handle')).not.toBeVisible();

    // Toggle back to edit
    await page.getByTitle(/toggle preview/i).click();

    // Should be able to select again
    await page.getByText(/hero section/i).click();
    await expect(page.locator('.selection-box-handle')).toBeVisible();
  });

  test.skip('should update component properties', async ({ page }) => {
    await page.goto('/builder');

    // Add hero
    await page.getByRole('button', { name: /add hero/i }).click();

    // Select hero
    await page.getByText(/hero section/i).click();

    // Find properties panel and update title
    const titleInput = page.getByLabel(/title/i);
    await titleInput.fill('My Custom Title');

    // Title should update in preview
    await page.getByTitle(/toggle preview/i).click();
    await expect(page.getByText(/my custom title/i)).toBeVisible();
  });
});

test.describe('Builder State Persistence', () => {
  test.skip('should persist components across page reload', async ({ page }) => {
    await page.goto('/builder');

    // Add components
    await page.getByRole('button', { name: /add hero/i }).click();
    await page.getByRole('button', { name: /add section/i }).click();

    // Reload page
    await page.reload();

    // Components should still be there
    await expect(page.getByText(/hero section/i)).toBeVisible();
    await expect(page.getByText(/content section/i)).toBeVisible();
  });
});
