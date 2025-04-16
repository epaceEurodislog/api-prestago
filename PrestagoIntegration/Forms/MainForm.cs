
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;
using System.Threading.Tasks;
using PrestagoIntegration.Models;
using PrestagoIntegration.Services;
using PrestagoIntegration.Utils;

namespace PrestagoIntegration.Forms
{
    public partial class MainForm : Form
    {
        private readonly AppConfig _config;
        private readonly AuthService _authService;
        private readonly ReceptionService _receptionService;
        private readonly List<NSEItem> _nseItems = new List<NSEItem>();

        // Controls
        private MenuStrip menuStrip;
        private ToolStripMenuItem menuItemFile;
        private ToolStripMenuItem menuItemConfiguration;
        private ToolStripMenuItem menuItemExit;
        private TextBox textBoxStockOutletCode;
        private TextBox textBoxEquipmentCode;
        private TextBox textBoxEquipmentName;
        private TextBox textBoxSerialNumber;
        private TextBox textBoxStatus;
        private TextBox textBoxInterventionNumber;
        private Button buttonAddNSE;
        private Button buttonSendReception;
        private Button buttonClear;
        private Button buttonTestConnection;
        private DataGridView dataGridViewNSEs;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel;

        public MainForm()
        {
            // Chargement de la configuration
            _config = AppConfig.Instance;

            // Initialisation des services
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_config.ApiUrl)
            };
            _authService = new AuthService(httpClient, _config);
            _receptionService = new ReceptionService(httpClient, _authService, _config);

