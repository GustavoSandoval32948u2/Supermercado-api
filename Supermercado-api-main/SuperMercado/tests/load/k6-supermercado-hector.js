import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '20s', target: 10 },
    { duration: '40s', target: 25 },
    { duration: '20s', target: 0 }
  ],
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1200']
  }
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5143';

export default function () {
  const health = http.get(`${BASE_URL}/health`);
  check(health, {
    'health 200': (r) => r.status === 200
  });

  const agregar = http.post(`${BASE_URL}/api/Carrito/agregar/1?cantidad=1`);
  check(agregar, {
    'producto agregado': (r) => r.status === 200
  });

  const carrito = http.get(`${BASE_URL}/api/Carrito`);
  check(carrito, {
    'carrito consultado': (r) => r.status === 200
  });

  const compra = http.post(`${BASE_URL}/api/Carrito/comprar`);
  check(compra, {
    'compra confirmada': (r) => r.status === 200
  });

  const payload = JSON.stringify({
    ventaId: Math.floor(Math.random() * 9000) + 1000,
    clienteNombre: 'Cliente K6',
    emailDestino: 'cliente.k6@gmail.com',
    total: 89.50
  });

  const pedido = http.post(`${BASE_URL}/api/PedidosQueue/simular-pedido-completo`, payload, {
    headers: { 'Content-Type': 'application/json' }
  });
  check(pedido, {
    'pedido simulado': (r) => r.status === 200
  });

  sleep(1);
}
