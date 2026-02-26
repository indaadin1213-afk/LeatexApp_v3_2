
using System;
using System.Collections.Generic;

namespace LeatexApp.Models
{
    public class Otpremnica
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BrojOtpremnice { get; set; } = string.Empty;
        public DateTime DatumKreiranja { get; set; } = DateTime.Now;
        public string Izdao { get; set; } = string.Empty;
        public string Primio { get; set; } = string.Empty;
        public List<OtpremnicaStavka> Stavke { get; set; } = new();
    }

    public class OtpremnicaStavka
    {
        public string Naziv { get; set; } = string.Empty;
        public string Sifra { get; set; } = string.Empty;
        public List<string> Serijski { get; set; } = new();
    }
}
