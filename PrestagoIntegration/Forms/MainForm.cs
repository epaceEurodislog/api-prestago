using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using PrestagoIntegration.Models;
using PrestagoIntegration.Services;
using PrestagoIntegration.Utils;
using PrestagoIntegration.Forms;

namespace PrestagoIntegration
{
    public partial class MainForm : Form
    {
        private PrestagoApiService _apiService;
        private List<ReceptionItem> _receptionItems = new List<ReceptionItem>();
        private List<ExpeditionItem> _expeditionItems = new List<ExpeditionItem>();
        private string _targetStockCode = "";

        // UI Controls
        private TabControl tabControl;
        private TabPage tabPageReception;
        private TabPage tabPageExpedition;
        private TextBox textBoxStockOutletCode;
        private Button buttonLogin;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;

        // Reception tab controls
        private TextBox textBoxEquipmentCode;
        private TextBox textBoxEquipmentName;
        private TextBox textBoxSerialNumber;
        private TextBox textBoxInterventionNumber;
        private ComboBox comboBoxState;
        private Button buttonAddReception;
        private Button buttonSendReception;
        private DataGridView dataGridViewReception;

        // Expedition tab controls
        private TextBox textBoxExpEquipmentCode;
        private TextBox textBoxExpEquipmentName;
        private TextBox textBoxExpSerialNumber;
        private TextBox textBoxTargetStockCode;
        private Button buttonAddExpedition;
        private Button buttonSendExpedition;
        private DataGridView dataGridViewExpedition;

        public MainForm()
        {
            InitializeComponent();

            // Initialize API service with default URL
            _apiService = new PrestagoApiService("https://prestago-test.pmu.fr/", "", "");

            // Load configuration and setup UI
            LoadConfiguration();
            SetupDataGridViews();
        }

        private void InitializeComponent()
        {
            this.tabControl = new TabControl();
            this.tabPageReception = new TabPage();
            this.tabPageExpedition = new TabPage();
            this.statusStrip = new StatusStrip();
            this.statusLabel = new ToolStripStatusLabel();

            // Main form settings
            this.Text = "Intégration Prestago";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Tab Control
            this.tabControl.Dock = DockStyle.Fill;
            this.tabControl.Controls.Add(this.tabPageReception);
            this.tabControl.Controls.Add(this.tabPageExpedition);

            // Reception Tab
            this.tabPageReception.Text = "Réception";
            InitializeReceptionTab();

            // Expedition Tab
            this.tabPageExpedition.Text = "Expédition";
            InitializeExpeditionTab();

            // Status Strip
            this.statusStrip.Items.Add(this.statusLabel);
            this.statusLabel.Text = "Non connecté";

            // Stock Outlet Code and Login button (common)
            Panel panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10)
            };

            Label labelStockOutletCode = new Label
            {
                Text = "Code Dépôt:",
                Location = new Point(10, 15),
                Size = new Size(80, 20)
            };

            textBoxStockOutletCode = new TextBox
            {
                Location = new Point(100, 12),
                Size = new Size(150, 25)
            };

            buttonLogin = new Button
            {
                Text = "Connexion",
                Location = new Point(650, 10),
                Size = new Size(100, 30)
            };
            buttonLogin.Click += new EventHandler(buttonLogin_Click);

            panelTop.Controls.Add(labelStockOutletCode);
            panelTop.Controls.Add(textBoxStockOutletCode);
            panelTop.Controls.Add(buttonLogin);

