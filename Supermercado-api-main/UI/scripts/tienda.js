window.addEventListener("DOMContentLoaded", () => {
    cargarProductos();
});

async function cargarProductos() {

    try {
        const res = await fetch(`${API}/productos`);
        const result = await res.json();

        console.log("API RESULT:", result);

        const productos = result.data;

        const contenedor = document.getElementById("productos");

        if (!contenedor) {
            console.error("NO EXISTE #productos");
            return;
        }

        contenedor.innerHTML = "";

        productos.forEach(p => {
            contenedor.innerHTML += `
                <div class="card">
                    <h3>${p.nombre}</h3>
                    <p>${p.descripcion}</p>
                    <p>Q${p.precio}</p>
                    <p>Stock: ${p.stock}</p>

                    <button onclick="agregarCarrito(${p.id})">
                        Agregar
                    </button>
                </div>
            `;
        });

    } catch (error) {
        console.error("ERROR CARGANDO PRODUCTOS:", error);
    }
}

function agregarCarrito(id) {

    fetch(`${API}/productos/${id}`)
        .then(r => r.json())
        .then(producto => {

            let carrito = JSON.parse(localStorage.getItem("carrito")) || [];

            carrito.push(producto);

            localStorage.setItem("carrito", JSON.stringify(carrito));

            alert("Agregado");
        });
}
