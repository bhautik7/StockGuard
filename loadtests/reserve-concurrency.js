import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = 'http://localhost:5270'; // change to match your actual port
const TOKEN = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJiMzUyNWViYS02NGVmLTQ5ZWYtYjM5Ny05ZGUwMGQ4YjQ4Y2UiLCJlbWFpbCI6ImFkbWluQHN0b2NrZ3VhcmQuY29tIiwiZnVsbE5hbWUiOiJUZXN0IEFkbWluIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW5pc3RyYXRvciIsImV4cCI6MTc4NjQyMjU0NCwiaXNzIjoiU3RvY2tHdWFyZC5BcGkiLCJhdWQiOiJTdG9ja0d1YXJkLkNsaWVudCJ9.xjkTBs4cvMbaebWReehUi8Mfb00vCmszwtERNMvHAk0'
const PRODUCT_ID = '7876e8c0-d2c0-4d14-964e-3c15c366d6db';//PASTE_YOUR_PRODUCT_ID_HERE

export const options = {
  vus: 100,        // 100 virtual users
  iterations: 100,  // each one runs exactly once — 100 total requests
};

export default function () {
  const payload = JSON.stringify({
    productId: PRODUCT_ID,
    quantity: 1,
    idempotencyKey: `loadtest-${__VU}-${__ITER}` // unique per virtual user, so idempotency doesn't interfere
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${TOKEN}`,
    },
  };

  const res = http.post(`${BASE_URL}/api/reservations`, payload, params);

  check(res, {
    'status is 200 or 409': (r) => r.status === 200 || r.status === 409,
  });
}