            // Add all controls to the form
            this.Controls.Add(this.tabControl);
            this.Controls.Add(panelTop);
            this.Controls.Add(this.statusStrip);
        }

        private void InitializeReceptionTab()
        {
            // Input Panel
            Panel panelInput = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(10)
            };

            // Controls for reception
            Label labelEquipmentCode = new Label
            {
                Text = "Code Équipement:",
                Location = new Point(10, 20),
                Size = new Size(120, 20)
            };

            textBoxEquipmentCode = new TextBox
            {
                Location = new Point(140, 17),
                Size = new Size(200, 25)
            };

            Label labelEquipmentName = new Label
            {
                Text = "Nom Équipement:",
                Location = new Point(10, 50),
                Size = new Size(120, 20)
            };

            textBoxEquipmentName = new TextBox
            {
                Location = new Point(140, 47),
                Size = new Size(200, 25)
            };

            Label labelSerialNumber = new Label
            {
                Text = "Numéro de Série:",
                Location = new Point(10, 80),
                Size = new Size(120, 20)
            };

            textBoxSerialNumber = new TextBox
            {
                Location = new Point(140, 77),
                Size = new Size(200, 25)
            };

            Label labelState = new Label
            {
                Text = "État:",
                Location = new Point(10, 110),
                Size = new Size(120, 20)
            };

            comboBoxState = new ComboBox
            {
                Location = new Point(140, 107),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboBoxState.Items.AddRange(new string[] { "AVAILABLE", "INSTALLED" });
            comboBoxState.SelectedIndex = 0;

            Label labelInterventionNumber = new Label
            {
                Text = "N° Intervention:",
                Location = new Point(10, 140),
                Size = new Size(120, 20)
            };

            textBoxInterventionNumber = new TextBox
            {
                Location = new Point(140, 137),
                Size = new Size(200, 25)
            };

            buttonAddReception = new Button
            {
                Text = "Ajouter",
                Location = new Point(400, 60),
                Size = new Size(120, 40)
            };
            buttonAddReception.Click += new EventHandler(buttonAddReception_Click);

            buttonSendReception = new Button
            {
                Text = "Envoyer Réception",
                Location = new Point(550, 60),
                Size = new Size(150, 40)
            };
            buttonSendReception.Click += new EventHandler(buttonSendReception_Click);

            // Add controls to panel
            panelInput.Controls.Add(labelEquipmentCode);
            panelInput.Controls.Add(textBoxEquipmentCode);
            panelInput.Controls.Add(labelEquipmentName);
            panelInput.Controls.Add(textBoxEquipmentName);
            panelInput.Controls.Add(labelSerialNumber);
            panelInput.Controls.Add(textBoxSerialNumber);
            panelInput.Controls.Add(labelState);
            panelInput.Controls.Add(comboBoxState);
            panelInput.Controls.Add(labelInterventionNumber);
            panelInput.Controls.Add(textBoxInterventionNumber);
            panelInput.Controls.Add(buttonAddReception);
            panelInput.Controls.Add(buttonSendReception);

            // DataGridView
            dataGridViewReception = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Add to tab page
            this.tabPageReception.Controls.Add(dataGridViewReception);
            this.tabPageReception.Controls.Add(panelInput);
        }

        private void InitializeExpeditionTab()
        {
            // Input Panel
            Panel panelInput = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(10)
            };

            // Controls for expedition
            Label labelExpEquipmentCode = new Label
            {
                Text = "Code Équipement:",
                Location = new Point(10, 20),
                Size = new Size(120, 20)
            };

            textBoxExpEquipmentCode = new TextBox
            {
                Location = new Point(140, 17),
                Size = new Size(200, 25)
            };

            Label labelExpEquipmentName = new Label
            {
                Text = "Nom Équipement:",
                Location = new Point(10, 50),
                Size = new Size(120, 20)
            };

            textBoxExpEquipmentName = new TextBox
            {
                Location = new Point(140, 47),
                Size = new Size(200, 25)
            };

            Label labelExpSerialNumber = new Label
            {
                Text = "Numéro de Série:",
                Location = new Point(10, 80),
                Size = new Size(120, 20)
            };

            textBoxExpSerialNumber = new TextBox
            {
                Location = new Point(140, 77),
                Size = new Size(200, 25)
            };

            Label labelTargetStockCode = new Label
            {
                Text = "Code Dépôt Cible:",
                Location = new Point(10, 110),
                Size = new Size(120, 20)
            };

            textBoxTargetStockCode = new TextBox
            {
                Location = new Point(140, 107),
                Size = new Size(200, 25)
            };

            buttonAddExpedition = new Button
            {
                Text = "Ajouter",
                Location = new Point(400, 60),
                Size = new Size(120, 40)
            };
            buttonAddExpedition.Click += new EventHandler(buttonAddExpedition_Click);

            buttonSendExpedition = new Button
            {
                Text = "Envoyer Expédition",
                Location = new Point(550, 60),
                Size = new Size(150, 40)
            };
            buttonSendExpedition.Click += new EventHandler(buttonSendExpedition_Click);

            // Add controls to panel
            panelInput.Controls.Add(labelExpEquipmentCode);
            panelInput.Controls.Add(textBoxExpEquipmentCode);
            panelInput.Controls.Add(labelExpEquipmentName);
            panelInput.Controls.Add(textBoxExpEquipmentName);
            panelInput.Controls.Add(labelExpSerialNumber);
            panelInput.Controls.Add(textBoxExpSerialNumber);
            panelInput.Controls.Add(labelTargetStockCode);
            panelInput.Controls.Add(textBoxTargetStockCode);
            panelInput.Controls.Add(buttonAddExpedition);
            panelInput.Controls.Add(buttonSendExpedition);

            // DataGridView
            dataGridViewExpedition = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Add to tab page
            this.tabPageExpedition.Controls.Add(dataGridViewExpedition);
            this.tabPageExpedition.Controls.Add(panelInput);
        }

        private void LoadConfiguration()
        {
            // Load configuration from settings or ask user
            textBoxStockOutletCode.Text = "600048"; // Default for Eurodislog
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
        }

        private async void buttonLogin_Click(object sender, EventArgs e)
        {
            // Show login dialog and authenticate
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        bool success = await _apiService.AuthenticateAsync(loginForm.Username, loginForm.Password);
                        if (success)
                        {
                            MessageBox.Show("Connexion réussie!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            statusLabel.Text = "Connecté";
                        }
                        else
                        {
                            MessageBox.Show("Échec de la connexion. Vérifiez vos identifiants.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Logger.LogError("Login", ex);
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void buttonAddReception_Click(object sender, EventArgs e)
        {
            // Validate input fields
            if (string.IsNullOrWhiteSpace(textBoxEquipmentCode.Text) ||
                string.IsNullOrWhiteSpace(textBoxEquipmentName.Text) ||
                string.IsNullOrWhiteSpace(textBoxSerialNumber.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            Logger.Log($"NSE ajouté pour réception: {item.SerialNumber}");
        }

        private async void buttonSendReception_Click(object sender, EventArgs e)
        {
            if (_receptionItems.Count == 0)
            {
                MessageBox.Show("Aucun équipement à envoyer.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                bool success = await _apiService.SendReceptionAsync(textBoxStockOutletCode.Text, _receptionItems);

                if (success)
                {
                    MessageBox.Show("Réception envoyée avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _receptionItems.Clear();
                    RefreshReceptionGrid();
                    Logger.Log($"Réception envoyée avec succès - {_receptionItems.Count} équipements");
                }
                else
                {
                    MessageBox.Show("Échec de l'envoi de la réception.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Logger.Log("Échec de l'envoi de la réception");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("SendReception", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        private void RefreshReceptionGrid()
        {
            ((BindingSource)dataGridViewReception.DataSource).ResetBindings(false);
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
            // Validate input fields
            if (string.IsNullOrWhiteSpace(textBoxExpEquipmentCode.Text) ||
                string.IsNullOrWhiteSpace(textBoxExpEquipmentName.Text) ||
                string.IsNullOrWhiteSpace(textBoxExpSerialNumber.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save target stock code
            if (!string.IsNullOrWhiteSpace(textBoxTargetStockCode.Text))
            {
                _targetStockCode = textBoxTargetStockCode.Text;
            }

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

            // Clear fields
            ClearExpeditionFields();

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

            try
            {
                Cursor = Cursors.WaitCursor;

                var expeditionRequest = new ExpeditionRequest
                {
                    StockEquipments = _expeditionItems,
                    TargetStockCode = _targetStockCode
                };

                bool success = await _apiService.SendExpeditionAsync(textBoxStockOutletCode.Text, expeditionRequest);

                if (success)
                {
                    MessageBox.Show("Expédition envoyée avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _expeditionItems.Clear();
                    RefreshExpeditionGrid();
                    Logger.Log($"Expédition envoyée avec succès - {expeditionRequest.StockEquipments.Count} équipements");
                }
                else
                {
                    MessageBox.Show("Échec de l'envoi de l'expédition.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Logger.Log("Échec de l'envoi de l'expédition");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("SendExpedition", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RefreshExpeditionGrid()
        {
            ((BindingSource)dataGridViewExpedition.DataSource).ResetBindings(false);
        }

        private void ClearExpeditionFields()
        {
            textBoxExpEquipmentCode.Text = "";
            textBoxExpEquipmentName.Text = "";
            textBoxExpSerialNumber.Text = "";
            // Ne pas effacer le code de dépôt cible
        }
    }
}