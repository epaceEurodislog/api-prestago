using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using PrestagoIntegration.Models;
using PrestagoIntegration.Services;
using PrestagoIntegration.Utils;
using PrestagoIntegration.Forms;

namespace PrestagoIntegration
{
    public partial class MainForm : Form
    {
        private PrestagoService _prestagoService;
        private readonly AppConfig _config;
        private List<ReceptionItem> _receptionItems = new List<ReceptionItem>();
        private List<ExpeditionItem> _expeditionItems = new List<ExpeditionItem>();
        private string _targetStockCode = "";

        // Constantes pour les couleurs et styles
        private readonly Color PRIMARY_COLOR = Color.FromArgb(0, 120, 215);     // Bleu principal
        private readonly Color SECONDARY_COLOR = Color.FromArgb(243, 243, 243); // Gris clair
        private readonly Color SUCCESS_COLOR = Color.FromArgb(40, 167, 69);     // Vert
        private readonly Color WARNING_COLOR = Color.FromArgb(255, 193, 7);     // Jaune
        private readonly Color DANGER_COLOR = Color.FromArgb(220, 53, 69);      // Rouge
        private readonly Color TEXT_COLOR = Color.FromArgb(33, 37, 41);         // Gris foncé pour texte
        private readonly Font HEADER_FONT = new Font("Segoe UI", 16, FontStyle.Regular);
        private readonly Font NORMAL_FONT = new Font("Segoe UI", 9, FontStyle.Regular);
        private readonly Font BUTTON_FONT = new Font("Segoe UI", 9, FontStyle.Regular);

        // UI Controls
        private MenuStrip menuStrip;
        private TabControl tabControl;
        private TabPage tabPageReception;
        private TabPage tabPageExpedition;
        private Panel panelHeader;
        private Label labelTitle;
        private TextBox textBoxStockOutletCode;
        private Button buttonLogin;
        private Button buttonSettings;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripProgressBar progressBar;

