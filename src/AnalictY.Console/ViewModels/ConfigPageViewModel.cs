using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnalictY.Console.Models;
using AnalictY.Console.Infrastructure;

namespace AnalictY.Console.ViewModels
{
    public class ConfigPageViewModel : INotifyPropertyChanged
    {
        private string _selectedSector = "all";
        private string _timeZoneId = "America/Sao_Paulo";
        private string _timeZoneLabel = "Brasil - Brasília (GMT-3)";
        private string _timeZoneMessage = "";
        private DateTime _currentClock = DateTime.Now;
        private string _systemVersion = "1.0.0";
        private bool _loading = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ConfigPageViewModel()
        {
            InitializeSectors();
            InitializeCards();
            InitializeTimeZoneOptions();
            InitializeCommands();
            LoadConfigs();
        }

        private void InitializeCommands()
        {
            SelectSectorCommand = new RelayCommand(parameter => 
            {
                if (parameter is string sectorId)
                {
                    SelectSector(sectorId);
                }
                return Task.CompletedTask;
            });
            SaveTimeZoneCommand = new RelayCommand(_ => SaveTimeZone());
        }

        private void InitializeSectors()
        {
            ConfigSectors = new ObservableCollection<ConfigSector>
            {
                new ConfigSector { Id = "all", Label = "Todos", Description = "Tudo" },
                new ConfigSector { Id = "connections", Label = "Conexões", Description = "OPC, MQTT e banco" },
                new ConfigSector { Id = "production", Label = "Produção", Description = "Máquinas, tags e turnos" },
                new ConfigSector { Id = "alerts", Label = "Alertas", Description = "Notificações" },
                new ConfigSector { Id = "visualization", Label = "Visualização", Description = "Dashboards e BI" },
                new ConfigSector { Id = "system", Label = "Sistema", Description = "Logs e administração" }
            };
        }

