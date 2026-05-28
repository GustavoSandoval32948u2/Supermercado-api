import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    stages: [
        { duration: '10s', target: 5 },
        { duration: '20s', target: 10 },
        { duration: '10s', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<2000'],
        http_req_failed: ['rate<0.1'],
    },
};

const BASE_URL = 'https://localhost:7239';

// 🔥 MUCHOS CLIENTES DISPONIBLES
const clientes = [
    1,
    2,
    1002,
    1003,
    1004,
    1005,
    1006,
    1007,
    1008,
    1009,
    1010,
    1011,
    1012
];

export default function () {

    // 🔥 CLIENTE ÚNICO POR VU
    let clienteId = clientes[(__VU - 1) % clientes.length];

    // 🔥 PRODUCTO ALEATORIO
    let productoId = Math.floor(Math.random() * 3) + 1;

    // =========================
    // AGREGAR PRODUCTO
    // =========================
    let add1 = http.post(
        `${BASE_URL}/api/Carrito/agregar/${productoId}?clienteId=${clienteId}&cantidad=1`,
        null,
        {
            insecureSkipTLSVerify: true
        }
    );

    check(add1, {
        'add carrito OK': (r) => r.status === 200,
    });

    if (add1.status !== 200) {
        console.log(`ADD ERROR: ${add1.status} - ${add1.body}`);
        return;
    }

    sleep(1);

    // =========================
    // COMPRA SYNC
    // =========================
    let sync = http.post(
        `${BASE_URL}/api/Carrito/comprar?clienteId=${clienteId}`,
        null,
        {
            insecureSkipTLSVerify: true
        }
    );

    check(sync, {
        'sync OK': (r) => r.status === 200,
    });

    if (sync.status !== 200) {
        console.log(`SYNC ERROR: ${sync.status} - ${sync.body}`);
    }

    sleep(1);

    // =========================
    // AGREGAR OTRA VEZ
    // =========================
    let add2 = http.post(
        `${BASE_URL}/api/Carrito/agregar/${productoId}?clienteId=${clienteId}&cantidad=1`,
        null,
        {
            insecureSkipTLSVerify: true
        }
    );

    check(add2, {
        'add2 carrito OK': (r) => r.status === 200,
    });

    if (add2.status !== 200) {
        console.log(`ADD2 ERROR: ${add2.status} - ${add2.body}`);
        return;
    }

    sleep(1);

    // =========================
    // COMPRA ASYNC
    // =========================
    let asyncRes = http.post(
        `${BASE_URL}/api/Carrito/comprar-async?clienteId=${clienteId}`,
        null,
        {
            insecureSkipTLSVerify: true
        }
    );

    check(asyncRes, {
        'async OK': (r) => r.status === 202,
    });

    if (asyncRes.status !== 202) {
        console.log(`ASYNC ERROR: ${asyncRes.status} - ${asyncRes.body}`);
    }

    sleep(1);
}