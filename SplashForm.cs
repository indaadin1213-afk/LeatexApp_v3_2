
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LeatexApp.Models;
using LeatexApp.Services;

namespace LeatexApp.Forms
{
    public class MainForm : Form
    {
        private readonly User _currentUser;
        private TabControl tabs = new TabControl();

        // cache artikli
        List<Artikal> _artikli = new List<Artikal>();

        // Header (simple)
        Panel header = new Panel();
        Label lblTitle = new Label();
        Label lblContacts = new Label();

        // Deklaracija (PRINT) controls
        ComboBox dNaziv = new ComboBox();
        TextBox dSifra = new TextBox();
        NumericUpDown dCount = new NumericUpDown();
        Controls.ModernButton dGen = new Controls.ModernButton();

        // ULAZ controls
        TextBox uSifra = new TextBox();
        TextBox uSerijski = new TextBox();
        Label uStatus = new Label();
        ListView uLast = new ListView();

        // Otpremnica controls (sačuvano)
        ComboBox oNaziv = new ComboBox();
        ComboBox oSifra = new ComboBox();
        TextBox oSerijski = new TextBox();
        Controls.ModernButton oAddSer = new Controls.ModernButton();
        Controls.ModernButton oDodajStavku = new Controls.ModernButton();
        Controls.ModernButton oGen = new Controls.ModernButton();
        ListView oList = new ListView();
        List<OtpremnicaStavka> stavke = new();
        TextBox oIzdao = new TextBox();
        TextBox oPrimio = new TextBox();

        public MainForm(User user)
        {
            _currentUser = user;
            this.Text = $"Leatex – Glavni meni ({_currentUser.Role})";
            this.Width = 1200; this.Height = 800; this.BackColor = ThemeService.LightBack; this.ForeColor = ThemeService.Text;
            BuildHeader();
            tabs.Dock = DockStyle.Fill; this.Controls.Add(tabs);
            LoadArtikliCache();
            InitDeklaracijaTab();
            InitUlazTab();
            InitOtpremnicaTab();
        }

        void BuildHeader(){ header.Dock = DockStyle.Top; header.Height = 70; header.BackColor = System.Drawing.Color.White; header.Padding = new Padding(12,8,12,8); lblTitle.Text = SettingsService.Load().KompanijaNaziv; lblTitle.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold); lblTitle.Dock = DockStyle.Top; lblContacts.Text = $"{SettingsService.Load().Telefon1}   •   {SettingsService.Load().Telefon2}   •   {SettingsService.Load().Email}"; lblContacts.Dock = DockStyle.Bottom; lblContacts.ForeColor = System.Drawing.Color.DimGray; header.Controls.Add(lblTitle); header.Controls.Add(lblContacts); this.Controls.Add(header);}    
        void LoadArtikliCache(){ _artikli = StorageService.LoadArtikli(); }
        Artikal? FindBySifra(string s)=> _artikli.FirstOrDefault(x=>x.Sifra.Equals(s, StringComparison.OrdinalIgnoreCase));
        Artikal? FindByNaziv(string n)=> _artikli.FirstOrDefault(x=>x.Naziv.Equals(n, StringComparison.OrdinalIgnoreCase));

        // ===== Deklaracija PRINT =====
        void InitDeklaracijaTab()
        {
            var tab = new TabPage("Deklaracija (print)");
            var lbl1 = new Label(){Text="Naziv", Left=20, Top=20}; dNaziv.Left=140; dNaziv.Top=20; dNaziv.Width=300; dNaziv.DropDownStyle=ComboBoxStyle.DropDown;
            var lbl2 = new Label(){Text="Šifra", Left=20, Top=50}; dSifra.Left=140; dSifra.Top=50; dSifra.Width=200; dSifra.ReadOnly=true;
            var lbl3 = new Label(){Text="Broj deklaracija", Left=20, Top=80}; dCount.Left=140; dCount.Top=80; dCount.Width=120; dCount.Minimum=1; dCount.Maximum=100000; dCount.Value=33;
            dNaziv.Items.AddRange(_artikli.Select(a=>a.Naziv).ToArray());
            dNaziv.SelectedIndexChanged += (s,e)=>{ var a = FindByNaziv(dNaziv.Text); if(a!=null) dSifra.Text = a.Sifra; };
            dNaziv.Leave += (s,e)=>{ var a = FindByNaziv(dNaziv.Text); if(a!=null) dSifra.Text = a.Sifra; };

            dGen.Text = "Generiši deklaracije"; dGen.Left=140; dGen.Top=120; dGen.Width=220; dGen.Click += (s,e)=>{
                if(string.IsNullOrWhiteSpace(dNaziv.Text)){ MessageBox.Show("Unesi naziv."); return; }
                var a = FindByNaziv(dNaziv.Text);
                if(a==null){ if(MessageBox.Show("Artikal ne postoji. Dodati?","Potvrda", MessageBoxButtons.YesNo)==DialogResult.Yes){ a = new Artikal{ Naziv=dNaziv.Text.Trim(), Sifra = PromptForSifra() }; var list = StorageService.LoadArtikli(); list.Add(a); StorageService.SaveArtikli(list); LoadArtikliCache(); } else return; }
                dSifra.Text = a!.Sifra;
                int count = (int)dCount.Value;
                var serials = StorageService.GenerateSerials(count);
                // upis u pending
                var pending = StorageService.LoadPendingDeklaracije();
                foreach(var ser in serials) pending.Add(new PendingDeklaracija{ Naziv=a.Naziv, Sifra=a.Sifra, Serijski=ser, Datum=DateTime.Now, Iskoristeno=false });
                StorageService.SavePendingDeklaracije(pending);
                var file = PdfService.GenerateDeklaracije(a.Naziv, a.Sifra, serials, SettingsService.Load());
                int pages = (int)Math.Ceiling(count/4.0);
                MessageBox.Show($"Generisano {count} deklaracija (≈ {pages} A4 stranica)\n{file}");
            };

            tab.Controls.AddRange(new Control[]{lbl1,dNaziv,lbl2,dSifra,lbl3,dCount,dGen});
            tabs.TabPages.Add(tab);
        }

