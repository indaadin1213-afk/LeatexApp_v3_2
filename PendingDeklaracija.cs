
using System.Collections.Generic;

namespace LeatexApp.Models
{
    public class StanjeArtikla
    {
        public string Sifra { get; set; } = string.Empty;
        public string Naziv { get; set; } = string.Empty;
        public int Kolicina { get; set; }
    }

    public class StanjeSkladista
    {
        public List<StanjeArtikla> Stanja { get; set; } = new();
    }
}
