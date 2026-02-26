
using System;

namespace LeatexApp.Models
{
    public class UlazLog
    {
        public DateTime Datum { get; set; } = DateTime.Now;
        public string Sifra { get; set; } = string.Empty;
        public string Naziv { get; set; } = string.Empty;
        public string Serijski { get; set; } = string.Empty;
        public string Korisnik { get; set; } = string.Empty;
    }
}
