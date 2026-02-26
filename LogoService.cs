
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LeatexApp.Models;

namespace LeatexApp.Services
{
    public static class StorageService
    {
        public static string DataDir => Path.Combine(AppContext.BaseDirectory, "data");
        private static string UsersPath => Path.Combine(DataDir, "users.json");
        private static string OtpremnicePath => Path.Combine(DataDir, "otpremnice.json");
        private static string StanjePath => Path.Combine(DataDir, "stanje.json");
        private static string ArtikliPath => Path.Combine(DataDir, "artikli.json");
        private static string UlaziPath => Path.Combine(DataDir, "ulazi.json");
        private static string PendingDekPath => Path.Combine(DataDir, "pending_deklaracije.json");

        public static void Init()
        {
            Directory.CreateDirectory(DataDir);
            if (!File.Exists(UsersPath))
            {
                var radnik = SecurityService.CreateUser("radnik", "1234", Uloga.Radnik);
                var direktor = SecurityService.CreateUser("direktor", "vlado2004", Uloga.Direktor);
                SaveUsers(new List<User>{ radnik, direktor });
            }
            if (!File.Exists(OtpremnicePath)) SaveOtpremnice(new List<Otpremnica>());
            if (!File.Exists(StanjePath)) SaveStanje(new StanjeSkladista());
            if (!File.Exists(ArtikliPath)) SaveArtikli(new List<Artikal>());
            if (!File.Exists(UlaziPath)) SaveUlazi(new List<UlazLog>());
            if (!File.Exists(PendingDekPath)) SavePendingDeklaracije(new List<PendingDeklaracija>());
        }

        // Users
        public static List<User> LoadUsers()
        {
            if (!File.Exists(UsersPath)) return new();
            var json = File.ReadAllText(UsersPath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new();
        }
        public static void SaveUsers(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions{WriteIndented=true});
            File.WriteAllText(UsersPath, json);
        }

        // Otpremnice
        public static List<Otpremnica> LoadOtpremnice()
        {
            var json = File.ReadAllText(OtpremnicePath);
            return JsonSerializer.Deserialize<List<Otpremnica>>(json) ?? new();
        }
        public static void SaveOtpremnice(List<Otpremnica> list)
        {
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions{WriteIndented=true});
            File.WriteAllText(OtpremnicePath, json);
        }

        // Stanje
        public static StanjeSkladista LoadStanje()
        {
            var json = File.ReadAllText(StanjePath);
            return JsonSerializer.Deserialize<StanjeSkladista>(json) ?? new();
        }
        public static void SaveStanje(StanjeSkladista stanje)
        {
            var json = JsonSerializer.Serialize(stanje, new JsonSerializerOptions{WriteIndented=true});
            File.WriteAllText(StanjePath, json);
        }

        public static void PovecajStanje(string sifra, string naziv, int kolicina)
        {
            var s = LoadStanje();
            var item = s.Stanja.FirstOrDefault(x => x.Sifra == sifra);
            if (item == null){ item = new StanjeArtikla{Sifra=sifra, Naziv=naziv, Kolicina=0}; s.Stanja.Add(item);}    
            item.Kolicina += kolicina;
            SaveStanje(s);
        }
        public static bool SmanjiStanje(string sifra, int kolicina)
        {
            var s = LoadStanje();
            var item = s.Stanja.FirstOrDefault(x => x.Sifra == sifra);
            if (item == null) return false;
            if (item.Kolicina < kolicina) return false;
            item.Kolicina -= kolicina;
            SaveStanje(s);
            return true;
        }

        // Artikli
        public static List<Artikal> LoadArtikli()
        {
            var json = File.ReadAllText(ArtikliPath);
            return JsonSerializer.Deserialize<List<Artikal>>(json) ?? new();
        }
        public static void SaveArtikli(List<Artikal> list)
        {
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions{WriteIndented=true});
            File.WriteAllText(ArtikliPath, json);
        }

        // ULAZ log
        public static List<UlazLog> LoadUlazi()
        {
            var json = File.ReadAllText(UlaziPath);
            return JsonSerializer.Deserialize<List<UlazLog>>(json) ?? new();
        }
        public static void SaveUlazi(List<UlazLog> list)
        {
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions{WriteIndented=true});
            File.WriteAllText(UlaziPath, json);
        }

        // Pending deklaracije
        public static List<PendingDeklaracija> LoadPendingDeklaracije()
        {
            var json = File.ReadAllText(PendingDekPath);
            return JsonSerializer.Deserialize<List<PendingDeklaracija>>(json) ?? new();
        }
        public static void SavePendingDeklaracije(List<PendingDeklaracija> list)
        {
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions{WriteIndented=true});
            File.WriteAllText(PendingDekPath, json);
        }

        public static List<string> GenerateSerials(int count)
        {
            var set = SettingsService.Load();
            var start = set.NextSerial;
            var list = new List<string>();
            for(int i=0;i<count;i++) list.Add((start+i).ToString());
            set.NextSerial = start + count;
            SettingsService.Save(set);
            return list;
        }

        public static bool ValidatePendingMatch(string sifra, string serijski, out PendingDeklaracija? match)
        {
            match = null;
            var list = LoadPendingDeklaracije();
            match = list.FirstOrDefault(x=> !x.Iskoristeno && x.Serijski == serijski);
            if (match == null) return false;
            if (!match.Sifra.Equals(sifra, System.StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        public static void MarkPendingUsed(string serijski)
        {
            var list = LoadPendingDeklaracije();
            var m = list.FirstOrDefault(x=>x.Serijski==serijski);
            if (m!=null) { m.Iskoristeno = true; SavePendingDeklaracije(list); }
        }
    }
}
