
using System;
using System.Windows.Forms;
using LeatexApp.Models;
using LeatexApp.Services;

namespace LeatexApp.Forms
{
    public class LoginForm : Form
    {
        TextBox txtUser = new TextBox();
        TextBox txtPass = new TextBox();
        Controls.ModernButton btnOk = new Controls.ModernButton();
        public User? LoggedInUser { get; private set; }

        public LoginForm()
        {
            this.Text = "Leatex – Prijava";
            this.Width = 360; this.Height = 220; this.FormBorderStyle = FormBorderStyle.FixedDialog; this.MaximizeBox = false;
            this.BackColor = ThemeService.LightBack; ForeColor = ThemeService.Text;

            var lblU = new Label(){ Text = "Korisničko ime", Left = 20, Top = 25, Width=120 };
            txtUser.Left = 160; txtUser.Top = 20; txtUser.Width = 150; txtUser.Text = "radnik";
            var lblP = new Label(){ Text = "Lozinka", Left = 20, Top = 65, Width=120 };
            txtPass.Left = 160; txtPass.Top = 60; txtPass.Width = 150; txtPass.PasswordChar='•';

            btnOk.Text = "Prijava"; btnOk.Left = 160; btnOk.Top = 110; btnOk.Width = 150;
            btnOk.Click += (s,e) =>
            {
                var u = SecurityService.Authenticate(txtUser.Text.Trim(), txtPass.Text.Trim());
                if (u == null){ MessageBox.Show("Neispravno korisničko ime ili lozinka."); return; }
                LoggedInUser = u; this.DialogResult = DialogResult.OK; this.Close();
            };

            this.Controls.AddRange(new Control[]{ lblU, txtUser, lblP, txtPass, btnOk});
        }
    }
}
