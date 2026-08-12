import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = 'http://localhost:8080'; // your Docker Api port

export const options = {
  vus: 1,
  duration: '10s',
};

export default function () {
  const res = http.get(`${BASE_URL}/api/categories`);

  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}