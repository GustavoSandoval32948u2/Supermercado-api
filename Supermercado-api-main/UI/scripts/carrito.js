window.addEventListener("DOMContentLoaded", () => {
    renderCarrito();
});

function renderCarrito() {

    let carrito = JSON.parse(localStorage.getItem("carrito")) || [];

    const contenedor = document.getElementById("carrito");

    contenedor.innerHTML = "";

    carrito.forEach(p => {
        contenedor.innerHTML += `
            <div class="card">
                <h3>${p.nombre}</h3>
                <p>${p.descripcion}</p>
                <p>Q${p.precio}</p>
            </div>
        `;
    });
}

function getCarritoItems() {

    let carrito = JSON.parse(localStorage.getItem("carrito")) || [];

    return carrito.map(p => ({
        productoId: p.id,
        cantidad: 1
    }));
}

async function comprarSync() {

    try {

        let items = getCarritoItems();

        const res = await fetch(`${API}/Ventas`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                clienteId: 1,
                productos: items
            })
        });

        let data = null;

        try {
            data = await res.json();
        } catch (e) {
            console.log("Respuesta no JSON");
        }

        if (!res.ok) {
            console.error("ERROR SYNC:", data);
            alert("Error en compra sync");
            return;
        }

        alert(`Compra SYNC OK - Venta #${data.ventaId}`);

        localStorage.removeItem("carrito");
        location.reload();

    } catch (err) {
        console.error(err);
        alert("Error en compra sync");
    }
}

async function marcarComoDespachado(ventaId) {

    // ASYNC REAL: dispara cola del backend
    const res = await fetch(`${API}/Ventas/${ventaId}/estado`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify("Pedido despachado")
    });

    if (!res.ok) {
        alert("Error en estado async");
        return;
    }

    alert("Estado cambiado (async + cola ejecutada)");
}

async function comprarAsync() {

    try {

        let items = getCarritoItems();

        // 1. Crear venta primero (SYNC base)
        const res = await fetch(`${API}/Ventas`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                clienteId: 1,
                productos: items
            })
        });

        const data = await res.json();

        if (!res.ok) {
            alert(data?.mensaje || "Error en compra async");
            return;
        }

        // 2. Disparar proceso async real (cola)
        await marcarComoDespachado(data.ventaId);

        alert(`Compra ASYNC iniciada - Venta #${data.ventaId}`);

        localStorage.removeItem("carrito");
        location.reload();

    } catch (err) {
        console.error(err);
        alert("Error en compra async");
    }
}