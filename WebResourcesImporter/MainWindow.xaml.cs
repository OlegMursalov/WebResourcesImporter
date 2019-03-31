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

        private static string types = "*.html;*.css;*.js;*.xml;*.png;*.jpg;*.gif;*.xap;*.xsl;*.ico;*.svg;*.resx";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            ProcessingControlsEnabled(false, SolutionName, Import, Disconnect, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox);
            if (!string.IsNullOrEmpty(SolutionName.Text))
            {
                var overwriteMod = OverwriteFilesCheckBox.IsChecked;
                var changeTheCharactersMod = ChangeTheCharactersCheckBox.IsChecked;
                var importer = new Importer(overwriteMod, changeTheCharactersMod);
                var solutionName = SolutionName.Text;
                var solution = await Task.Run<Entity>(() => {
                    return importer.GetSolutionByName(_service, solutionName);
                });
                if (solution != null)
                {
                    if (solution.Contains("publisherid"))
                    {
                        var publisherid = solution["publisherid"] as EntityReference;
                        if (publisherid != null)
                        {
                            var publisher = importer.GetPublisherById(_service, publisherid.Id);
                            if (publisher != null)
                            {
                                if (publisher.Contains("customizationprefix"))
                                {
                                    var prefix = publisher["customizationprefix"] as string;
                                    if (!string.IsNullOrEmpty(prefix))
                                    {
                                        var fileDialog = new OpenFileDialog();
                                        fileDialog.Filter = $"Web resource ({types})|{types}";
                                        fileDialog.CheckFileExists = true;
                                        fileDialog.Multiselect = true;
                                        var result = fileDialog.ShowDialog();
                                        if (result == true)
                                        {
                                            LoadingLabel.Visibility = Visibility.Visible;
                                            this.Background = new SolidColorBrush(Color.FromRgb(243, 240, 215));
                                            var fileNames = fileDialog.FileNames;
                                            var importInfo = await Task.Run(() => importer.Process(_service, solutionName, prefix, fileNames));
                                            LoadingLabel.Visibility = Visibility.Hidden;
                                            this.Background = new SolidColorBrush(Color.FromRgb(213, 240, 222));
                                            MessageBox.Show(importInfo.GetInfo());
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Publisher has no prefix.");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Publisher has no prefix.");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Publisher not found.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("The solution has no publisher.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("The solution has no publisher.");
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
            ProcessingControlsEnabled(true, SolutionName, Import, Disconnect, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox);
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            _service = null;
            ProcessingControlsEnabled(true, SOAPServiceUri, UserName, Password, Connect, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox);
            CommonProcessInterface(isConnect: false);
        }

        private void CommonProcessInterface(bool isConnect)
        {
            Disconnect.Visibility = isConnect ? Visibility.Visible : Visibility.Hidden;
            SolutionNameLabel.Visibility = isConnect ? Visibility.Visible : Visibility.Hidden;
            SolutionName.Visibility = isConnect ? Visibility.Visible : Visibility.Hidden;
            Import.Visibility = isConnect ? Visibility.Visible : Visibility.Hidden;
            this.Background = new SolidColorBrush(isConnect ? Color.FromRgb(213, 240, 222) : Color.FromRgb(243, 216, 215));
        }

        private void ProcessingControlsEnabled(bool isEnabled, params Control[] controls)
        {
            for (int i = 0; i < controls.Length; i++)
            {
                controls[i].IsEnabled = isEnabled;
            }
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            ProcessingControlsEnabled(false, SOAPServiceUri, UserName, Password, Connect, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox);
            var soapServiceUri = SOAPServiceUri.Text;
            var solutionName = SolutionName.Text;
            var userName = UserName.Text;
            var password = Password.Password;
            if (!string.IsNullOrEmpty(soapServiceUri) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                try
                {
                    var clientCredentials = new ClientCredentials();
                    clientCredentials.UserName.UserName = userName; // Oleg@mursalov.onmicrosoft.com
                    clientCredentials.UserName.Password = password;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    IOrganizationService service = await Task.Run<OrganizationServiceProxy>(() =>
                    {
                        return new OrganizationServiceProxy(new Uri(soapServiceUri), null, clientCredentials, null);
                    });
                    if (service != null)
                    {
                        var response = await Task.Run<OrganizationResponse>(() =>
                        {
                            return service.Execute(new WhoAmIRequest());
                        }) as WhoAmIResponse;
                        if (response != null && response.UserId != Guid.Empty)
                        {
                            _service = service;
                            CommonProcessInterface(isConnect: true);
                            return;
                        }
                        else
                        {
                            MessageBox.Show("No connection.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("No connection.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Fill in the 'SOAP Service uri', 'Username' and 'Password' fields.");
            }
            ProcessingControlsEnabled(true, SOAPServiceUri, UserName, Password, Connect, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox);
        }
    }
}
