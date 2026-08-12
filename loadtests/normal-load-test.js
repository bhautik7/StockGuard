import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = 'http://localhost:8080';

export const options = {
  stages: [
    { duration: '10s', target: 10 },  // ramp up to 10 virtual users
    { duration: '30s', target: 10 },  // stay at 10 users for 30s
    { duration: '10s', target: 0 },   // ramp back down
  ],
  thresholds: {
    http_req_duration: ['p(95)<300'], // fail the test if p95 exceeds 300ms
  },
};

export default function () {
  const res = http.get(`${BASE_URL}/api/products`);

  check(res, {
    'status is 200': (r) => r.status === 200,
  });

  sleep(Math.random() * 2); // simulate realistic, varied browsing pace
}