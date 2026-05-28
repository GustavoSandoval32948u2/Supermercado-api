using System.Net;
using System.Net.Mail;
using supermercado.API.Models;

namespace supermercado.API.Services
{
    // se encarga de mandar el correo html al cliente cuando cambia el estado del pedido
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task EnviarNotificacionPedidoAsync(MensajePedido mensaje)
        {
            var remitente = _config["Email:Remitente"];
            var password = _config["Email:Password"];
            var smtpHost = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");

            // si no hay credenciales configuradas simplemente saltamos el envio
            if (string.IsNullOrEmpty(remitente) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Credenciales de email no configuradas, saltando envio");
                return;
            }

            var asunto = ObtenerAsunto(mensaje.Estado, mensaje.VentaId);
            var cuerpo = ConstruirCuerpoEmail(mensaje);

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(remitente, password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(remitente, "Supermercado Online"),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true
            };

            mail.To.Add(mensaje.ClienteEmail);

            try
            {
                await smtpClient.SendMailAsync(mail);
                _logger.LogInformation(
                    "Correo enviado a {Email} - VentaId: {VentaId}, Estado: {Estado}",
                    mensaje.ClienteEmail,
                    mensaje.VentaId,
                    mensaje.Estado
                );
            }
            catch (Exception ex)
            {
                // si falla el correo no queremos que truene el worker entero
                _logger.LogError(ex, "Error al enviar correo para VentaId: {VentaId}", mensaje.VentaId);
            }
        }

        private string ObtenerAsunto(string estado, int ventaId)
        {
            return estado switch
            {
                "Recibido" => $"Pedido #{ventaId} recibido - Gracias por tu compra",
                "Despachado" => $"Pedido #{ventaId} en camino",
                "Entregado" => $"Pedido #{ventaId} entregado",
                _ => $"Actualizacion de tu pedido #{ventaId}"
            };
        }

        private string ConstruirCuerpoEmail(MensajePedido mensaje)
        {
            var (titulo, descripcion, color) = mensaje.Estado switch
            {
                "Pedido recibido" => (
                    "Pedido recibido",
                    "Hemos recibido tu pedido y ya lo estamos procesando. Te avisaremos cuando sea despachado.",
                    "#2196F3"
                ),

                "Pedido despachado" => (
                    "Tu pedido va en camino",
                    $"Tu pedido ha sido despachado. Número de seguimiento: <strong>{mensaje.NumeroSeguimiento ?? "N/A"}</strong>",
                    "#FF9800"
                ),

                "Pedido entregado" => (
                    "Pedido entregado",
                    "Tu pedido fue entregado exitosamente. Esperamos que hayas disfrutado tu compra.",
                    "#4CAF50"
                ),

                "Cancelado" => (
                    "Pedido cancelado",
                    "Tu pedido ha sido cancelado. Si fue un error, contáctanos lo antes posible.",
                    "#F44336"
                ),

                _ => (
                    "Actualización de pedido",
                    $"El estado de tu pedido cambió a: {mensaje.Estado}",
                    "#9E9E9E"
                )
            };

            return $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head><meta charset='UTF-8'></head>
        <body style='font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px;'>
            <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                <div style='background-color: {color}; padding: 30px; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>{titulo}</h1>
                </div>
                <div style='padding: 30px;'>
                    <p style='font-size: 16px; color: #333;'>Hola <strong>{mensaje.ClienteNombre}</strong>,</p>
                    <p style='font-size: 15px; color: #555; line-height: 1.6;'>{descripcion}</p>

                    <div style='background-color: #f9f9f9; border-left: 4px solid {color}; padding: 15px; margin: 20px 0; border-radius: 4px;'>
                        <p style='margin: 5px 0; color: #333;'><strong>Número de pedido:</strong> #{mensaje.VentaId}</p>
                        <p style='margin: 5px 0; color: #333;'><strong>Total:</strong> Q{mensaje.Total:F2}</p>
                        <p style='margin: 5px 0; color: #333;'><strong>Estado:</strong> {mensaje.Estado}</p>
                        <p style='margin: 5px 0; color: #333;'><strong>Fecha:</strong> {mensaje.FechaEvento:dd/MM/yyyy HH:mm}</p>
                    </div>

                    <p style='font-size: 14px; color: #777;'>
                        Si tienes alguna pregunta, responde a este correo y con gusto te ayudamos.
                    </p>
                </div>

                <div style='background-color: #f5f5f5; padding: 15px; text-align: center;'>
                    <p style='color: #999; font-size: 12px; margin: 0;'>
                        Supermercado Online &copy; {DateTime.Now.Year}
                    </p>
                </div>
            </div>
        </body>
        </html>";
        }


    }       
}
