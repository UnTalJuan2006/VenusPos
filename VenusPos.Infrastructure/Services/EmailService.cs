using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using VenusPos.Application.Interfaces.Services;
using VenusPos.Domain.Entities;

namespace VenusPos.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarConfirmacionReservaAsync(Reserva reserva)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) ||
                string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
            {
                throw new InvalidOperationException("Configuración de email incompleta en appsettings.json");
            }

            var mascota = reserva.ReservaMascotas?.FirstOrDefault()?.Mascota;
            var cliente = reserva.Cliente;
            var empleado = reserva.Empleado;

            if (cliente == null || string.IsNullOrEmpty(cliente.Email))
            {
                throw new InvalidOperationException("Cliente sin email configurado");
            }

            var htmlBody = GenerarTemplateConfirmacion(
                cliente.Nombre,
                reserva.CodigoReserva ?? "PENDIENTE",
                reserva.FechaReserva,
                reserva.HoraInicio,
                reserva.HoraFin,
                mascota?.Nombre ?? "N/A",
                mascota?.Raza ?? "N/A",
                empleado?.Nombre ?? "Personal VenusMascotas",
                reserva.PrecioTotal,
                reserva.DuracionMinutos
            );

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = $"Confirmación de Reserva {reserva.CodigoReserva} - VenusMascotas",
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(cliente.Email, cliente.Nombre));

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }

        private string GenerarTemplateConfirmacion(
            string nombreCliente,
            string codigoReserva,
            DateTime fechaReserva,
            TimeOnly horaInicio,
            TimeOnly horaFin,
            string nombreMascota,
            string razaMascota,
            string nombreEmpleado,
            decimal precioTotal,
            int duracionMinutos)
        {
            var fechaFormateada = fechaReserva.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
            var horaFormateada = $"{horaInicio:HH:mm} - {horaFin:HH:mm}";
            var precioFormateado = precioTotal.ToString("C0", new System.Globalization.CultureInfo("es-CO"));

            return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Confirmación de Reserva - VenusMascotas</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f9;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f4f4f9; padding: 20px 0;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);"">

                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #8b5cf6 0%, #6d28d9 100%); padding: 40px 20px; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 600;"">VenusMascotas</h1>
                            <p style=""margin: 10px 0 0 0; color: #e9d5ff; font-size: 14px;"">Cuidado profesional para tu mejor amigo</p>
                        </td>
                    </tr>

                    <!-- Success Icon -->
                    <tr>
                        <td style=""padding: 30px 20px 20px; text-align: center;"">
                            <div style=""display: inline-block; width: 80px; height: 80px; background-color: #10b981; border-radius: 50%; position: relative;"">
                                <span style=""color: white; font-size: 48px; line-height: 80px;"">✓</span>
                            </div>
                            <h2 style=""margin: 20px 0 10px 0; color: #1f2937; font-size: 24px;"">¡Reserva Confirmada!</h2>
                            <p style=""margin: 0; color: #6b7280; font-size: 16px;"">Hola {nombreCliente}, tu cita ha sido confirmada exitosamente.</p>
                        </td>
                    </tr>

                    <!-- Reservation Code -->
                    <tr>
                        <td style=""padding: 0 20px 30px; text-align: center;"">
                            <div style=""background-color: #f3f4f6; border-radius: 8px; padding: 20px; display: inline-block;"">
                                <p style=""margin: 0 0 5px 0; color: #6b7280; font-size: 12px; text-transform: uppercase; letter-spacing: 1px;"">Código de Reserva</p>
                                <p style=""margin: 0; color: #8b5cf6; font-size: 32px; font-weight: 700; letter-spacing: 2px;"">{codigoReserva}</p>
                            </div>
                        </td>
                    </tr>

                    <!-- Reservation Details -->
                    <tr>
                        <td style=""padding: 0 40px 30px;"">
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td style=""padding: 15px 0; border-bottom: 1px solid #e5e7eb;"">
                                        <table width=""100%"">
                                            <tr>
                                                <td style=""color: #6b7280; font-size: 14px; width: 40%;"">📅 Fecha</td>
                                                <td style=""color: #1f2937; font-size: 14px; font-weight: 600; text-align: right;"">{fechaFormateada}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 15px 0; border-bottom: 1px solid #e5e7eb;"">
                                        <table width=""100%"">
                                            <tr>
                                                <td style=""color: #6b7280; font-size: 14px; width: 40%;"">🕐 Horario</td>
                                                <td style=""color: #1f2937; font-size: 14px; font-weight: 600; text-align: right;"">{horaFormateada}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 15px 0; border-bottom: 1px solid #e5e7eb;"">
                                        <table width=""100%"">
                                            <tr>
                                                <td style=""color: #6b7280; font-size: 14px; width: 40%;"">⏱️ Duración</td>
                                                <td style=""color: #1f2937; font-size: 14px; font-weight: 600; text-align: right;"">{duracionMinutos} minutos</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 15px 0; border-bottom: 1px solid #e5e7eb;"">
                                        <table width=""100%"">
                                            <tr>
                                                <td style=""color: #6b7280; font-size: 14px; width: 40%;"">🐾 Mascota</td>
                                                <td style=""color: #1f2937; font-size: 14px; font-weight: 600; text-align: right;"">{nombreMascota} ({razaMascota})</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 15px 0; border-bottom: 1px solid #e5e7eb;"">
                                        <table width=""100%"">
                                            <tr>
                                                <td style=""color: #6b7280; font-size: 14px; width: 40%;"">👨‍⚕️ Profesional</td>
                                                <td style=""color: #1f2937; font-size: 14px; font-weight: 600; text-align: right;"">{nombreEmpleado}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 15px 0;"">
                                        <table width=""100%"">
                                            <tr>
                                                <td style=""color: #6b7280; font-size: 14px; width: 40%;"">💰 Precio Total</td>
                                                <td style=""color: #8b5cf6; font-size: 18px; font-weight: 700; text-align: right;"">{precioFormateado}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Important Information -->
                    <tr>
                        <td style=""padding: 0 40px 30px;"">
                            <div style=""background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; border-radius: 4px;"">
                                <p style=""margin: 0; color: #92400e; font-size: 13px; line-height: 1.6;"">
                                    <strong>Información importante:</strong><br>
                                    • Por favor llega 10 minutos antes de tu cita<br>
                                    • Trae la cartilla de vacunación de tu mascota<br>
                                    • Si necesitas cancelar, hazlo con al menos 24 horas de anticipación<br>
                                    • Guarda este código de reserva para tu visita
                                </p>
                            </div>
                        </td>
                    </tr>

                    <!-- Contact -->
                    <tr>
                        <td style=""padding: 0 40px 30px; text-align: center;"">
                            <p style=""margin: 0 0 10px 0; color: #6b7280; font-size: 14px;"">¿Necesitas ayuda?</p>
                            <p style=""margin: 0; color: #8b5cf6; font-size: 14px;"">
                                📞 Llámanos: (601) 123-4567<br>
                                📧 Email: info@venusmascotas.com
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f9fafb; padding: 20px; text-align: center; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0; color: #9ca3af; font-size: 12px;"">
                                © 2025 VenusMascotas. Todos los derechos reservados.<br>
                                Este es un correo automático, por favor no responder.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
