using Microsoft.Crm.Sdk.Messages;
using Microsoft.Win32;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Net;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WebResourcesImporter
{
    public partial class MainWindow : Window
    {
        private IOrganizationService _service = null;
        private Importer _importer = null;

        private static string types = "*.html;*.css;*.js;*.xml;*.png;*.jpg;*.gif;*.xap;*.xsl;*.ico;*.svg;*.resx";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            ProcessingControlsVisibility(false, Info);
            ProcessingControlsEnabled(false, SolutionName, SelectSolution, ImportRadio, ExportRadio, Import, Disconnect, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox);
            if (_importer != null)
            {
                var fileDialog = new OpenFileDialog();
                fileDialog.Filter = $"Web resource ({types})|{types}";
                fileDialog.CheckFileExists = true;
                fileDialog.Multiselect = true;
                var result = fileDialog.ShowDialog();
                if (result == true)
                {
                    LoadingLabel.Visibility = Visibility.Visible;
                    SetMainWindowBackground(Color.FromRgb(243, 240, 215));
                    var fileNames = fileDialog.FileNames;
                    var overwriteMod = OverwriteFilesCheckBox.IsChecked;
                    var changeTheCharactersMod = ChangeTheCharactersCheckBox.IsChecked;
                    var importInfo = await Task.Run(() => _importer.Process(_service, fileNames, overwriteMod, changeTheCharactersMod));
                    LoadingLabel.Visibility = Visibility.Hidden;
                    SetMainWindowBackground(Color.FromRgb(213, 240, 222));
                    var infoList = importInfo.GetInfo();
                    Info.Items.Clear();
                    foreach (var item in infoList)
                    {
                        Info.Items.Add(item);
                    }
                    ProcessingControlsVisibility(true, Info);
                }
            }
            ProcessingControlsEnabled(true, SolutionName, SelectSolution, ImportRadio, ExportRadio, Import, Disconnect, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox);
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            _service = null;
            _importer = null;
            SetMainWindowBackground(Color.FromRgb(243, 216, 215));
            ProcessingControlsEnabled(true, SOAPServiceUri, UserName, Password, Connect);
            ProcessingControlsVisibility(false, Disconnect, SolutionNameLabel, SolutionName, SelectSolution, SelectActionLabel, 
                ImportRadio, ExportRadio, TitleAction, Import, SettingsImport, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox, Info);
            Info.Items.Clear();
            SolutionName.Text = string.Empty;
            ImportRadio.IsChecked = true;
            OverwriteFilesCheckBox.IsChecked = false;
            ChangeTheCharactersCheckBox.IsChecked = false;
        }

        private void SetMainWindowBackground(Color color)
        {
            this.Background = new SolidColorBrush(color);
        }

        private void ProcessingControlsVisibility(bool isVisible, params Control[] controls)
        {
            for (int i = 0; i < controls.Length; i++)
            {
                controls[i].Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void ProcessingControlsEnabled(bool isEnabled, params Control[] controls)
        {
            for (int i = 0; i < controls.Length; i++)
            {
                controls[i].IsEnabled = isEnabled;
            }
        }

        private async void Select_Solution_Click(object sender, RoutedEventArgs e)
        {
            ProcessingControlsEnabled(false, SolutionName, Disconnect, SelectSolution);
            ProcessingControlsVisibility(false, SelectActionLabel, ImportRadio, ExportRadio, TitleAction, Import, SettingsImport, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox, Info);
            _importer = new Importer();
            var solutionName = SolutionName.Text;
            if (!string.IsNullOrEmpty(solutionName))
            {
                var solution = await Task.Run<Entity>(() =>
                {
                    return _importer.GetSolutionByName(_service, solutionName);
                });
                if (solution != null)
                {
                    var prefix = await Task.Run<string>(() =>
                    {
                        return _importer.GetPrefixFromSolutionPublisher(_service, solution);
                    });
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        ImportRadio.IsChecked = true;
                        OverwriteFilesCheckBox.IsChecked = false;
                        ChangeTheCharactersCheckBox.IsChecked = false;
                        ProcessingControlsVisibility(true, SelectActionLabel, ImportRadio, ExportRadio, TitleAction, Import, SettingsImport, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox);
                    }
                    else
                    {
                        MessageBox.Show("Publisher has no prefix.");
                    }
                }
                else
                {
                    MessageBox.Show("No solution was found.");
                }
            }
            else
            {
                MessageBox.Show("Fill in the 'Solution name' field.");
            }
            ProcessingControlsEnabled(true, SolutionName, Disconnect, SelectSolution);
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            ProcessingControlsEnabled(false, SOAPServiceUri, UserName, Password, Connect);
            var soapServiceUri = SOAPServiceUri.Text;
            var solutionName = SolutionName.Text;
            var userName = UserName.Text;
            var password = Password.Password;
            if (!string.IsNullOrEmpty(soapServiceUri) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                IOrganizationService service = null;
                var clientCredentials = new ClientCredentials();
                clientCredentials.UserName.UserName = userName; // Oleg@mursalov.onmicrosoft.com
                clientCredentials.UserName.Password = password;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                try
                {
                    service = await Task.Run<OrganizationServiceProxy>(() =>
                    {
                        return new OrganizationServiceProxy(new Uri(soapServiceUri), null, clientCredentials, null);
                    });
                }
                catch (UriFormatException)
                {
                    MessageBox.Show("The 'SOAP service uri' field is not correct. Invalid URI: Unable to determine URI format.");
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.HResult == -2146233079)
                    {
                        MessageBox.Show("The 'SOAP service uri' field is not correct. Metadata contains an unsolvable link.");
                    }
                }
                if (service != null)
                {
                    WhoAmIResponse response = null;
                    try
                    {
                        response = await Task.Run<OrganizationResponse>(() =>
                        {
                            return service.Execute(new WhoAmIRequest());
                        }) as WhoAmIResponse;
                    }
                    catch (Exception ex)
                    {
                        if (ex.HResult == -2147467261 || ex.HResult == -2146233087)
                        {
                            MessageBox.Show("Login failed.");
                        }
                    }
                    if (response != null && response.UserId != Guid.Empty)
                    {
                        _service = service;
                        SetMainWindowBackground(Color.FromRgb(213, 240, 222));
                        ProcessingControlsVisibility(true, Disconnect, SolutionNameLabel, SolutionName, SelectSolution);
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Fill in the 'SOAP Service uri', 'Username' and 'Password' fields.");
            }
            ProcessingControlsEnabled(true, SOAPServiceUri, UserName, Password, Connect);
        }

        private void SolutionName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ImportRadio.IsChecked = true;
            OverwriteFilesCheckBox.IsChecked = false;
            ChangeTheCharactersCheckBox.IsChecked = false;
            Info.Items.Clear();
            ProcessingControlsVisibility(false, SelectActionLabel, ImportRadio, ExportRadio, TitleAction, Import, SettingsImport, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox, Info);
        }

        private void OverwriteFilesCheckBox_Change(object sender, RoutedEventArgs e)
        {
            Info.Items.Clear();
            ProcessingControlsVisibility(false, Info);
        }

        private void ChangeTheCharactersCheckBox_Change(object sender, RoutedEventArgs e)
        {
            Info.Items.Clear();
            ProcessingControlsVisibility(false, Info);
        }
    }
}
