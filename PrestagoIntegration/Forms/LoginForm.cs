using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PrestagoIntegration.Forms
{
    public partial class LoginForm : Form
    {
        private TextBox textBoxUsername;
        private TextBox textBoxPassword;
        private Button buttonLogin;
        private Button buttonCancel;

        // Correction des attributs pour éviter les erreurs de sérialisation
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Username { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Password { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configuration du formulaire
            this.Text = "Connexion";
            this.Size = new System.Drawing.Size(400, 180); // Taille ajustée
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Panel principal
            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 20, 20, 10),
                ColumnCount = 2,
                RowCount = 3
            };

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            // Labels et champs de texte
            Label labelUsername = new Label
            {
                Text = "Identifiant:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            textBoxUsername = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 3, 0, 3)
            };

            Label labelPassword = new Label
            {
                Text = "Mot de passe:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            textBoxPassword = new TextBox
            {
                Dock = DockStyle.Fill,
                PasswordChar = '*',
                Margin = new Padding(0, 3, 0, 3)
            };

            // Panel de boutons
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Margin = new Padding(0, 10, 0, 0)
            };

            buttonLogin = new Button
            {
                Text = "Se connecter",
                Size = new Size(110, 28),
                Margin = new Padding(5, 0, 0, 0)
            };
            buttonLogin.Click += buttonLogin_Click;

            buttonCancel = new Button
            {
                Text = "Annuler",
                Size = new Size(90, 28)
            };
            buttonCancel.Click += buttonCancel_Click;

            buttonPanel.Controls.Add(buttonCancel);
            buttonPanel.Controls.Add(buttonLogin);

            // Ajouter tous les contrôles au panel principal
            mainPanel.Controls.Add(labelUsername, 0, 0);
            mainPanel.Controls.Add(textBoxUsername, 1, 0);
            mainPanel.Controls.Add(labelPassword, 0, 1);
            mainPanel.Controls.Add(textBoxPassword, 1, 1);
            mainPanel.Controls.Add(buttonPanel, 1, 2);

            // Ajouter le panel au formulaire
            this.Controls.Add(mainPanel);

            this.AcceptButton = buttonLogin;
            this.CancelButton = buttonCancel;

            this.ResumeLayout(false);
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxUsername.Text) ||
                string.IsNullOrWhiteSpace(textBoxPassword.Text))
            {
                MessageBox.Show("Veuillez saisir le nom d'utilisateur et le mot de passe.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Username = textBoxUsername.Text;
            Password = textBoxPassword.Text;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}