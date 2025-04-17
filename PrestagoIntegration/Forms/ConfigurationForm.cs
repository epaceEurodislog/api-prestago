using System;
using System.Drawing;
using System.Windows.Forms;
using PrestagoIntegration.Services;
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
        private Button buttonTest;

        // Constantes pour les couleurs et styles
        private readonly Color PRIMARY_COLOR = Color.FromArgb(0, 120, 215);
        private readonly Color SECONDARY_COLOR = Color.FromArgb(243, 243, 243);
        private readonly Color SUCCESS_COLOR = Color.FromArgb(40, 167, 69);
        private readonly Font HEADER_FONT = new Font("Segoe UI", 12, FontStyle.Bold);
        private readonly Font NORMAL_FONT = new Font("Segoe UI", 9, FontStyle.Regular);

        public ConfigurationForm(AppConfig config)
        {
            _config = config;
            InitializeComponent();
            LoadConfigValues();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configuration de base du formulaire
            this.Text = "Configuration Prestago";
            this.Size = new Size(550, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = NORMAL_FONT;

            // Titre
            Label labelTitle = new Label
            {
                Text = "Configuration de l'intégration Prestago",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = HEADER_FONT,
                ForeColor = PRIMARY_COLOR,
                BackColor = SECONDARY_COLOR
            };

            // Création du tableau pour les champs
            TableLayoutPanel tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(20),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            // URL de l'API
            var labelApiUrl = new Label
            {
                Text = "URL de l'API:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            textBoxApiUrl = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            // Login
            var labelLogin = new Label
            {
                Text = "Login:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            textBoxLogin = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            // Mot de passe
            var labelPassword = new Label
            {
                Text = "Mot de passe:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            textBoxPassword = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '*',
                UseSystemPasswordChar = true,
                Padding = new Padding(5)
            };

            // Code dépôt par défaut
            var labelDefaultStockOutletCode = new Label
            {
                Text = "Code dépôt par défaut:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            textBoxDefaultStockOutletCode = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            // Ajout des contrôles au tableau
            tableLayout.Controls.Add(labelApiUrl, 0, 0);
            tableLayout.Controls.Add(textBoxApiUrl, 1, 0);
            tableLayout.Controls.Add(labelLogin, 0, 1);
            tableLayout.Controls.Add(textBoxLogin, 1, 1);
            tableLayout.Controls.Add(labelPassword, 0, 2);
            tableLayout.Controls.Add(textBoxPassword, 1, 2);
            tableLayout.Controls.Add(labelDefaultStockOutletCode, 0, 3);
            tableLayout.Controls.Add(textBoxDefaultStockOutletCode, 1, 3);

            // Boutons d'action
            FlowLayoutPanel panelButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 60,
                Padding = new Padding(10)
            };

            buttonCancel = new Button
            {
                Text = "Annuler",
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = SECONDARY_COLOR,
                DialogResult = DialogResult.Cancel
            };
            buttonCancel.Click += buttonCancel_Click;

            buttonSave = new Button
            {
                Text = "Enregistrer",
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = PRIMARY_COLOR,
                ForeColor = Color.White,
                DialogResult = DialogResult.OK
            };
            buttonSave.Click += buttonSave_Click;

            buttonTest = new Button
            {
                Text = "Tester la connexion",
                Size = new Size(150, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = SUCCESS_COLOR,
                ForeColor = Color.White
            };
            buttonTest.Click += buttonTest_Click;

            panelButtons.Controls.Add(buttonCancel);
            panelButtons.Controls.Add(buttonSave);
            panelButtons.Controls.Add(buttonTest);

            // Ajout au formulaire
            this.Controls.Add(tableLayout);
            this.Controls.Add(panelButtons);
            this.Controls.Add(labelTitle);

            this.AcceptButton = buttonSave;
            this.CancelButton = buttonCancel;

            this.ResumeLayout(false);
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

        private async void buttonTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxLogin.Text) || string.IsNullOrWhiteSpace(textBoxPassword.Text))
            {
                MessageBox.Show("Veuillez remplir les champs de login et mot de passe pour tester la connexion.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            buttonTest.Enabled = false;
            buttonTest.Text = "Test en cours...";
            this.UseWaitCursor = true;

            try
            {
                // Créer un service temporaire pour tester
                var tempService = new PrestagoService(
                    textBoxApiUrl.Text,
                    textBoxLogin.Text,
                    textBoxPassword.Text);

                bool success = await tempService.AuthenticateAsync();

                if (success)
                {
                    MessageBox.Show("Connexion à l'API Prestago réussie !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Échec de la connexion à l'API Prestago. Vérifiez vos paramètres.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du test : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("Test de connexion", ex);
            }
            finally
            {
                buttonTest.Enabled = true;
                buttonTest.Text = "Tester la connexion";
                this.UseWaitCursor = false;
            }
        }
    }
}