        public MainForm()
        {
            InitializeComponent();

            // Charger la configuration
            _config = AppConfig.Instance;

            // Initialiser le service avec les valeurs de configuration
            _prestagoService = new PrestagoService(
                _config.ApiUrl,
                _config.Login,
                _config.Password);

            // Charger configuration et configurer UI
            LoadConfiguration();
            SetupDataGridViews();

            // Tester la connexion automatiquement si des identifiants sont déjà configurés
            if (!string.IsNullOrEmpty(_config.Login) && !string.IsNullOrEmpty(_config.Password))
            {
                TestConnectionAsync();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configuration de base du formulaire
            this.Text = "PMU Prestago Integration";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = NORMAL_FONT;
            this.BackColor = SECONDARY_COLOR;
            this.ForeColor = TEXT_COLOR;

            // Menu principal
            menuStrip = new MenuStrip
            {
                BackColor = PRIMARY_COLOR,
                ForeColor = Color.White,
                Padding = new Padding(6, 2, 0, 2),
                Font = NORMAL_FONT
            };

            var fileMenuItem = new ToolStripMenuItem("Fichier");
            var configMenuItem = new ToolStripMenuItem("Configuration");
            configMenuItem.Click += (s, e) => ShowConfigurationForm();

            var aboutMenuItem = new ToolStripMenuItem("À propos");
            aboutMenuItem.Click += (s, e) => ShowAboutDialog();

            var exitMenuItem = new ToolStripMenuItem("Quitter");
            exitMenuItem.Click += (s, e) => Application.Exit();

            fileMenuItem.DropDownItems.Add(configMenuItem);
            fileMenuItem.DropDownItems.Add(new ToolStripSeparator());
            fileMenuItem.DropDownItems.Add(exitMenuItem);

            var helpMenuItem = new ToolStripMenuItem("Aide");
            helpMenuItem.DropDownItems.Add(aboutMenuItem);

            menuStrip.Items.Add(fileMenuItem);
            menuStrip.Items.Add(helpMenuItem);

            // En-tête de l'application
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(20, 10, 20, 10)
            };

            labelTitle = new Label
            {
                Text = "INTÉGRATION PRESTAGO PMU",
                Font = HEADER_FONT,
                ForeColor = PRIMARY_COLOR,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Panel pour le code de dépôt et la connexion
            var panelAuthentication = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(20, 10, 20, 10)
            };

            var labelStockOutletCode = new Label
            {
                Text = "Code Dépôt:",
                AutoSize = true,
                Location = new Point(20, 20),
                ForeColor = TEXT_COLOR
            };

            textBoxStockOutletCode = new TextBox
            {
                Location = new Point(120, 17),
                Size = new Size(150, 25),
                BorderStyle = BorderStyle.FixedSingle,
                Font = NORMAL_FONT
            };

            buttonLogin = new Button
            {
                Text = "Se connecter",
                Location = new Point(650, 15),
                Size = new Size(120, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = PRIMARY_COLOR,
                ForeColor = Color.White,
                Font = BUTTON_FONT,
                Cursor = Cursors.Hand
            };
            buttonLogin.FlatAppearance.BorderSize = 0;
            buttonLogin.Click += buttonLogin_Click;

            buttonSettings = new Button
            {
                Text = "⚙️ Paramètres",
                Location = new Point(780, 15),
                Size = new Size(120, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = SECONDARY_COLOR,
                ForeColor = TEXT_COLOR,
                Font = BUTTON_FONT,
                Cursor = Cursors.Hand
            };
            buttonSettings.FlatAppearance.BorderSize = 0;
            buttonSettings.Click += (s, e) => ShowConfigurationForm();

            panelAuthentication.Controls.Add(labelStockOutletCode);
            panelAuthentication.Controls.Add(textBoxStockOutletCode);
            panelAuthentication.Controls.Add(buttonLogin);
            panelAuthentication.Controls.Add(buttonSettings);

            // Barre de statut
            statusStrip = new StatusStrip
            {
                BackColor = Color.White,
                SizingGrip = false
            };

            statusLabel = new ToolStripStatusLabel
            {
                Text = "Non connecté",
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched
            };

            progressBar = new ToolStripProgressBar
            {
                Visible = false,
                Width = 100
            };

            var statusVersion = new ToolStripStatusLabel
            {
                Text = "v1.0.0",
                Alignment = ToolStripItemAlignment.Right
            };

            statusStrip.Items.Add(statusLabel);
            statusStrip.Items.Add(progressBar);
            statusStrip.Items.Add(statusVersion);

            // TabControl pour réception et expédition
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.Normal,
                SizeMode = TabSizeMode.Normal,
                Font = NORMAL_FONT,
                Padding = new Point(15, 5)
            };

            tabPageReception = new TabPage
            {
                Text = "Réception de NSE",
                Padding = new Padding(10),
                BackColor = Color.White
            };

            tabPageExpedition = new TabPage
            {
                Text = "Expédition de NSE",
                Padding = new Padding(10),
                BackColor = Color.White
            };

            tabControl.Controls.Add(tabPageReception);
            tabControl.Controls.Add(tabPageExpedition);

            // Initialiser les onglets
            InitializeReceptionTab();
            InitializeExpeditionTab();

            // Ajouter les contrôles au formulaire
            this.Controls.Add(tabControl);
            this.Controls.Add(panelAuthentication);
            this.Controls.Add(panelHeader);
            this.Controls.Add(statusStrip);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            panelHeader.Controls.Add(labelTitle);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializeReceptionTab()
        {
            // Panel pour les contrôles de saisie
            GroupBox groupBoxInput = new GroupBox
            {
                Text = "Ajout d'un équipement",
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(15),
                Font = new Font(NORMAL_FONT, FontStyle.Bold)
            };

            // Créer une disposition en tableau pour les champs de saisie
            TableLayoutPanel tableControls = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 5,
                Padding = new Padding(0, 10, 0, 0)
            };

            tableControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            for (int i = 0; i < tableControls.RowCount; i++)
            {
                tableControls.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            }

            // Champs du formulaire
            var fields = new[]
            {
                ("Code Équipement:", new TextBox(), "textBoxEquipmentCode"),
                ("Nom Équipement:", new TextBox(), "textBoxEquipmentName"),
                ("Numéro de Série:", new TextBox(), "textBoxSerialNumber"),
                ("État:", new ComboBox(), "comboBoxState"),
                ("N° Intervention:", new TextBox(), "textBoxInterventionNumber")
            };

            int row = 0;
            foreach (var (labelText, control, name) in fields)
            {
                // Configurer le label
                var label = new Label
                {
                    Text = labelText,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font(NORMAL_FONT, FontStyle.Regular)
                };

                // Configurer le contrôle
                control.Dock = DockStyle.Fill;
                control.Name = name;

                if (control is ComboBox comboBox)
                {
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    comboBox.Items.AddRange(new object[] { "AVAILABLE", "INSTALLED" });
                    comboBox.SelectedIndex = 0;
                }

                // Ajouter à la table
                tableControls.Controls.Add(label, 0, row);
                tableControls.Controls.Add(control, 1, row);

                switch (name)
                {
                    case "textBoxEquipmentCode":
                        textBoxEquipmentCode = (TextBox)control;
                        break;
                    case "textBoxEquipmentName":
                        textBoxEquipmentName = (TextBox)control;
                        break;
                    case "textBoxSerialNumber":
                        textBoxSerialNumber = (TextBox)control;
                        break;
                    case "comboBoxState":
                        comboBoxState = (ComboBox)control;
                        break;
                    case "textBoxInterventionNumber":
                        textBoxInterventionNumber = (TextBox)control;
                        break;
                }

                row++;
            }

            // Boutons d'action
            var buttonAddReception = new Button
            {
                Text = "Ajouter équipement",
                Size = new Size(160, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = PRIMARY_COLOR,
                ForeColor = Color.White,
                Font = BUTTON_FONT,
                Cursor = Cursors.Hand
            };
            buttonAddReception.FlatAppearance.BorderSize = 0;
            buttonAddReception.Click += buttonAddReception_Click;

            var buttonSendReception = new Button
            {
                Text = "Envoyer réception",
                Size = new Size(160, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = SUCCESS_COLOR,
                ForeColor = Color.White,
                Font = BUTTON_FONT,
                Cursor = Cursors.Hand
            };
            buttonSendReception.FlatAppearance.BorderSize = 0;
            buttonSendReception.Click += buttonSendReception_Click;

            var buttonClearReception = new Button
            {
                Text = "Vider la liste",
                Size = new Size(160, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = DANGER_COLOR,
                ForeColor = Color.White,
                Font = BUTTON_FONT,
                Cursor = Cursors.Hand
            };
            buttonClearReception.FlatAppearance.BorderSize = 0;
            buttonClearReception.Click += (s, e) =>
            {
                if (MessageBox.Show("Êtes-vous sûr de vouloir vider la liste des équipements ?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _receptionItems.Clear();
                    RefreshReceptionGrid();
                }
            };

            // Mise en page des boutons
            FlowLayoutPanel flowButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            flowButtons.Controls.Add(buttonAddReception);
            flowButtons.Controls.Add(buttonSendReception);
            flowButtons.Controls.Add(buttonClearReception);

            tableControls.Controls.Add(flowButtons, 2, 1);
            tableControls.SetRowSpan(flowButtons, 3);

            groupBoxInput.Controls.Add(tableControls);

            // DataGridView pour la liste des équipements
            GroupBox groupBoxList = new GroupBox
            {
                Text = "Liste des équipements à réceptionner",
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                Font = new Font(NORMAL_FONT, FontStyle.Bold)
            };

            dataGridViewReception = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                MultiSelect = false,
                Font = NORMAL_FONT
            };

            dataGridViewReception.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dataGridViewReception.ColumnHeadersDefaultCellStyle.BackColor = PRIMARY_COLOR;
            dataGridViewReception.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewReception.ColumnHeadersDefaultCellStyle.Font = new Font(NORMAL_FONT, FontStyle.Bold);

            // Ajouter un menu contextuel pour supprimer des équipements
            var contextMenu = new ContextMenuStrip();
            var deleteItem = new ToolStripMenuItem("Supprimer cet équipement");
            deleteItem.Click += (s, e) =>
            {
                if (dataGridViewReception.SelectedRows.Count > 0 &&
                    dataGridViewReception.SelectedRows[0].Index < _receptionItems.Count)
                {
                    _receptionItems.RemoveAt(dataGridViewReception.SelectedRows[0].Index);
                    RefreshReceptionGrid();
                }
            };
            contextMenu.Items.Add(deleteItem);
            dataGridViewReception.ContextMenuStrip = contextMenu;

            groupBoxList.Controls.Add(dataGridViewReception);

            // Ajouter les groupes au tab
            tabPageReception.Controls.Add(groupBoxList);
            tabPageReception.Controls.Add(groupBoxInput);
        }

        private void InitializeExpeditionTab()
        {
            // Structure similaire à l'onglet réception
            GroupBox groupBoxInput = new GroupBox
            {
                Text = "Ajout d'un équipement",
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(15),
                Font = new Font(NORMAL_FONT, FontStyle.Bold)
            };

            TableLayoutPanel tableControls = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 4,
                Padding = new Padding(0, 10, 0, 0)
            };

            tableControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            for (int i = 0; i < tableControls.RowCount; i++)
            {
                tableControls.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            }

            // Champs du formulaire
            var fields = new[]
            {
                ("Code Équipement:", new TextBox(), "textBoxExpEquipmentCode"),
                ("Nom Équipement:", new TextBox(), "textBoxExpEquipmentName"),
                ("Numéro de Série:", new TextBox(), "textBoxExpSerialNumber"),
                ("Code Dépôt Cible:", new TextBox(), "textBoxTargetStockCode")
            };

            int row = 0;
            foreach (var (labelText, control, name) in fields)
            {
                var label = new Label
                {
                    Text = labelText,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font(NORMAL_FONT, FontStyle.Regular)
                };

                control.Dock = DockStyle.Fill;
                control.Name = name;

                tableControls.Controls.Add(label, 0, row);
                tableControls.Controls.Add(control, 1, row);

                switch (name)
                {
                    case "textBoxExpEquipmentCode":
                        textBoxExpEquipmentCode = (TextBox)control;
                        break;
                    case "textBoxExpEquipmentName":
                        textBoxExpEquipmentName = (TextBox)control;
                        break;
                    case "textBoxExpSerialNumber":
                        textBoxExpSerialNumber = (TextBox)control;
                        break;
                    case "textBoxTargetStockCode":
                        textBoxTargetStockCode = (TextBox)control;
                        break;
                }

                row++;
            }

            // Boutons d'action
            var buttonAddExpedition = new Button
            {
                Text = "Ajouter équipement",
                Size = new Size(160, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = PRIMARY_COLOR,
                ForeColor = Color.White,
                Font = BUTTON_FONT,
                Cursor = Cursors.Hand
            };
            buttonAddExpedition.FlatAppearance.BorderSize = 0;
            buttonAddExpedition.Click += buttonAddExpedition_Click;

            var buttonSendExpedition = new Button
            {
                Text = "Envoyer expédition",
                Size = new Size(160, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = SUCCESS_COLOR,
                ForeColor = Color.White,
                Font = BUTTON_FONT,
                Cursor = Cursors.Hand
            };
            buttonSendExpedition.FlatAppearance.BorderSize = 0;
            buttonSendExpedition.Click += buttonSendExpedition_Click;

            var buttonClearExpedition = new Button
            {
                Text = "Vider la liste",
                Size = new Size(160, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = DANGER_COLOR,
                ForeColor = Color.White,
                Font = BUTTON_FONT,
                Cursor = Cursors.Hand
            };
            buttonClearExpedition.FlatAppearance.BorderSize = 0;
            buttonClearExpedition.Click += (s, e) =>
            {
                if (MessageBox.Show("Êtes-vous sûr de vouloir vider la liste des équipements ?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _expeditionItems.Clear();
                    RefreshExpeditionGrid();
                }
            };

            FlowLayoutPanel flowButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            flowButtons.Controls.Add(buttonAddExpedition);
            flowButtons.Controls.Add(buttonSendExpedition);
            flowButtons.Controls.Add(buttonClearExpedition);

            tableControls.Controls.Add(flowButtons, 2, 1);
            tableControls.SetRowSpan(flowButtons, 3);

            groupBoxInput.Controls.Add(tableControls);

            // DataGridView pour la liste des équipements
            GroupBox groupBoxList = new GroupBox
            {
                Text = "Liste des équipements à expédier",
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                Font = new Font(NORMAL_FONT, FontStyle.Bold)
            };

            dataGridViewExpedition = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                MultiSelect = false,
                Font = NORMAL_FONT
            };

            dataGridViewExpedition.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dataGridViewExpedition.ColumnHeadersDefaultCellStyle.BackColor = PRIMARY_COLOR;
            dataGridViewExpedition.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewExpedition.ColumnHeadersDefaultCellStyle.Font = new Font(NORMAL_FONT, FontStyle.Bold);

            // Menu contextuel pour supprimer
            var contextMenu = new ContextMenuStrip();
            var deleteItem = new ToolStripMenuItem("Supprimer cet équipement");
            deleteItem.Click += (s, e) =>
            {
                if (dataGridViewExpedition.SelectedRows.Count > 0 &&
                    dataGridViewExpedition.SelectedRows[0].Index < _expeditionItems.Count)
                {
                    _expeditionItems.RemoveAt(dataGridViewExpedition.SelectedRows[0].Index);
                    RefreshExpeditionGrid();
                }
            };
            contextMenu.Items.Add(deleteItem);
            dataGridViewExpedition.ContextMenuStrip = contextMenu;

            groupBoxList.Controls.Add(dataGridViewExpedition);

            // Ajouter les groupes au tab
            tabPageExpedition.Controls.Add(groupBoxList);
            tabPageExpedition.Controls.Add(groupBoxInput);
        }

        private void LoadConfiguration()
        {
            textBoxStockOutletCode.Text = _config.DefaultStockOutletCode; // Default for Eurodislog
            if (!string.IsNullOrEmpty(_config.Login))
            {
                buttonLogin.Text = "Reconnecter";
            }

            if (!string.IsNullOrEmpty(_config.TargetStockCode))
            {
                textBoxTargetStockCode.Text = _config.TargetStockCode;
                _targetStockCode = _config.TargetStockCode;
            }
        }

        private void SetupDataGridViews()
        {
            // Setup DataGridView for reception items
            var bindingSource1 = new BindingSource();
            bindingSource1.DataSource = _receptionItems;
            dataGridViewReception.DataSource = bindingSource1;

            // Setup DataGridView for expedition items
            var bindingSource2 = new BindingSource();
            bindingSource2.DataSource = _expeditionItems;
            dataGridViewExpedition.DataSource = bindingSource2;

            // Personnaliser l'affichage
            CustomizeDataGridView(dataGridViewReception);
            CustomizeDataGridView(dataGridViewExpedition);
        }

        private void CustomizeDataGridView(DataGridView dgv)
        {
            if (dgv.Columns.Count > 0)
            {
                // Renommer les colonnes avec des noms plus lisibles
                var columnNames = new Dictionary<string, string>
                {
                    { "EquipmentCode", "Code équipement" },
                    { "EquipmentName", "Nom équipement" },
                    { "SerialNumber", "Numéro de série" },
                    { "State", "État" },
                    { "InterventionNumber", "N° Intervention" }
                };

                foreach (DataGridViewColumn column in dgv.Columns)
                {
                    if (columnNames.ContainsKey(column.DataPropertyName))
                    {
                        column.HeaderText = columnNames[column.DataPropertyName];
                    }
                }

                // Ajouter une tooltip pour la suppression
                dgv.CellToolTipText = "Clic-droit pour supprimer";
            }
        }

        private async void buttonLogin_Click(object sender, EventArgs e)
        {
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        SetBusy(true, "Connexion en cours...");

                        // Mettre à jour la configuration
                        _config.Login = loginForm.Username;
                        _config.Password = loginForm.Password;
                        AppConfig.SaveConfig(_config);

                        // Créer un nouveau service avec ces identifiants
                        _prestagoService = new PrestagoService(
                            _config.ApiUrl,
                            loginForm.Username,
                            loginForm.Password);

                        bool success = await _prestagoService.AuthenticateAsync();
                        if (success)
                        {
                            MessageBox.Show("Connexion réussie!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            statusLabel.Text = "Connecté";
                            statusLabel.ForeColor = SUCCESS_COLOR;
                            buttonLogin.Text = "Reconnecter";
                        }
                        else
                        {
                            MessageBox.Show("Échec de la connexion. Vérifiez vos identifiants.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            statusLabel.Text = "Non connecté";
                            statusLabel.ForeColor = DANGER_COLOR;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Logger.LogError("Login", ex);
                        statusLabel.Text = "Erreur de connexion";
                        statusLabel.ForeColor = DANGER_COLOR;
                    }
                    finally
                    {
                        SetBusy(false);
                    }
                }
            }
        }

        private async void TestConnectionAsync()
        {
            try
            {
                SetBusy(true, "Test de connexion...");
                bool success = await _prestagoService.AuthenticateAsync();
                if (success)
                {
                    statusLabel.Text = "Connecté";
                    statusLabel.ForeColor = SUCCESS_COLOR;
                }
                else
                {
                    statusLabel.Text = "Non connecté";
                    statusLabel.ForeColor = DANGER_COLOR;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Test de connexion automatique", ex);
                statusLabel.Text = "Erreur de connexion";
                statusLabel.ForeColor = DANGER_COLOR;
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void buttonAddReception_Click(object sender, EventArgs e)
        {
            // Validation des entrées
            if (string.IsNullOrWhiteSpace(textBoxEquipmentCode.Text) ||
                string.IsNullOrWhiteSpace(textBoxEquipmentName.Text) ||
                string.IsNullOrWhiteSpace(textBoxSerialNumber.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Vérifier si le NSE existe déjà
            if (_receptionItems.Any(x => x.SerialNumber == textBoxSerialNumber.Text))
            {
                MessageBox.Show("Ce numéro de série existe déjà dans la liste.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Create new item
            var item = new ReceptionItem
            {
                EquipmentCode = textBoxEquipmentCode.Text,
                EquipmentName = textBoxEquipmentName.Text,
                SerialNumber = textBoxSerialNumber.Text,
                State = comboBoxState.Text
            };

            // Try to parse intervention number if provided
            if (!string.IsNullOrEmpty(textBoxInterventionNumber.Text))
            {
                if (int.TryParse(textBoxInterventionNumber.Text, out int interventionNumber))
                {
                    item.InterventionNumber = interventionNumber;
                }
                else
                {
                    MessageBox.Show("Le numéro d'intervention doit être un nombre entier.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Add to list
            _receptionItems.Add(item);

            // Refresh grid
            RefreshReceptionGrid();

            // Clear fields
            ClearReceptionFields();

            // Focus sur le premier champ pour faciliter la saisie
            textBoxEquipmentCode.Focus();

            // Notification
            statusLabel.Text = $"Équipement ajouté : {item.SerialNumber}";
            Logger.Log($"NSE ajouté pour réception: {item.SerialNumber}");
        }

        private async void buttonSendReception_Click(object sender, EventArgs e)
        {
            if (_receptionItems.Count == 0)
            {
                MessageBox.Show("Aucun équipement à envoyer.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmation
            if (MessageBox.Show($"Êtes-vous sûr de vouloir envoyer {_receptionItems.Count} équipement(s) en réception ?",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SetBusy(true, "Envoi en cours...");
                bool success = await _prestagoService.SendReceptionAsync(textBoxStockOutletCode.Text, _receptionItems);

                if (success)
                {
                    MessageBox.Show("Réception envoyée avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _receptionItems.Clear();
                    RefreshReceptionGrid();
                    Logger.Log($"Réception envoyée avec succès - {_receptionItems.Count} équipements");
                    statusLabel.Text = "Réception envoyée avec succès";
                    statusLabel.ForeColor = SUCCESS_COLOR;
                }
                else
                {
                    MessageBox.Show("Échec de l'envoi de la réception.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Logger.Log("Échec de l'envoi de la réception");
                    statusLabel.Text = "Échec de l'envoi de la réception";
                    statusLabel.ForeColor = DANGER_COLOR;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("SendReception", ex);
                statusLabel.Text = "Erreur lors de l'envoi";
                statusLabel.ForeColor = DANGER_COLOR;
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void RefreshReceptionGrid()
        {
            ((BindingSource)dataGridViewReception.DataSource).ResetBindings(false);
            CustomizeDataGridView(dataGridViewReception);
        }

        private void ClearReceptionFields()
        {
            textBoxEquipmentCode.Text = "";
            textBoxEquipmentName.Text = "";
            textBoxSerialNumber.Text = "";
            textBoxInterventionNumber.Text = "";
            comboBoxState.SelectedIndex = 0; // Reset to "AVAILABLE"
        }

        private void buttonAddExpedition_Click(object sender, EventArgs e)
        {
            // Validation des entrées
            if (string.IsNullOrWhiteSpace(textBoxExpEquipmentCode.Text) ||
                string.IsNullOrWhiteSpace(textBoxExpEquipmentName.Text) ||
                string.IsNullOrWhiteSpace(textBoxExpSerialNumber.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Vérifier si le NSE existe déjà
            if (_expeditionItems.Any(x => x.SerialNumber == textBoxExpSerialNumber.Text))
            {
                MessageBox.Show("Ce numéro de série existe déjà dans la liste.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Vérifier si le dépôt cible est spécifié
            if (string.IsNullOrWhiteSpace(textBoxTargetStockCode.Text))
            {
                MessageBox.Show("Veuillez spécifier un code de dépôt cible.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save target stock code
            _targetStockCode = textBoxTargetStockCode.Text;

            // Sauvegarder dans la configuration
            _config.TargetStockCode = _targetStockCode;
            AppConfig.SaveConfig(_config);

            // Create new item
            var item = new ExpeditionItem
            {
                EquipmentCode = textBoxExpEquipmentCode.Text,
                EquipmentName = textBoxExpEquipmentName.Text,
                SerialNumber = textBoxExpSerialNumber.Text
            };

            // Add to list
            _expeditionItems.Add(item);

            // Refresh grid
            RefreshExpeditionGrid();

            // Clear fields except target stock code
            ClearExpeditionFields();

            // Focus sur le premier champ pour faciliter la saisie
            textBoxExpEquipmentCode.Focus();

            // Notification
            statusLabel.Text = $"Équipement ajouté : {item.SerialNumber}";
            Logger.Log($"NSE ajouté pour expédition: {item.SerialNumber}");
        }

        private async void buttonSendExpedition_Click(object sender, EventArgs e)
        {
            if (_expeditionItems.Count == 0)
            {
                MessageBox.Show("Aucun équipement à expédier.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_targetStockCode))
            {
                MessageBox.Show("Veuillez spécifier un code de dépôt cible.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmation
            if (MessageBox.Show($"Êtes-vous sûr de vouloir expédier {_expeditionItems.Count} équipement(s) vers le dépôt {_targetStockCode} ?",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SetBusy(true, "Envoi en cours...");

                var expeditionRequest = new ExpeditionRequest
                {
                    StockEquipments = _expeditionItems,
                    TargetStockCode = _targetStockCode
                };

                bool success = await _prestagoService.SendExpeditionAsync(textBoxStockOutletCode.Text, expeditionRequest);

                if (success)
                {
                    MessageBox.Show("Expédition envoyée avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _expeditionItems.Clear();
                    RefreshExpeditionGrid();
                    Logger.Log($"Expédition envoyée avec succès - {expeditionRequest.StockEquipments.Count} équipements");
                    statusLabel.Text = "Expédition envoyée avec succès";
                    statusLabel.ForeColor = SUCCESS_COLOR;
                }
                else
                {
                    MessageBox.Show("Échec de l'envoi de l'expédition.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Logger.Log("Échec de l'envoi de l'expédition");
                    statusLabel.Text = "Échec de l'envoi de l'expédition";
                    statusLabel.ForeColor = DANGER_COLOR;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("SendExpedition", ex);
                statusLabel.Text = "Erreur lors de l'envoi";
                statusLabel.ForeColor = DANGER_COLOR;
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void RefreshExpeditionGrid()
        {
            ((BindingSource)dataGridViewExpedition.DataSource).ResetBindings(false);
            CustomizeDataGridView(dataGridViewExpedition);
        }

        private void ClearExpeditionFields()
        {
            textBoxExpEquipmentCode.Text = "";
            textBoxExpEquipmentName.Text = "";
            textBoxExpSerialNumber.Text = "";
            // Ne pas effacer le code de dépôt cible
        }

        // Méthodes d'utilitaires d'UI
        private void SetBusy(bool isBusy, string statusText = null)
        {
            // Mettre à jour le statut
            if (statusText != null)
            {
                statusLabel.Text = statusText;
                statusLabel.ForeColor = TEXT_COLOR;
            }

            // Afficher/masquer la barre de progression
            progressBar.Visible = isBusy;
            if (isBusy)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
            }

            // Changer le curseur
            this.UseWaitCursor = isBusy;

            // Désactiver/activer les contrôles principaux
            buttonLogin.Enabled = !isBusy;
            buttonSettings.Enabled = !isBusy;
            tabControl.Enabled = !isBusy;

            // Forcer la mise à jour de l'interface
            Application.DoEvents();
        }

        private void ShowConfigurationForm()
        {
            using (var configForm = new ConfigurationForm(_config))
            {
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    // Réinitialiser le service avec les nouvelles configurations
                    _prestagoService = new PrestagoService(
                        _config.ApiUrl,
                        _config.Login,
                        _config.Password);

                    // Mettre à jour l'interface
                    textBoxStockOutletCode.Text = _config.DefaultStockOutletCode;
                    TestConnectionAsync();
                }
            }
        }

        private void ShowAboutDialog()
        {
            MessageBox.Show(
                "PMU Prestago Integration\n" +
                "Version 1.0.0\n\n" +
                "Cette application permet l'intégration avec le système Prestago de PMU pour gérer les réceptions et expéditions d'équipements.\n\n" +
                "© 2024 BDEQUEKER.\n" +
                "Tous droits réservés.",
                "À propos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}