        string PromptForSifra(){ using var f = new Form(); f.Text="Nova šifra"; f.Width=360; f.Height=140; var t = new TextBox(){Left=20,Top=20,Width=300}; var b=new Button(){Left=240,Top=60,Text="OK"}; string sifra=""; b.Click+=(s,e)=>{ sifra=t.Text.Trim(); if(string.IsNullOrWhiteSpace(sifra)){ MessageBox.Show("Unesi šifru."); return;} f.DialogResult=DialogResult.OK; f.Close();}; f.Controls.AddRange(new Control[]{ new Label(){Left=20,Top=0,Text="Unesi šifru"}, t, b}); return f.ShowDialog()==DialogResult.OK? sifra: ""; }

        // ===== ULAZ TAB =====
        void InitUlazTab()
        {
            var tab = new TabPage("ULAZ");
            var lbl1 = new Label(){Text="Šifra", Left=20, Top=20}; uSifra.Left=140; uSifra.Top=20; uSifra.Width=200;
            var lbl2 = new Label(){Text="Serijski", Left=20, Top=50}; uSerijski.Left=140; uSerijski.Top=50; uSerijski.Width=200;
            uStatus.Left=20; uStatus.Top=90; uStatus.Width=700; uStatus.ForeColor=System.Drawing.Color.DimGray;
            uSerijski.KeyDown += (s,e)=>{ if(e.KeyCode==Keys.Enter){ ProcessUlaz(); e.Handled=true; e.SuppressKeyPress=true; }};

            uLast.Left=20; uLast.Top=130; uLast.Width=1120; uLast.Height=500; uLast.View=View.Details; uLast.FullRowSelect=true; uLast.Columns.Add("Datum",160); uLast.Columns.Add("Šifra",120); uLast.Columns.Add("Serijski",200); uLast.Columns.Add("Korisnik",120);

            tab.Controls.AddRange(new Control[]{ lbl1,uSifra,lbl2,uSerijski,uStatus,uLast });
            tabs.TabPages.Add(tab);
        }

        void ProcessUlaz()
        {
            var sifra = uSifra.Text.Trim(); var ser = uSerijski.Text.Trim();
            if(string.IsNullOrWhiteSpace(sifra) || string.IsNullOrWhiteSpace(ser)){ uStatus.Text = "Unesi šifru i serijski."; uStatus.ForeColor = System.Drawing.Color.Maroon; return; }
            if(!StorageService.ValidatePendingMatch(sifra, ser, out var match))
            {
                uStatus.Text = "Greška: serijski ne postoji na deklaracijama ili se šifra ne poklapa."; uStatus.ForeColor = System.Drawing.Color.Maroon; return;
            }
            // OK → knjiži
            StorageService.PovecajStanje(match!.Sifra, match!.Naziv, 1);
            StorageService.MarkPendingUsed(ser);
            var ul = StorageService.LoadUlazi(); ul.Add(new UlazLog{ Sifra=match.Sifra, Naziv=match.Naziv, Serijski=ser, Korisnik=_currentUser.Username }); StorageService.SaveUlazi(ul);
            uStatus.Text = $"ULAZ OK: {match.Naziv} ({match.Sifra}) – {ser}"; uStatus.ForeColor = System.Drawing.Color.ForestGreen;
            // Dodaj u listu (zadnjih 10)
            uLast.Items.Insert(0, new ListViewItem(new string[]{ DateTime.Now.ToString("dd.MM.yyyy HH:mm"), match.Sifra, ser, _currentUser.Username }));
            while(uLast.Items.Count>10) uLast.Items.RemoveAt(uLast.Items.Count-1);
            // B: ostavi šifru, očisti serijski
            uSerijski.Text = string.Empty; uSerijski.Focus();
        }