            InitializeComponent();
            LoadInitialData();
        }

        private void InitializeComponent()
        {
            this.Text = "Prestago Integration - Réception NSE";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Menu Strip
            menuStrip = new MenuStrip();
            menuItemFile = new ToolStripMenuItem("Fichier");
            menuItemConfiguration = new ToolStripMenuItem("Configuration");
            menuItemExit = new ToolStripMenuItem("Quitter");

            menuItemConfiguration.Click += MenuItemConfiguration_Click;
            menuItemExit.Click += MenuItemExit_Click;

            menuItemFile.DropDownItems.Add(menuItemConfiguration);
            menuItemFile.DropDownItems.Add(new ToolStripSeparator());
            menuItemFile.DropDownItems.Add(menuItemExit);

            menuStrip.Items.Add(menuItemFile);

            // Status Strip
            statusStrip = new StatusStrip();
            toolStripStatusLabel = new ToolStripStatusLabel("Prêt");
            statusStrip.Items.Add(toolStripStatusLabel);

            // Panel for Stock Outlet Code
            var panelStockOutlet = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };

            var labelStockOutletCode = new Label
            {
                Text = "Code Dépôt:",
                Location = new Point(10, 15),
                Size = new Size(100, 25)
            };

            textBoxStockOutletCode = new TextBox
            {
                Location = new Point(120, 15),
                Size = new Size(150, 25)
            };

            buttonTestConnection = new Button
            {
                Text = "Tester Connexion",
                Location = new Point(290, 14),
                Size = new Size(130, 30)
            };
            buttonTestConnection.Click += ButtonTestConnection_Click;

            panelStockOutlet.Controls.Add(labelStockOutletCode);
            panelStockOutlet.Controls.Add(textBoxStockOutletCode);
            panelStockOutlet.Controls.Add(buttonTestConnection);

            // Panel for NSE Input
            var panelNseInput = new Panel
            {
                Dock = DockStyle.Top,
                Height = 210,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            var labelNseInput = new Label
            {
                Text = "Ajouter un NSE:",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(200, 25)
            };

            var labelEquipmentCode = new Label
            {
                Text = "Code Équipement:",
                Location = new Point(10, 45),
                Size = new Size(150, 25)
            };

            textBoxEquipmentCode = new TextBox
            {
                Location = new Point(160, 45),
                Size = new Size(250, 25)
            };

            var labelEquipmentName = new Label
            {
                Text = "Nom Équipement:",
                Location = new Point(10, 75),
                Size = new Size(150, 25)
            };

            textBoxEquipmentName = new TextBox
            {
                Location = new Point(160, 75),
                Size = new Size(250, 25)
            };

            var labelSerialNumber = new Label
            {
                Text = "Numéro de Série:",
                Location = new Point(10, 105),
                Size = new Size(150, 25)
            };

            textBoxSerialNumber = new TextBox
            {
                Location = new Point(160, 105),
                Size = new Size(250, 25)
            };

            var labelStatus = new Label
            {
                Text = "Status (AVAILABLE):",
                Location = new Point(10, 135),
                Size = new Size(150, 25)
            };

            textBoxStatus = new TextBox
            {
                Location = new Point(160, 135),
                Size = new Size(250, 25),
                Text = "AVAILABLE"
            };

            var labelInterventionNumber = new Label
            {
                Text = "N° Intervention:",
                Location = new Point(10, 165),
                Size = new Size(150, 25)
            };

            textBoxInterventionNumber = new TextBox
            {
                Location = new Point(160, 165),
                Size = new Size(250, 25)
            };

            buttonAddNSE = new Button
            {
                Text = "Ajouter NSE",
                Location = new Point(430, 105),
                Size = new Size(150, 40)
            };
            buttonAddNSE.Click += ButtonAddNSE_Click;

            panelNseInput.Controls.Add(labelNseInput);
            panelNseInput.Controls.Add(labelEquipmentCode);
            panelNseInput.Controls.Add(textBoxEquipmentCode);
            panelNseInput.Controls.Add(labelEquipmentName);
            panelNseInput.Controls.Add(textBoxEquipmentName);
            panelNseInput.Controls.Add(labelSerialNumber);
            panelNseInput.Controls.Add(textBoxSerialNumber);
            panelNseInput.Controls.Add(labelStatus);
            panelNseInput.Controls.Add(textBoxStatus);
            panelNseInput.Controls.Add(labelInterventionNumber);
            panelNseInput.Controls.Add(textBoxInterventionNumber);
            panelNseInput.Controls.Add(buttonAddNSE);

            // Panel for NSE List
            var panelNseList = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var labelNseList = new Label
            {
                Text = "Liste des NSE à envoyer:",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25
            };

            dataGridViewNSEs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };

            // Panel for buttons
            var panelButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10)
            };

            buttonSendReception = new Button
            {
                Text = "Envoyer Réception",
                Size = new Size(200, 40),
                Location = new Point(570, 10)
            };
            buttonSendReception.Click += ButtonSendReception_Click;

            buttonClear = new Button
            {
                Text = "Effacer Liste",
                Size = new Size(150, 40),
                Location = new Point(400, 10)
            };
            buttonClear.Click += ButtonClear_Click;

            panelButtons.Controls.Add(buttonSendReception);
            panelButtons.Controls.Add(buttonClear);

            panelNseList.Controls.Add(dataGridViewNSEs);
            panelNseList.Controls.Add(labelNseList);

            // Add all controls to form
            this.Controls.Add(panelNseList);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelNseInput);
            this.Controls.Add(panelStockOutlet);
            this.Controls.Add(statusStrip);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
        }

        private void LoadInitialData()
        {
            // Initialize NSE List
            RefreshNSEList();

            // Set default values
            textBoxStockOutletCode.Text = _config.DefaultStockOutletCode;
            textBoxStatus.Text = "AVAILABLE";

            // Check if configuration is missing
            if (string.IsNullOrEmpty(_config.Login) || string.IsNullOrEmpty(_config.Password))
            {
                MessageBox.Show(
                    "La configuration n'est pas complète. Veuillez configurer vos identifiants Prestago.",
                    "Configuration requise",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                OpenConfigurationForm();
            }
        }

        private void RefreshNSEList()
        {
            // Créer une source de données pour le DataGridView
            var bindingSource = new BindingSource();
            bindingSource.DataSource = _nseItems;
            dataGridViewNSEs.DataSource = bindingSource;

            // Mettre à jour l'état des boutons
            buttonSendReception.Enabled = _nseItems.Count > 0;
            buttonClear.Enabled = _nseItems.Count > 0;

            // Mettre à jour le statut
            toolStripStatusLabel.Text = $"Prêt - {_nseItems.Count} NSE(s) en attente d'envoi";
        }

        private void ButtonAddNSE_Click(object sender, EventArgs e)
        {
            // Validation des entrées
            if (string.IsNullOrWhiteSpace(textBoxEquipmentCode.Text) ||
                string.IsNullOrWhiteSpace(textBoxEquipmentName.Text) ||
                string.IsNullOrWhiteSpace(textBoxSerialNumber.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Créer un nouvel NSE
            var nseItem = new NSEItem
            {
                StockOutletCode = textBoxStockOutletCode.Text,
                EquipmentCode = textBoxEquipmentCode.Text,
                EquipmentName = textBoxEquipmentName.Text,
                SerialNumber = textBoxSerialNumber.Text,
                Status = string.IsNullOrWhiteSpace(textBoxStatus.Text) ? "AVAILABLE" : textBoxStatus.Text,
                InterventionNumber = textBoxInterventionNumber.Text ?? ""
            };

            // Vérifier si le NSE existe déjà
            bool serialExists = _nseItems.Exists(item => item.SerialNumber == nseItem.SerialNumber);
            if (serialExists)
            {
                MessageBox.Show("Ce numéro de série existe déjà dans la liste.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ajouter à la liste
            _nseItems.Add(nseItem);
            RefreshNSEList();

            // Réinitialiser les champs
            textBoxEquipmentCode.Text = "";
            textBoxEquipmentName.Text = "";
            textBoxSerialNumber.Text = "";
            textBoxInterventionNumber.Text = "";
            textBoxEquipmentCode.Focus();

            Logger.Log($"NSE ajouté: {nseItem.SerialNumber}");
        }

        private async void ButtonSendReception_Click(object sender, EventArgs e)
        {
            if (_nseItems.Count == 0)
            {
                MessageBox.Show("Veuillez ajouter au moins un NSE avant d'envoyer.", "Liste vide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmation
            DialogResult result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir envoyer {_nseItems.Count} NSE(s) au dépôt {textBoxStockOutletCode.Text}?",
                "Confirmation d'envoi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            // Désactiver les contrôles pendant l'envoi
            SetControlsEnabled(false);
            toolStripStatusLabel.Text = "Envoi en cours...";

            try
            {
                // Si le code dépôt a changé, mettre à jour tous les NSE
                string stockOutletCode = textBoxStockOutletCode.Text;
                foreach (var nse in _nseItems)
                {
                    nse.StockOutletCode = stockOutletCode;
                }

                // Envoi de la réception
                bool success = await _receptionService.SendReceptionAsync(stockOutletCode, _nseItems);

                if (success)
                {
                    MessageBox.Show("Réception envoyée avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _nseItems.Clear();
                    RefreshNSEList();
                    Logger.Log($"Réception réussie - {stockOutletCode}");
                }
                else
                {
                    MessageBox.Show(
                        "Échec de l'envoi de la réception. Consultez les logs pour plus de détails.",
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    Logger.Log("Échec de la réception");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("Envoi réception", ex);
            }
            finally
            {
                // Réactiver les contrôles
                SetControlsEnabled(true);
                toolStripStatusLabel.Text = "Prêt";
            }
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            if (_nseItems.Count == 0)
                return;

            DialogResult result = MessageBox.Show(
                "Êtes-vous sûr de vouloir effacer tous les NSE de la liste?",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                _nseItems.Clear();
                RefreshNSEList();
                Logger.Log("Liste NSE effacée");
            }
        }

        private async void ButtonTestConnection_Click(object sender, EventArgs e)
        {
            SetControlsEnabled(false);
            toolStripStatusLabel.Text = "Test de connexion en cours...";

            try
            {
                bool result = await _authService.AuthenticateAsync();

                if (result)
                {
                    MessageBox.Show(
                        "Connexion à l'API Prestago réussie!",
                        "Succès",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    toolStripStatusLabel.Text = "Connexion réussie";
                    Logger.Log("Test de connexion réussi");
                }
                else
                {
                    MessageBox.Show(
                        "Échec de la connexion à l'API Prestago. Vérifiez vos identifiants et la configuration.",
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    toolStripStatusLabel.Text = "Échec de connexion";
                    Logger.Log("Échec du test de connexion");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur de connexion : {ex.Message}",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Logger.LogError("Test connexion", ex);
                toolStripStatusLabel.Text = "Erreur de connexion";
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private void MenuItemConfiguration_Click(object sender, EventArgs e)
        {
            OpenConfigurationForm();
        }

        private void OpenConfigurationForm()
        {
            var configForm = new ConfigurationForm(_config);
            if (configForm.ShowDialog() == DialogResult.OK)
            {
                // Rafraîchir les valeurs si nécessaire
                textBoxStockOutletCode.Text = _config.DefaultStockOutletCode;
            }
        }

        private void MenuItemExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SetControlsEnabled(bool enabled)
        {
            buttonAddNSE.Enabled = enabled;
            buttonSendReception.Enabled = enabled && _nseItems.Count > 0;
            buttonClear.Enabled = enabled && _nseItems.Count > 0;
            buttonTestConnection.Enabled = enabled;
            textBoxEquipmentCode.Enabled = enabled;
            textBoxEquipmentName.Enabled = enabled;
            textBoxSerialNumber.Enabled = enabled;
            textBoxStatus.Enabled = enabled;
            textBoxInterventionNumber.Enabled = enabled;
            textBoxStockOutletCode.Enabled = enabled;
            menuItemConfiguration.Enabled = enabled;
        }
    }
}