        private void InitializeCards()
        {
            ConfigCards = new ObservableCollection<ConfigCard>
            {
                // Conexões
                new ConfigCard 
                { 
                    Id = "opc", 
                    Title = "OPC UA", 
                    Description = "Navegação de nós OPC UA", 
                    Icon = "Server", 
                    IconPath = "M3 2h18a2 2 0 012 2v14a2 2 0 01-2 2H3a2 2 0 01-2-2V4a2 2 0 012-2zm0 2v14h18V4H3zm2 2h2v2H5V6zm0 4h2v2H5v-2zm0 4h2v2H5v-2zm4-8h10v2H9V6zm0 4h10v2H9v-2zm0 4h10v2H9v-2z",
                    Count = "0", 
                    Category = "connections",
                    ColorHex = "#10B981",
                    HoverColorHex = "#D1FAE5",
                    IconColorHex = "#059669",
                    IsButton = true,
                    NavigationTarget = "opc-browser"
                },
                new ConfigCard 
                { 
                    Id = "mqtt", 
                    Title = "MQTT", 
                    Description = "Comunicação MQTT/TLS", 
                    Icon = "Wifi", 
                    IconPath = "M5 12.55a11 11 0 0110.093-6.712 11 11 0 0110.093 6.712M5 12.55a11 11 0 0010.093 6.712 11 11 0 0010.093-6.712M5 12.55a11 11 0 0010.093 6.712 11 11 0 0010.093-6.712M12 3v18",
                    Count = "0", 
                    Category = "connections",
                    ColorHex = "#6366F1",
                    HoverColorHex = "#E0E7FF",
                    IconColorHex = "#4F46E5",
                    IsButton = true,
                    NavigationTarget = "mqtt-monitor"
                },
                new ConfigCard 
                { 
                    Id = "mysql", 
                    Title = "Banco de Dados", 
                    Description = "Conexões MySQL e SQL Server", 
                    Icon = "Database", 
                    IconPath = "M12 2C6.48 2 2 4.02 2 6.5S6.48 11 12 11s10-2.02 10-4.5S17.52 2 12 2zm0 2c3.86 0 7 1.12 7 2.5S15.86 9 12 9s-7-1.12-7-2.5S8.14 4 12 4zm0 8c-5.52 0-10 2.02-10 4.5S6.48 21 12 21s10-2.02 10-4.5S17.52 12 12 12zm0 2c3.86 0 7 1.12 7 2.5S15.86 19 12 19s-7-1.12-7-2.5S8.14 14 12 14z",
                    Count = "0", 
                    Category = "connections",
                    ColorHex = "#0EA5E9",
                    HoverColorHex = "#E0F2FE",
                    IconColorHex = "#0284C7",
                    IsButton = true,
                    NavigationTarget = "database-browser"
                },
                new ConfigCard 
                { 
                    Id = "connections", 
                    Title = "Conexões", 
                    Description = "Todas as conexões salvas", 
                    Icon = "Plug", 
                    IconPath = "M12 2v20M2 12h20M4.93 4.93l14.14 14.14M19.07 4.93L4.93 19.07",
                    Count = "0", 
                    Category = "connections",
                    ColorHex = "#A855F7",
                    HoverColorHex = "#F3E8FF",
                    IconColorHex = "#9333EA",
                    IsButton = true,
                    NavigationTarget = "connections"
                },
                new ConfigCard 
                { 
                    Id = "weintek", 
                    Title = "Weintek HTTP", 
                    Description = "Gateway FHDX/cMT e browser de tags", 
                    Icon = "Network", 
                    IconPath = "M4 6a2 2 0 012-2h12a2 2 0 012 2v12a2 2 0 01-2 2H6a2 2 0 01-2-2V6zm2 0v12h12V6H6zm2 2h8v2H8V8zm0 4h8v2H8v-2zm0 4h5v2H8v-2z",
                    Count = "", 
                    Category = "connections",
                    ColorHex = "#EF4444",
                    HoverColorHex = "#FEE2E2",
                    IconColorHex = "#DC2626",
                    IsButton = true,
                    NavigationTarget = "weintek-browser"
                },
                // Produção
                new ConfigCard 
                { 
                    Id = "tags", 
                    Title = "TAGs", 
                    Description = "Gestão de TAGs do sistema", 
                    Icon = "Tags", 
                    IconPath = "M20.59 13.41l-7.17 7.17a2 2 0 01-2.83 0L2 12V2h10l8.59 8.59a2 2 0 010 2.82zM7 7a2 2 0 100 4 2 2 0 000-4z",
                    Count = "", 
                    Category = "production",
                    ColorHex = "#F59E0B",
                    HoverColorHex = "#FEF3C7",
                    IconColorHex = "#D97706",
                    IsButton = true,
                    NavigationTarget = "tags"
                },
                new ConfigCard 
                { 
                    Id = "machines", 
                    Title = "Máquinas", 
                    Description = "Gestão de máquinas do sistema", 
                    Icon = "Factory", 
                    IconPath = "M2 20a2 2 0 002 2h16a2 2 0 002-2V8l-7 5V8l-7 5V4a2 2 0 00-2-2H4a2 2 0 00-2 2v16z",
                    Count = "", 
                    Category = "production",
                    ColorHex = "#64748B",
                    HoverColorHex = "#F1F5F9",
                    IconColorHex = "#475569",
                    IsButton = true,
                    NavigationTarget = "machines"
                },
                new ConfigCard 
                { 
                    Id = "shifts", 
                    Title = "Turnos", 
                    Description = "Janelas operacionais da fábrica", 
                    Icon = "Clock", 
                    IconPath = "M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z",
                    Count = "", 
                    Category = "production",
                    ColorHex = "#22C55E",
                    HoverColorHex = "#DCFCE7",
                    IconColorHex = "#16A34A",
                    IsButton = true,
                    NavigationTarget = "shifts"
                },
                new ConfigCard 
                { 
                    Id = "simulator", 
                    Title = "Simulador", 
                    Description = "Console de máquina virtual", 
                    Icon = "Flask", 
                    IconPath = "M10 2v7.31l-4 4v2.69l4 4V22h4v-2l4-4v-2.69l-4-4V2h-8zm2 2h4v5.17l2 2v1.66l-2 2V18h-4v-3.17l-2-2v-1.66l2-2V4z",
                    Count = "", 
                    Category = "production",
                    ColorHex = "#14B8A6",
                    HoverColorHex = "#CCFBF1",
                    IconColorHex = "#0D9488",
                    IsButton = true,
                    NavigationTarget = "simulator"
                },
                new ConfigCard 
                { 
                    Id = "diagnostics", 
                    Title = "Diagnóstico", 
                    Description = "Fluxo real de produção", 
                    Icon = "TestTube", 
                    IconPath = "M9 2h6v2H9V2zm0 4h6v2H9V6zm0 4h6v2H9v-2zm0 4h6v2H9v-2zm0 4h6v2H9v-2z",
                    Count = "", 
                    Category = "production",
                    ColorHex = "#2563EB",
                    HoverColorHex = "#DBEAFE",
                    IconColorHex = "#1D4ED8",
                    IsButton = true,
                    NavigationTarget = "production-diagnostics"
                },
                // Alertas
                new ConfigCard 
                { 
                    Id = "alerts", 
                    Title = "Alertas", 
                    Description = "Regras e reconhecimento", 
                    Icon = "Bell", 
                    IconPath = "M18 8A6 6 0 006 8c0 7-3 9-3 9h18s-3-2-3-9M13.73 21a2 2 0 01-3.46 0",
                    Count = "", 
                    Category = "alerts",
                    ColorHex = "#F43F5E",
                    HoverColorHex = "#FFE4E6",
                    IconColorHex = "#E11D48",
                    IsButton = true,
                    NavigationTarget = "alerts"
                },
                new ConfigCard 
                { 
                    Id = "telegram", 
                    Title = "Telegram", 
                    Description = "Bot, chat_id e destinatários", 
                    Icon = "Send", 
                    IconPath = "M22 2L11 13M22 2l-7 20-4-9-9-4 20-7z",
                    Count = "", 
                    Category = "alerts",
                    ColorHex = "#2563EB",
                    HoverColorHex = "#DBEAFE",
                    IconColorHex = "#1D4ED8",
                    IsButton = true,
                    NavigationTarget = "telegram-notifications"
                },
                // Visualização
                new ConfigCard 
                { 
                    Id = "dashboards", 
                    Title = "Dashboards", 
                    Description = "Layouts de gráficos por máquina", 
                    Icon = "Layout", 
                    IconPath = "M3 3h7v7H3V3zm0 11h7v7H3v-7zm11-11h7v7h-7V3zm0 11h7v7h-7v-7z",
                    Count = "", 
                    Category = "visualization",
                    ColorHex = "#8B5CF6",
                    HoverColorHex = "#EDE9FE",
                    IconColorHex = "#7C3AED",
                    IsButton = true,
                    NavigationTarget = "dashboards"
                },
                // Sistema
                new ConfigCard 
                { 
                    Id = "logs", 
                    Title = "Logs", 
                    Description = "Console técnico do sistema", 
                    Icon = "Terminal", 
                    IconPath = "M4 17l6-6-6-6M12 19h8",
                    Count = "", 
                    Category = "system",
                    ColorHex = "#06B6D4",
                    HoverColorHex = "#CFFAFE",
                    IconColorHex = "#0891B2",
                    IsButton = true,
                    NavigationTarget = "logs"
                },
                new ConfigCard 
                { 
                    Id = "localServer", 
                    Title = "Servidor local", 
                    Description = "Acesso por localhost ou IP fixo", 
                    Icon = "Network", 
                    IconPath = "M4 6a2 2 0 012-2h12a2 2 0 012 2v12a2 2 0 01-2 2H6a2 2 0 01-2-2V6zm2 0v12h12V6H6zm2 2h8v2H8V8zm0 4h8v2H8v-2zm0 4h5v2H8v-2z",
                    Count = "", 
                    Category = "system",
                    ColorHex = "#0891B2",
                    HoverColorHex = "#CFFAFE",
                    IconColorHex = "#0E7490",
                    IsButton = true,
                    NavigationTarget = "local-server"
                },
                new ConfigCard 
                { 
                    Id = "users", 
                    Title = "Usuários", 
                    Description = "Contas, perfis e permissões", 
                    Icon = "Users", 
                    IconPath = "M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2M9 7a4 4 0 110-8 4 4 0 010 8zm10 10a4 4 0 004-4h-3m-3-4a4 4 0 110-8 4 4 0 010 8z",
                    Count = "", 
                    Category = "system",
                    ColorHex = "#EF4444",
                    HoverColorHex = "#FEE2E2",
                    IconColorHex = "#DC2626",
                    IsButton = true,
                    NavigationTarget = "users"
                },
                new ConfigCard 
                { 
                    Id = "audit", 
                    Title = "Auditoria", 
                    Description = "Eventos administrativos recentes", 
                    Icon = "Shield", 
                    IconPath = "M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z",
                    Count = "", 
                    Category = "system",
                    ColorHex = "#334155",
                    HoverColorHex = "#F1F5F9",
                    IconColorHex = "#1E293B",
                    IsButton = true,
                    NavigationTarget = "audit"
                }
            };
        }

