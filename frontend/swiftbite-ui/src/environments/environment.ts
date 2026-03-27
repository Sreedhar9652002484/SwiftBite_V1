export const environment = {
  production: false,
  authServerUrl:  'http://localhost:5149',
  apiGatewayUrl: 'http://localhost:5000',  // 🆕 All API calls go through Gateway
  angularBaseUrl: 'http://localhost:4200',
   // SignalR (through gateway)
  signalRHub: 'http://localhost:5000/hubs/notifications',
  razorpayKeyId: 'rzp_test_dummy_key_id',
};


