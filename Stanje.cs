
using System.Drawing;
using System.Windows.Forms;
using LeatexApp.Services;

namespace LeatexApp.Forms
{
    public class SplashForm : Form
    {
        public SplashForm()
        {
            this.FormBorderStyle = FormBorderStyle.None; this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White; this.Width = 520; this.Height = 300;
            var logo = LogoService.GetLogoImage();
            var pb = new PictureBox(){Left=30, Top=40, Width=460, Height=80, SizeMode=PictureBoxSizeMode.Zoom, Image = logo};
            var lbl1 = new Label(){Left=0, Top=150, Width=520, Height=30, TextAlign=ContentAlignment.MiddleCenter, Text = "LEATEX d.o.o. Lukavac", Font = new Font("Segoe UI", 12, FontStyle.Bold)};
            var lbl2 = new Label(){Left=0, Top=180, Width=520, Height=20, TextAlign=ContentAlignment.MiddleCenter, Text = "second‑hand & outlet veleprodaja", Font = new Font("Segoe UI", 10, FontStyle.Regular)};
            var lbl3 = new Label(){Left=0, Top=230, Width=520, Height=20, TextAlign=ContentAlignment.MiddleCenter, Text = "Učitavanje aplikacije…", ForeColor=Color.Gray};
            this.Controls.AddRange(new Control[]{pb,lbl1,lbl2,lbl3});
        }
        protected override bool ShowWithoutActivation => true;
    }
}
