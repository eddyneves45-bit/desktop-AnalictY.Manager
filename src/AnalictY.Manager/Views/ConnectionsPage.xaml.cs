using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views
{
    public partial class ConnectionsPage : Page
    {
        private const string PasswordPlaceholder = "********";
        private ConnectionsPageViewModel? _viewModel;
        private ConnectionRow? _selectedConnection;
        private bool _isEditMode;
        private bool _passwordHasPlaceholder;

        public ConnectionsPage()
        {
            InitializeComponent();
            _viewModel = new ConnectionsPageViewModel(new ConfigService(AppServices.HttpClient));
            DataContext = _viewModel;
            Loaded += ConnectionsPage_Loaded;
        }

        private async void ConnectionsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                await _viewModel.LoadAsync();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this);
            while (parent != null && parent is not ConfigPage)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is ConfigPage configPage)
            {
                configPage.ReturnToCards();
            }
        }

        private void NewConnection_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = false;
            _selectedConnection = null;
            DialogTitle.Text = "Nova Conexao";
            ClearDialogFields();
            TypeComboBox.SelectedIndex = 0;
            TypeComboBox.IsEnabled = true;
            EditDialogOverlay.Visibility = Visibility.Visible;
        }

        private void EditConnection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: ConnectionRow connection })
            {
                return;
            }

            _isEditMode = true;
            _selectedConnection = connection;
            DialogTitle.Text = "Editar Conexao";
            ClearDialogFields();

            TypeComboBox.SelectedItem = connection.Type;
            TypeComboBox.IsEnabled = false;
            NameTextBox.Text = connection.Name;
            ActiveCheckBox.IsChecked = connection.IsActive;
            UsernameTextBox.Text = connection.Username;

            if (connection.Type == "OPC UA")
            {
                EndpointTextBox.Text = connection.Address;
                SetCombo(SecurityPolicyComboBox, connection.SecurityPolicy);
                SetCombo(SecurityModeComboBox, connection.SecurityMode);
                CertificatePathTextBox.Text = connection.CertificatePath;
                PrivateKeyPathTextBox.Text = connection.PrivateKeyPath;
                UpdateIntervalTextBox.Text = string.IsNullOrWhiteSpace(connection.UpdateInterval) ? "1000" : connection.UpdateInterval;
                if (!string.IsNullOrWhiteSpace(connection.Username))
                {
                    MarkPasswordAsConfigured();
                }
            }
            else if (connection.Type == "MQTT")
            {
                HostTextBox.Text = string.IsNullOrWhiteSpace(connection.Host) ? ReadHost(connection.Address) : connection.Host;
                PortTextBox.Text = string.IsNullOrWhiteSpace(connection.Port) ? ReadPort(connection.Address, "1883") : connection.Port;
                ClientIdTextBox.Text = connection.ClientId;
                TlsCheckBox.IsChecked = connection.TlsEnabled;
                CaCertPathTextBox.Text = connection.CaCertPath;
                ClientCertPathTextBox.Text = connection.ClientCertPath;
                ClientKeyPathTextBox.Text = connection.ClientKeyPath;
                TopicsTextBox.Text = connection.Topics;
                SetCombo(QosComboBox, connection.Qos);
                if (!string.IsNullOrWhiteSpace(connection.Username))
                {
                    MarkPasswordAsConfigured();
                }
            }
            else if (connection.Type is "MySQL" or "SQL Server")
            {
                HostTextBox.Text = string.IsNullOrWhiteSpace(connection.Host) ? ReadHost(connection.Address) : connection.Host;
                PortTextBox.Text = string.IsNullOrWhiteSpace(connection.Port) ? ReadPort(connection.Address, connection.Type == "SQL Server" ? "1433" : "3306") : connection.Port;
                DatabaseTextBox.Text = string.IsNullOrWhiteSpace(connection.Database) ? ReadDatabase(connection.Address) : connection.Database;
                PoolSizeTextBox.Text = string.IsNullOrWhiteSpace(connection.PoolSize) ? "10" : connection.PoolSize;
                if (!string.IsNullOrWhiteSpace(connection.Username))
                {
                    MarkPasswordAsConfigured();
                }
            }
            else if (connection.Type == "FTP/SFTP")
            {
                HostTextBox.Text = string.IsNullOrWhiteSpace(connection.Host) ? ReadHost(connection.Address) : connection.Host;
                PortTextBox.Text = string.IsNullOrWhiteSpace(connection.Port) ? ReadPort(connection.Address, "22") : connection.Port;
                SetCombo(ProtocolComboBox, connection.Protocol);
                FtpPrivateKeyPathTextBox.Text = connection.PrivateKeyPath;
                DirectoryTextBox.Text = string.IsNullOrWhiteSpace(connection.Directory) ? "/" : connection.Directory;
                FrequencyTextBox.Text = string.IsNullOrWhiteSpace(connection.Frequency) ? "manual" : connection.Frequency;
                DataTypeTextBox.Text = string.IsNullOrWhiteSpace(connection.DataType) ? "production" : connection.DataType;
                SetCombo(FileFormatComboBox, connection.FileFormat);
                if (connection.PasswordConfigured || !string.IsNullOrWhiteSpace(connection.Username))
                {
                    MarkPasswordAsConfigured();
                }
            }

            UpdateFieldVisibility();
            EditDialogOverlay.Visibility = Visibility.Visible;
        }

        private async void DeleteConnection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: ConnectionRow connection })
            {
                return;
            }

            if (connection.Type == "FTP/SFTP")
            {
                ShowNotification("Atencao", "Configuracao FTP/SFTP nao pode ser excluida, apenas editada.");
                return;
            }

            var result = MessageBox.Show(
                $"Deseja excluir a conexao '{connection.Name}'?",
                "Confirmar Exclusao",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes && _viewModel != null)
            {
                _viewModel.ErrorMessage = string.Empty;
                _viewModel.SelectedConnection = connection;
                await _viewModel.DeleteConnectionAsync();
                ShowOperationNotification("Conexao excluida");
            }
        }

        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ConnectionRow connection } && _viewModel != null)
            {
                _viewModel.ErrorMessage = string.Empty;
                _viewModel.SelectedConnection = connection;
                await _viewModel.TestConnectionAsync();
                ShowOperationNotification("Conexao estabelecida");
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: string filter } && _viewModel != null)
            {
                _viewModel.SelectedTypeFilter = filter;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.SearchText = SearchBox.Text;
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFieldVisibility();
        }

        private void UpdateFieldVisibility()
        {
            if (TypeComboBox.SelectedItem is not string selectedType)
            {
                return;
            }

            OpcUaFields.Visibility = selectedType == "OPC UA" ? Visibility.Visible : Visibility.Collapsed;
            HostPortFields.Visibility = selectedType is "MQTT" or "MySQL" or "SQL Server" or "FTP/SFTP"
                ? Visibility.Visible
                : Visibility.Collapsed;
            DatabaseFields.Visibility = selectedType is "MySQL" or "SQL Server" ? Visibility.Visible : Visibility.Collapsed;
            MqttFields.Visibility = selectedType == "MQTT" ? Visibility.Visible : Visibility.Collapsed;
            FtpFields.Visibility = selectedType == "FTP/SFTP" ? Visibility.Visible : Visibility.Collapsed;
            CredentialFields.Visibility = selectedType is "OPC UA" or "MQTT" or "MySQL" or "SQL Server" or "FTP/SFTP"
                ? Visibility.Visible
                : Visibility.Collapsed;
            MysqlActions.Visibility = selectedType is "MySQL" or "SQL Server" ? Visibility.Visible : Visibility.Collapsed;
            FtpActions.Visibility = selectedType == "FTP/SFTP" ? Visibility.Visible : Visibility.Collapsed;

            if (_isEditMode)
            {
                return;
            }

            PortTextBox.Text = selectedType switch
            {
                "MQTT" => "1883",
                "MySQL" => "3306",
                "SQL Server" => "1433",
                "FTP/SFTP" => ComboValue(ProtocolComboBox) == "FTP" ? "21" : "22",
                _ => PortTextBox.Text
            };
        }

        private async void SaveConnection_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                return;
            }

            ErrorBorder.Visibility = Visibility.Collapsed;
            _viewModel.ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowError("Nome e obrigatorio.");
                return;
            }

            if (_passwordHasPlaceholder && PasswordBox.Password == PasswordPlaceholder)
            {
                ShowError("Digite a senha correta para salvar alteracoes nesta conexao. A senha salva nao e exibida por seguranca.");
                return;
            }

            _viewModel.EditName = NameTextBox.Text;
            _viewModel.EditType = TypeComboBox.SelectedItem?.ToString() ?? string.Empty;
            _viewModel.EditEndpoint = EndpointTextBox.Text;
            _viewModel.EditHost = HostTextBox.Text;
            _viewModel.EditPort = PortTextBox.Text;
            _viewModel.EditDatabase = DatabaseTextBox.Text;
            _viewModel.EditUsername = UsernameTextBox.Text;
            _viewModel.EditPassword = _passwordHasPlaceholder && PasswordBox.Password == PasswordPlaceholder ? string.Empty : PasswordBox.Password;
            _viewModel.EditSecurityPolicy = ComboValue(SecurityPolicyComboBox) ?? "None";
            _viewModel.EditSecurityMode = ComboValue(SecurityModeComboBox) ?? "None";
            _viewModel.EditCertificatePath = CertificatePathTextBox.Text;
            _viewModel.EditPrivateKeyPath = PrivateKeyPathTextBox.Text;
            _viewModel.EditFtpPrivateKeyPath = FtpPrivateKeyPathTextBox.Text;
            _viewModel.EditUpdateInterval = UpdateIntervalTextBox.Text;
            _viewModel.EditClientId = ClientIdTextBox.Text;
            _viewModel.EditTlsEnabled = TlsCheckBox.IsChecked == true;
            _viewModel.EditCaCertPath = CaCertPathTextBox.Text;
            _viewModel.EditClientCertPath = ClientCertPathTextBox.Text;
            _viewModel.EditClientKeyPath = ClientKeyPathTextBox.Text;
            _viewModel.EditTopics = TopicsTextBox.Text;
            _viewModel.EditQos = ComboValue(QosComboBox) ?? "0";
            _viewModel.EditPoolSize = PoolSizeTextBox.Text;
            _viewModel.EditIsActive = ActiveCheckBox.IsChecked == true;
            _viewModel.EditProtocol = ComboValue(ProtocolComboBox) ?? "SFTP";
            _viewModel.EditDirectory = DirectoryTextBox.Text;
            _viewModel.EditFrequency = FrequencyTextBox.Text;
            _viewModel.EditDataType = DataTypeTextBox.Text;
            _viewModel.EditFileFormat = ComboValue(FileFormatComboBox) ?? "CSV";

            if (_isEditMode && _selectedConnection != null)
            {
                _viewModel.SelectedConnection = _selectedConnection;
                _viewModel.EditDialogType = "Edit";
            }
            else
            {
                _viewModel.EditDialogType = "Create";
            }

            await _viewModel.SaveConnectionAsync();

            if (string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
            {
                EditDialogOverlay.Visibility = Visibility.Collapsed;
                ShowNotification("Conexao salva", _viewModel.StatusMessage);
            }
            else
            {
                ShowError(_viewModel.ErrorMessage);
                ShowNotification("Falha ao salvar", _viewModel.ErrorMessage);
            }
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            EditDialogOverlay.Visibility = Visibility.Collapsed;
            ClearDialogFields();
        }

        private void ClearDialogFields()
        {
            NameTextBox.Text = string.Empty;
            EndpointTextBox.Text = "opc.tcp://localhost:4840";
            HostTextBox.Text = string.Empty;
            PortTextBox.Text = string.Empty;
            DatabaseTextBox.Text = string.Empty;
            UsernameTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            _passwordHasPlaceholder = false;

            SecurityPolicyComboBox.SelectedIndex = 0;
            SecurityModeComboBox.SelectedIndex = 0;
            CertificatePathTextBox.Text = string.Empty;
            PrivateKeyPathTextBox.Text = string.Empty;
            FtpPrivateKeyPathTextBox.Text = string.Empty;
            UpdateIntervalTextBox.Text = "1000";
            ClientIdTextBox.Text = string.Empty;
            TopicsTextBox.Text = string.Empty;
            QosComboBox.SelectedIndex = 0;
            TlsCheckBox.IsChecked = false;
            CaCertPathTextBox.Text = string.Empty;
            ClientCertPathTextBox.Text = string.Empty;
            ClientKeyPathTextBox.Text = string.Empty;
            PoolSizeTextBox.Text = "10";
            ProtocolComboBox.SelectedIndex = 0;
            DirectoryTextBox.Text = "/";
            FrequencyTextBox.Text = "manual";
            DataTypeTextBox.Text = "production";
            FileFormatComboBox.SelectedIndex = 0;
            ActiveCheckBox.IsChecked = true;
        }

        private async void SetMysqlPrimary_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedConnection != null && _viewModel != null)
            {
                _viewModel.SelectedConnection = _selectedConnection;
                await _viewModel.SetMysqlPrimaryAsync();
                ShowOperationNotification("Banco atualizado");
            }
        }

        private async void SetMysqlLocal_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedConnection != null && _viewModel != null)
            {
                _viewModel.SelectedConnection = _selectedConnection;
                await _viewModel.SetMysqlLocalAsync();
                ShowOperationNotification("Banco atualizado");
            }
        }

        private async void SetMysqlRemote_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedConnection != null && _viewModel != null)
            {
                _viewModel.SelectedConnection = _selectedConnection;
                await _viewModel.SetMysqlRemoteAsync();
                ShowOperationNotification("Banco atualizado");
            }
        }

        private async void InitMysql_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedConnection != null && _viewModel != null)
            {
                _viewModel.SelectedConnection = _selectedConnection;
                await _viewModel.InitMysqlAsync();
                ShowOperationNotification("Banco inicializado");
            }
        }

        private async void TestFtp_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                return;
            }

            ErrorBorder.Visibility = Visibility.Collapsed;
            _viewModel.ErrorMessage = string.Empty;

            _viewModel.EditName = NameTextBox.Text;
            _viewModel.EditHost = HostTextBox.Text;
            _viewModel.EditPort = PortTextBox.Text;
            _viewModel.EditUsername = UsernameTextBox.Text;
            _viewModel.EditPassword = _passwordHasPlaceholder && PasswordBox.Password == PasswordPlaceholder ? string.Empty : PasswordBox.Password;
            _viewModel.EditProtocol = ComboValue(ProtocolComboBox) ?? "SFTP";
            _viewModel.EditDirectory = DirectoryTextBox.Text;
            _viewModel.EditPrivateKeyPath = FtpPrivateKeyPathTextBox.Text;

            await _viewModel.TestFtpAsync();

            if (string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
            {
                ShowNotification("Teste FTP", _viewModel.StatusMessage);
            }
            else
            {
                ShowError(_viewModel.ErrorMessage);
                ShowNotification("Falha no teste", _viewModel.ErrorMessage);
            }
        }

        private async void SendFtp_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                return;
            }

            ErrorBorder.Visibility = Visibility.Collapsed;
            _viewModel.ErrorMessage = string.Empty;

            await _viewModel.SendFtpNowAsync();

            if (string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
            {
                ShowNotification("Envio FTP", _viewModel.StatusMessage);
            }
            else
            {
                ShowError(_viewModel.ErrorMessage);
                ShowNotification("Falha no envio", _viewModel.ErrorMessage);
            }
        }

        private void PasswordBox_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            if (_passwordHasPlaceholder)
            {
                PasswordBox.Password = string.Empty;
                _passwordHasPlaceholder = false;
            }
        }

        private void CloseNotification_Click(object sender, RoutedEventArgs e)
        {
            NotificationOverlay.Visibility = Visibility.Collapsed;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
        }

        private void ShowOperationNotification(string successTitle)
        {
            if (_viewModel == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
            {
                ShowNotification("Falha na operacao", _viewModel.ErrorMessage);
                return;
            }

            ShowNotification(successTitle, string.IsNullOrWhiteSpace(_viewModel.StatusMessage)
                ? "Operacao concluida com sucesso."
                : _viewModel.StatusMessage);
        }

        private void ShowNotification(string title, string message)
        {
            NotificationTitle.Text = title;
            NotificationMessage.Text = message;
            NotificationOverlay.Visibility = Visibility.Visible;
        }

        private void MarkPasswordAsConfigured()
        {
            PasswordBox.Password = PasswordPlaceholder;
            _passwordHasPlaceholder = true;
        }

        private void FillHostAndPort(string address, string defaultPort)
        {
            HostTextBox.Text = ReadHost(address);
            PortTextBox.Text = ReadPort(address, defaultPort);
        }

        private static string ReadHost(string address)
        {
            var hostPort = address.Split('/')[0];
            var separatorIndex = hostPort.LastIndexOf(':');
            return separatorIndex > 0 ? hostPort[..separatorIndex] : hostPort;
        }

        private static string ReadPort(string address, string defaultPort)
        {
            var hostPort = address.Split('/')[0];
            var separatorIndex = hostPort.LastIndexOf(':');
            return separatorIndex > -1 && separatorIndex + 1 < hostPort.Length
                ? hostPort[(separatorIndex + 1)..]
                : defaultPort;
        }

        private static string ReadDatabase(string address)
        {
            var parts = address.Split('/', 2);
            return parts.Length == 2 ? parts[1] : string.Empty;
        }

        private static void SetCombo(ComboBox comboBox, string? value)
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
            foreach (var item in comboBox.Items)
            {
                if (item is ComboBoxItem comboItem &&
                    string.Equals(comboItem.Content?.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = comboItem;
                    return;
                }

                if (item is string text &&
                    string.Equals(text, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
        }

        private static string? ComboValue(ComboBox comboBox)
        {
            return comboBox.SelectedItem switch
            {
                ComboBoxItem item => item.Content?.ToString(),
                string value => value,
                _ => comboBox.Text
            };
        }
    }
}
