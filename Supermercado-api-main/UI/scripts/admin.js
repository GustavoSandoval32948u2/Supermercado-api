async function cargar() {

    const res = await fetch(`${API}/productos`);
    const result = await res.json();

    const productos = result.data;

    const tabla = document.getElementById("tabla");

    tabla.innerHTML = "";

    productos.forEach(p => {

        tabla.innerHTML += `
            <tr>
                <td>${p.id}</td>
                <td>${p.nombre}</td>
                <td>${p.precio}</td>
                <td>${p.stock}</td>
                <td>${p.categoria ?? ""}</td>
                <td>
                    <button onclick="eliminar(${p.id})">X</button>
                </td>
            </tr>
        `;
    });
}

async function crearProducto() {

    await fetch(`${API}/productos`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            nombre: nombre.value,
            descripcion: descripcion.value,
            precio: parseFloat(precio.value),
            stock: parseInt(stock.value),
            categoria: categoria.value
        })
    });

    cargar();
}

async function eliminar(id) {

    await fetch(`${API}/productos/${id}`, {
        method: "DELETE"
    });

    cargar();
}

cargar();