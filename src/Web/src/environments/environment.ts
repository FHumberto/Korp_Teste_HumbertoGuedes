export const environment = {
  production: false,
  apiEndpoints: {
    inventory: 'http://localhost:5236/api/v1',
    billing: 'http://localhost:5092/api/v1',
  },
} as const;
