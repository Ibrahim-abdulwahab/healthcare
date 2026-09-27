import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e', fullyParallel: false, workers: 1,
  timeout: 60000, expect: { timeout: 10000 },
  reporter: [['list'], ['html', { open: 'never', outputFolder: '../test-results/playwright-report' }]],
  outputDir: '../test-results/playwright',
  use: { ...devices['Desktop Chrome'], viewport: { width: 1536, height: 1024 }, timezoneId: 'America/Los_Angeles', baseURL: process.env.UI_BASE_URL || 'http://127.0.0.1:5173', channel: 'msedge', headless: true, trace: 'retain-on-failure', screenshot: 'only-on-failure' },
});
