using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-GT");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-GT");

var baseUrl = args.Length > 0
    ? args[0]
    : Environment.GetEnvironmentVariable("SUPERMERCADO_API_URL") ?? "http://localhost:5143";

using var http = new HttpClient
{
    BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
    Timeout = TimeSpan.FromSeconds(20)
};

var productosDemo = new[]
{
    new ProductoDemo(1, "Arroz Gallo Dorado 5 lb", 10.00m),
    new ProductoDemo(2, "Leche Entera 1 litro", 8.00m),
    new ProductoDemo(3, "Gaseosa Coca-Cola 600 ml", 5.00m),
    new ProductoDemo(4, "Jabon antibacterial", 12.00m),
    new ProductoDemo(5, "Servilletas familiares", 6.00m),
    new ProductoDemo(6, "Cafe molido 400 g", 20.00m),
    new ProductoDemo(7, "Aceite vegetal 900 ml", 18.00m),
    new ProductoDemo(8, "Agua pura 1 litro", 4.00m),
    new ProductoDemo(9, "Suavizante de ropa", 22.00m)
};

await VerificarApiAsync();

while (true)
{
    Console.WriteLine();
    Console.WriteLine("============================================");
    Console.WriteLine(" SUPERMERCADO ONLINE - SIMULADOR DE COMPRAS ");
    Console.WriteLine(" Héctor Jesus - UI, Testing y Entrega       ");
    Console.WriteLine("============================================");
    Console.WriteLine($"API actual: {http.BaseAddress}");
    Console.WriteLine("1. Ver productos de supermercado");
    Console.WriteLine("2. Agregar producto al carrito");
    Console.WriteLine("3. Ver carrito");
    Console.WriteLine("4. Confirmar compra");
    Console.WriteLine("5. Simular pedido completo con cola");
    Console.WriteLine("6. Ver estado de la cola");
    Console.WriteLine("0. Salir");
    Console.Write("Elige una opcion: ");

    var opcion = Console.ReadLine()?.Trim();
    Console.WriteLine();

    try
    {
        switch (opcion)
        {
            case "1":
                MostrarProductos(productosDemo);
                break;
            case "2":
                await AgregarProductoAsync(productosDemo);
                break;
            case "3":
                await VerCarritoAsync();
                break;
            case "4":
                await ConfirmarCompraAsync();
                break;
            case "5":
                await SimularPedidoCompletoAsync();
                break;
            case "6":
                await VerEstadoColaAsync();
                break;
            case "0":
                Console.WriteLine("Simulador finalizado.");
                return;
            default:
                Console.WriteLine("Opcion no valida.");
                break;
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"No se pudo conectar con la API: {ex.Message}");
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("La API tardo demasiado en responder.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error inesperado: {ex.Message}");
    }
}

async Task VerificarApiAsync()
{
    Console.WriteLine("Verificando API...");
    try
    {
        var response = await http.GetAsync("health");
        Console.WriteLine(response.IsSuccessStatusCode
            ? "API disponible."
            : $"La API respondio con estado {(int)response.StatusCode}.");
    }
    catch
    {
        Console.WriteLine("No se pudo verificar /health. Asegurate de ejecutar primero supermercado.API.");
    }
}

static void MostrarProductos(IEnumerable<ProductoDemo> productos)
{
    Console.WriteLine("Productos disponibles para la demo:");
    Console.WriteLine("ID | Producto                         | Precio");
    Console.WriteLine("---+----------------------------------+----------");
    foreach (var p in productos)
    {
        Console.WriteLine($"{p.Id,2} | {p.Nombre,-32} | Q {p.Precio,7:N2}");
    }
}

async Task AgregarProductoAsync(ProductoDemo[] productos)
{
    MostrarProductos(productos);

    Console.Write("Ingresa el ID del producto: ");
    if (!int.TryParse(Console.ReadLine(), out var id) || productos.All(p => p.Id != id))
    {
        Console.WriteLine("Producto no valido.");
        return;
    }

    Console.Write("Cantidad: ");
    if (!int.TryParse(Console.ReadLine(), out var cantidad) || cantidad <= 0)
    {
        Console.WriteLine("Cantidad no valida.");
        return;
    }

    var response = await http.PostAsync($"api/Carrito/agregar/{id}?cantidad={cantidad}", null);
    var contenido = await response.Content.ReadAsStringAsync();

    Console.WriteLine(response.IsSuccessStatusCode
        ? "Producto agregado correctamente al carrito."
        : $"No se pudo agregar: {contenido}");
}

async Task VerCarritoAsync()
{
    var response = await http.GetAsync("api/Carrito");
    var contenido = await response.Content.ReadAsStringAsync();

    Console.WriteLine("Carrito actual:");
    ImprimirJsonBonito(contenido);
}

async Task ConfirmarCompraAsync()
{
    var response = await http.PostAsync("api/Carrito/comprar", null);
    var contenido = await response.Content.ReadAsStringAsync();

    Console.WriteLine(response.IsSuccessStatusCode
        ? "Compra confirmada. Resultado:"
        : "No se pudo confirmar la compra:");

    ImprimirJsonBonito(contenido);
}

async Task SimularPedidoCompletoAsync()
{
    var request = new
    {
        ventaId = Random.Shared.Next(1000, 9999),
        clienteNombre = "Maria Lopez",
        emailDestino = "cliente.prueba@gmail.com",
        total = 89.50m
    };

    var response = await http.PostAsJsonAsync("api/PedidosQueue/simular-pedido-completo", request);
    var contenido = await response.Content.ReadAsStringAsync();

    Console.WriteLine(response.IsSuccessStatusCode
        ? "Pedido completo simulado y enviado a la cola."
        : "No se pudo simular el pedido:");

    ImprimirJsonBonito(contenido);
}

async Task VerEstadoColaAsync()
{
    var response = await http.GetAsync("api/PedidosQueue/estado-cola");
    var contenido = await response.Content.ReadAsStringAsync();

    Console.WriteLine("Estado de la cola:");
    ImprimirJsonBonito(contenido);
}

static void ImprimirJsonBonito(string contenido)
{
    if (string.IsNullOrWhiteSpace(contenido))
    {
        Console.WriteLine("Sin contenido.");
        return;
    }

    try
    {
        using var doc = JsonDocument.Parse(contenido);
        Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
    }
    catch
    {
        Console.WriteLine(contenido);
    }
}

public record ProductoDemo(int Id, string Nombre, decimal Precio);
