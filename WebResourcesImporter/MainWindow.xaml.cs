using Microsoft.Crm.Sdk.Messages;
using Microsoft.Win32;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using System.Windows.Media;

namespace WebResourcesImporter
{
    public partial class MainWindow : Window
    {
        private IOrganizationService _service = null;
        private ImpExp _impExp = null;
        private ExportInfo _exportInfo = null;

        private static string types = "*.html;*.css;*.js;*.xml;*.png;*.jpg;*.gif;*.xap;*.xsl;*.ico;*.svg;*.resx";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            ProcessingControlsVisibility(false, Info);
            ProcessingControlsEnabled(false, SolutionName, SelectSolution, ImportRadio, ExportRadio, Import, Disconnect, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox);
            if (_impExp != null)
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
                    var importInfo = await Task.Run(() => _impExp.Import(_service, fileNames, overwriteMod, changeTheCharactersMod));
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
            _impExp = null;
            _exportInfo = null;
            SetMainWindowBackground(Color.FromRgb(243, 216, 215));
            ProcessingControlsEnabled(true, SOAPServiceUri, UserName, Password, Connect);
            ProcessingControlsVisibility(false, Disconnect, SolutionNameLabel, SolutionName, SelectSolution, SelectActionLabel, 
                ImportRadio, ExportRadio, TitleAction, Import, SettingsImport, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox, Info);
            Info.Items.Clear();
            SolutionName.Text = string.Empty;
            ImportRadio.IsChecked = true;
            OverwriteFilesCheckBox.IsChecked = false;
            ChangeTheCharactersCheckBox.IsChecked = false;
            RemoveFileCheckBoxes();
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
            RemoveFileCheckBoxes();
            ProcessingControlsEnabled(false, SolutionName, Disconnect, SelectSolution);
            ProcessingControlsVisibility(false, SelectActionLabel, ImportRadio, ExportRadio, TitleAction, Import, Export, SettingsImport, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox, Info);
            _impExp = new ImpExp();
            var solutionName = SolutionName.Text;
            if (!string.IsNullOrEmpty(solutionName))
            {
                var solution = await Task.Run<Entity>(() =>
                {
                    return _impExp.GetSolutionByName(_service, solutionName);
                });
                if (solution != null)
                {
                    var prefix = await Task.Run<string>(() =>
                    {
                        return _impExp.GetPrefixFromSolutionPublisher(_service, solution);
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
                clientCredentials.UserName.UserName = userName; // Oleg@mursalov.onmicrosoft.com // https://mursalov.crm4.dynamics.com/
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
            TitleAction.Content = "Import action:";
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

        private void RemoveFileCheckBoxes()
        {
            var list = new List<CheckBox>();
            foreach (var item in MainGrid.Children)
            {
                var checkBox = item as CheckBox;
                if (checkBox != null && checkBox.Name != null && checkBox.Name.Contains("File_"))
                {
                    list.Add(checkBox);
                }
            }
            foreach (var item in list)
            {
                MainGrid.Children.Remove(item);
            }
        }

        private async void Import_Export_Radio_Checked(object sender, RoutedEventArgs e)
        {
            var pressed = sender as RadioButton;
            var content = pressed.Content as string;
            if (content == "Import")
            {
                TitleAction.Content = "Import action:";
                OverwriteFilesCheckBox.IsChecked = false;
                ChangeTheCharactersCheckBox.IsChecked = false;
                Info.Items.Clear();
                ProcessingControlsVisibility(false, Export);
                ProcessingControlsVisibility(true, SelectActionLabel, ImportRadio, ExportRadio, TitleAction, Import, SettingsImport, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox);
                RemoveFileCheckBoxes();
            }
            else if (content == "Export")
            {
                TitleAction.Content = "Wait, web resources are retrieved from the solution.";
                SetMainWindowBackground(Color.FromRgb(243, 240, 215));
                ProcessingControlsEnabled(false, Disconnect, SolutionName, SelectSolution, ImportRadio, ExportRadio);
                ProcessingControlsVisibility(false, Import, SettingsImport, OverwriteFilesCheckBox, ChangeTheCharactersCheckBox, Info, Export);
                _exportInfo = await Task.Run<ExportInfo>(() =>
                {
                    return _impExp.GetFilesFromSolution(_service);
                });
                if (_exportInfo != null && _exportInfo.Files != null && _exportInfo.Files.Count > 0)
                {
                    int marginTop = 245;
                    var files = _exportInfo.Files.ToArray();
                    CreateSelectAllCheckBox(marginTop);
                    marginTop += 25;
                    for (int i = 0; i < files.Length; i++)
                    {
                        MainGrid.Children.Add(new CheckBox
                        {
                            Name = $"File_{i}",
                            Content = files[i].Name,
                            Margin = new Thickness(10, marginTop, 0, 0),
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Width = 400,
                            Height = 23
                        });
                        marginTop += 25;
                    }
                    TitleAction.Content = "Export action:";
                    Export.Margin = new Thickness(10, marginTop, 0, 0);
                }
                else
                {
                    TitleAction.Content = "Solution does not contain web resources.";
                }
                SetMainWindowBackground(Color.FromRgb(213, 240, 222));
                ProcessingControlsEnabled(true, Disconnect, SolutionName, SelectSolution, ImportRadio, ExportRadio);
                ProcessingControlsVisibility(true, Export);
            }
        }

        private void CreateSelectAllCheckBox(int marginTop)
        {
            var selectAllCheckBox = new CheckBox
            {
                Name = $"File_All",
                Content = "Select all",
                Margin = new Thickness(10, marginTop, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 400,
                Height = 23
            };
            selectAllCheckBox.Checked += SelectAllCheckBox_Checked;
            selectAllCheckBox.Unchecked += SelectAllCheckBox_Unchecked;
            MainGrid.Children.Add(selectAllCheckBox);
        }

        private void SelectAllCheckBox_Common(bool flag)
        {
            foreach (var item in MainGrid.Children)
            {
                var checkBox = item as CheckBox;
                if (checkBox != null && checkBox.Name != null && checkBox.Name.Contains("File_"))
                {
                    checkBox.IsChecked = flag;
                }
            }
        }

        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectAllCheckBox_Common(false);
        }

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SelectAllCheckBox_Common(true);
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            var selectedFiles = new List<string>();
            if (_exportInfo != null && _exportInfo.Files != null)
            {
                foreach (var item in MainGrid.Children)
                {
                    var checkBox = item as CheckBox;
                    if (checkBox != null && checkBox.Name != null && checkBox.Name.Contains("File_") && checkBox.IsChecked != null && checkBox.IsChecked.Value)
                    {
                        var fileName = checkBox.Content as string;
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            selectedFiles.Add(fileName);
                        }
                    }
                }
                if (selectedFiles.Count > 0)
                {
                    using (var fbd = new WinForms.FolderBrowserDialog())
                    {
                        WinForms.DialogResult result = fbd.ShowDialog();
                        if (result == WinForms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        {
                            int i = 0;
                            TitleAction.Content = "Wait, exporting web resources.";
                            ProcessingControlsEnabled(false, SolutionName, SelectSolution, ImportRadio, ExportRadio, Disconnect, Export);
                            ProcessingControlsEnabled(false, "File_");
                            SetMainWindowBackground(Color.FromRgb(243, 240, 215));
                            var path = fbd.SelectedPath;
                            await Task.Run(() =>
                            {
                                foreach (var fileName in selectedFiles)
                                {
                                    var eFile = _exportInfo.Files.Where(ef => ef.Name == fileName).FirstOrDefault();
                                    if (eFile != null)
                                    {
                                        try
                                        {
                                            var bytes = Convert.FromBase64String(eFile.Body);
                                            File.WriteAllBytes($"{path}\\{eFile.Name}", bytes);
                                            i++;
                                        }
                                        catch (Exception ex)
                                        {
                                            
                                        }
                                    }
                                }
                            });
                            if (i > 0)
                            {
                                MessageBox.Show($"{i} files successfully exported to folder '{path}'.");
                            }
                            else
                            {
                                MessageBox.Show($"No files were exported.");
                            }
                        }
                    }
                    TitleAction.Content = "Export action:";
                    ProcessingControlsEnabled(true, SolutionName, SelectSolution, ImportRadio, ExportRadio, Disconnect, Export);
                    ProcessingControlsEnabled(true, "File_");
                    SetMainWindowBackground(Color.FromRgb(213, 240, 222));
                }
                else
                {
                    TitleAction.Content = "Export action:";
                    SetMainWindowBackground(Color.FromRgb(213, 240, 222));
                    MessageBox.Show("No web resources selected.");
                }
            }
        }

        private void ProcessingControlsEnabled(bool flag, string partName)
        {
            foreach (var item in MainGrid.Children)
            {
                var checkBox = item as CheckBox;
                if (checkBox != null && checkBox.Name != null && checkBox.Name.Contains(partName))
                {
                    checkBox.IsEnabled = flag;
                }
            }
        }
    }
}