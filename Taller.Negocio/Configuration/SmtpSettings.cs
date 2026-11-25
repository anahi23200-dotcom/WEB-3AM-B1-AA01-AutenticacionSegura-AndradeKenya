namespace Taller.Negocio.Configuration
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string RemitenteCorreo { get; set; } = string.Empty;
        public string RemitenteNombre { get; set; } = "Taller Automotriz - Seguridad";
        public string Usuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }
}