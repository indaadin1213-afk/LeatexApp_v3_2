
using System;

namespace LeatexApp.Models
{
    public class PendingDeklaracija
    {
        public string Naziv { get; set; } = string.Empty;
        public string Sifra { get; set; } = string.Empty;
        public string Serijski { get; set; } = string.Empty;
        public DateTime Datum { get; set; } = DateTime.Now;
        public bool Iskoristeno { get; set; } = false;
    }
}