        private void InitializeTimeZoneOptions()
        {
            TimeZoneOptions = new ObservableCollection<TimeZoneOption>
            {
                new TimeZoneOption { Id = "America/Sao_Paulo", Label = "Brasil - Brasília (GMT-3)" },
                new TimeZoneOption { Id = "America/New_York", Label = "Estados Unidos - Nova York (GMT-5)" },
                new TimeZoneOption { Id = "Europe/London", Label = "Reino Unido - Londres (GMT+0)" },
                new TimeZoneOption { Id = "Europe/Berlin", Label = "Alemanha - Berlim (GMT+1)" },
                new TimeZoneOption { Id = "Asia/Tokyo", Label = "Japão - Tóquio (GMT+9)" },
                new TimeZoneOption { Id = "Australia/Sydney", Label = "Austrália - Sydney (GMT+10)" }
            };
        }

        private async void LoadConfigs()
        {
            Loading = true;
            await Task.Delay(500); // Simulate loading
            Loading = false;
        }

        public ObservableCollection<ConfigSector> ConfigSectors { get; set; } = new();
        public ObservableCollection<ConfigCard> ConfigCards { get; set; } = new();
        public ObservableCollection<TimeZoneOption> TimeZoneOptions { get; set; } = new();

        public string SelectedSector
        {
            get => _selectedSector;
            set
            {
                if (_selectedSector != value)
                {
                    _selectedSector = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TimeZoneId
        {
            get => _timeZoneId;
            set
            {
                if (_timeZoneId != value)
                {
                    _timeZoneId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TimeZoneLabel
        {
            get => _timeZoneLabel;
            set
            {
                if (_timeZoneLabel != value)
                {
                    _timeZoneLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TimeZoneMessage
        {
            get => _timeZoneMessage;
            set
            {
                if (_timeZoneMessage != value)
                {
                    _timeZoneMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime CurrentClock
        {
            get => _currentClock;
            set
            {
                if (_currentClock != value)
                {
                    _currentClock = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SystemVersion
        {
            get => _systemVersion;
            set
            {
                if (_systemVersion != value)
                {
                    _systemVersion = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Loading
        {
            get => _loading;
            set
            {
                if (_loading != value)
                {
                    _loading = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SelectSectorCommand { get; private set; } = new RelayCommand(_ => Task.CompletedTask);
        public ICommand SaveTimeZoneCommand { get; private set; } = new RelayCommand(_ => Task.CompletedTask);

        public bool ShowCard(ConfigCard card)
        {
            return SelectedSector == "all" || card.Category == SelectedSector;
        }

        public void SelectSector(string sectorId)
        {
            SelectedSector = sectorId;
        }

        public async Task SaveTimeZone()
        {
            TimeZoneMessage = "Salvando...";
            await Task.Delay(1000);
            var selected = TimeZoneOptions.FirstOrDefault(t => t.Id == TimeZoneId);
            if (selected != null)
            {
                TimeZoneLabel = selected.Label;
            }
            TimeZoneMessage = "Fuso horário salvo com sucesso.";
            await Task.Delay(2000);
            TimeZoneMessage = "";
        }

        public void UpdateClock()
        {
            CurrentClock = DateTime.Now;
        }
    }
}
