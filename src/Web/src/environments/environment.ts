export const environment = {
  production: false,
  apiEndpoints: {
    inventory: 'https://localhost:7090/api/v1',
    billing: 'https://localhost:5092/api/v1',
  },
} as const;