        // ===== Otpremnica (sačuvano) =====
        void InitOtpremnicaTab()
        {
            var tab = new TabPage("Otpremnica (izlaz)");
            var lbl1 = new Label(){Text="Naziv", Left=20, Top=20}; oNaziv.Left=140; oNaziv.Top=20; oNaziv.Width=250;
            var lbl2 = new Label(){Text="Šifra", Left=20, Top=50}; oSifra.Left=140; oSifra.Top=50; oSifra.Width=150;
            var lbl3 = new Label(){Text="Serijski (ENTER)", Left=20, Top=80}; oSerijski.Left=140; oSerijski.Top=80; oSerijski.Width=250;
            oSerijski.KeyDown += (s,e)=>{ if(e.KeyCode==Keys.Enter){ AddSerijskiToCurrent(); e.Handled=true; e.SuppressKeyPress=true; }};
            oAddSer.Text = "+ Dodaj serijski"; oAddSer.Left=400; oAddSer.Top=80; oAddSer.Click += (s,e)=> AddSerijskiToCurrent();
            oDodajStavku.Text = "+ Dodaj stavku"; oDodajStavku.Left=140; oDodajStavku.Top=110; oDodajStavku.Click += (s,e)=>{
                if(string.IsNullOrWhiteSpace(oNaziv.Text) || string.IsNullOrWhiteSpace(oSifra.Text)) { MessageBox.Show("Naziv/Šifra obavezni"); return; }
                var ser = GetTempSer(); if(ser.Count==0){ MessageBox.Show("Dodaj bar jedan serijski"); return; }
                var st = new OtpremnicaStavka{ Naziv=oNaziv.Text.Trim(), Sifra=oSifra.Text.Trim(), Serijski=ser };
                stavke.Add(st); RefreshOtpremnicaList(); oNaziv.Text=""; oSifra.Text=""; oSerijski.Text=""; _tempSer.Clear();
            };
            oGen.Text = "Generiši PDF"; oGen.Left=280; oGen.Top=110; oGen.Click += (s,e)=>{
                if (stavke.Count==0){ MessageBox.Show("Nema stavki"); return; }
                var settings = SettingsService.Load(); var o = new Otpremnica{ Izdao=oIzdao.Text.Trim(), Primio=oPrimio.Text.Trim(), Stavke=stavke.ToList() };
                var sazetak = stavke.GroupBy(sv=>sv.Sifra).ToDictionary(g=>g.Key, g=>g.Sum(x=>x.Serijski.Count));
                var file = PdfService.GenerateOtpremnica(o, settings, sazetak); MessageBox.Show($"Otpremnica generisana:\n{file}"); stavke.Clear(); RefreshOtpremnicaList(); };

            oList.Left=20; oList.Top=180; oList.Width=1120; oList.Height=240; oList.View=View.Details; oList.Columns.Add("Naziv",300); oList.Columns.Add("Šifra",100); oList.Columns.Add("Serijski",680);
            var lblI = new Label(){Text="Izdao", Left=20, Top=140}; oIzdao.Left=80; oIzdao.Top=140; oIzdao.Width=160; var lblP = new Label(){Text="Primio", Left=260, Top=140}; oPrimio.Left=320; oPrimio.Top=140; oPrimio.Width=160;
            oNaziv.DropDownStyle=ComboBoxStyle.DropDown; oSifra.DropDownStyle=ComboBoxStyle.DropDown; oNaziv.Items.AddRange(_artikli.Select(a=>a.Naziv).ToArray());
            oNaziv.SelectedIndexChanged += (s,e)=>{ var a = FindByNaziv(oNaziv.Text); if(a!=null) oSifra.Text = a.Sifra; };
            oSifra.SelectedIndexChanged += (s,e)=>{ var a = FindBySifra(oSifra.Text); if(a!=null) oNaziv.Text = a.Naziv; };

            tab.Controls.AddRange(new Control[]{lbl1,oNaziv,lbl2,oSifra,lbl3,oSerijski,oAddSer,oDodajStavku,oGen,oList,lblI,oIzdao,lblP,oPrimio});
            tabs.TabPages.Add(tab);
        }

        List<string> _tempSer = new();
        void AddSerijskiToCurrent(){ if(!string.IsNullOrWhiteSpace(oSerijski.Text)){ _tempSer.Add(oSerijski.Text.Trim()); oSerijski.Text=""; }}
        List<string> GetTempSer(){ return new List<string>(_tempSer); }
        void RefreshOtpremnicaList(){ oList.Items.Clear(); foreach(var st in stavke){ var item = new ListViewItem(new string[]{ st.Naziv, st.Sifra, string.Join(", ", st.Serijski)}); oList.Items.Add(item);} }
    }
}
