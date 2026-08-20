export const environment = {
  production: false,
  apiEndpoints: {
    inventory: 'https://localhost:7090/api/v1',
    billing: 'https://localhost:7187/api/v1',
  },
} as const;
