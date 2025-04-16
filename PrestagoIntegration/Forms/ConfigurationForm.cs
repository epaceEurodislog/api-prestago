using System;
using System.Windows.Forms;
using PrestagoIntegration.Utils;

namespace PrestagoIntegration.Forms
{
    public partial class ConfigurationForm : Form
    {
        private readonly AppConfig _config;
        private TextBox textBoxApiUrl;
        private TextBox textBoxLogin;
        private TextBox textBoxPassword;
        private TextBox textBoxDefaultStockOutletCode;
        private Button buttonSave;
        private Button buttonCancel;

        public ConfigurationForm(AppConfig config)
        {
            _config = config;
            InitializeComponent();
            LoadConfigValues();
        }

        private void InitializeComponent()
        {
            this.Text = "Configuration Prestago";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Création des labels
            var labelApiUrl = new Label
            {
                Text = "URL de l'API:",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(150, 20)
            };

            var labelLogin = new Label
            {
                Text = "Login:",
                Location = new System.Drawing.Point(20, 80),
                Size = new System.Drawing.Size(150, 20)
            };

            var labelPassword = new Label
            {
                Text = "Mot de passe:",
                Location = new System.Drawing.Point(20, 140),
                Size = new System.Drawing.Size(150, 20)
            };

            var labelDefaultStockOutletCode = new Label
            {
                Text = "Code dépôt par défaut:",
                Location = new System.Drawing.Point(20, 200),
                Size = new System.Drawing.Size(150, 20)
            };

            // Création des TextBox
            textBoxApiUrl = new TextBox
            {
                Location = new System.Drawing.Point(20, 45),
                Size = new System.Drawing.Size(440, 25)
            };

            textBoxLogin = new TextBox
            {
                Location = new System.Drawing.Point(20, 105),
                Size = new System.Drawing.Size(440, 25)
            };

            textBoxPassword = new TextBox
            {
                Location = new System.Drawing.Point(20, 165),
                Size = new System.Drawing.Size(440, 25),
                PasswordChar = '*',
                UseSystemPasswordChar = true
            };

            textBoxDefaultStockOutletCode = new TextBox
            {
                Location = new System.Drawing.Point(20, 225),
                Size = new System.Drawing.Size(440, 25)
            };

            // Création des boutons
            buttonSave = new Button
            {
                Text = "Enregistrer",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(240, 300),
                Size = new System.Drawing.Size(100, 30)
            };
            buttonSave.Click += buttonSave_Click;

            buttonCancel = new Button
            {
                Text = "Annuler",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(360, 300),
                Size = new System.Drawing.Size(100, 30)
            };
            buttonCancel.Click += buttonCancel_Click;

            // Ajout des contrôles au formulaire
            this.Controls.Add(labelApiUrl);
            this.Controls.Add(textBoxApiUrl);
            this.Controls.Add(labelLogin);
            this.Controls.Add(textBoxLogin);
            this.Controls.Add(labelPassword);
            this.Controls.Add(textBoxPassword);
            this.Controls.Add(labelDefaultStockOutletCode);
            this.Controls.Add(textBoxDefaultStockOutletCode);
            this.Controls.Add(buttonSave);
            this.Controls.Add(buttonCancel);

            this.AcceptButton = buttonSave;
            this.CancelButton = buttonCancel;
        }

        private void LoadConfigValues()
        {
            textBoxApiUrl.Text = _config.ApiUrl;
            textBoxLogin.Text = _config.Login;
            textBoxPassword.Text = _config.Password;
            textBoxDefaultStockOutletCode.Text = _config.DefaultStockOutletCode;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            // Validation des entrées
            if (string.IsNullOrWhiteSpace(textBoxApiUrl.Text) ||
                string.IsNullOrWhiteSpace(textBoxLogin.Text) ||
                string.IsNullOrWhiteSpace(textBoxDefaultStockOutletCode.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Mise à jour de la configuration
            _config.ApiUrl = textBoxApiUrl.Text;
            _config.Login = textBoxLogin.Text;

            if (!string.IsNullOrWhiteSpace(textBoxPassword.Text))
            {
                _config.Password = textBoxPassword.Text;
            }

            _config.DefaultStockOutletCode = textBoxDefaultStockOutletCode.Text;

            // Sauvegarde
            AppConfig.SaveConfig(_config);

            MessageBox.Show("Configuration sauvegardée avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
