   M S B A M L   `   `   `  ����O  KWebResourcesImporter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:WebResourcesImporterWebResourcesImporter  5   	   %    WebResourcesImporter.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   4local"clr-namespace:WebResourcesImporter     5   	     � Title$  ImpExpWebResources��5   	   $	��600��6$   $	��600��61   ���	#FFF3D8D7.��5	      �� .��5
   
   � -   5         �Name$ MainGrid��5
      .+�5      �� -   5         ��Name$ SOAPServiceUriLabel��5      $��SOAP service uri:��61   $
��Left=�6M   $��	10,10,0,0q�6h   $	��Top=�6{   �� 5      -   5         ��Name$ SOAPServiceUri��5      $
��Left=�6.   $��23��6I   $��	10,36,0,0q�6U    �� TextWrapping$
 Wrap=�6h   $�� ��6|   $	��Top=�6�   $	��195��6�   �� 5      -   5        $ UserNameLabel��5      $��	Username:��6+   $
��Left=�6?   $��	10,61,0,0q�6Z   $	��Top=�6m   �� 5      -   5        $ UserName��5      $
��Left=�6(   $��23��6C   $��	10,87,0,0q�6O   $
 Wrap=�6b   $�� ��6v   $	��Top=�6~   $	��195��6�   �� 5      -   5        $ PasswordLabel��5      $��	Password:��6+   $
��Left=�6?   $��
10,110,0,0q�6Z   $	��Top=�6n   $	��120��6�   G� 5      -   5         G�Name$ Password��5      $
��Left=�6,   $��23��6G   $��
10,135,0,0q�6S   $	��Top=�6g   $	��195��6   �� 5      -   5         ��Name$ Connect��5      $��Connect to CRM��6&   $
��Left=�6?   $��
10,163,0,0q�6Z   $	��Top=�6n   $	��195��6�   $��23��6�   6�   �� 5      -	   5        $ Import��5   )   $y�Hidden=�6   $$��Select files and import to CRM��69   $
��Left=�6b   $��
10,245,0,0q�6}   $	��Top=�6�   $	��195��6�   $��34��6�   �� 5      -
   5        $ 
Disconnect��5   )   $y�Hidden=�6   $��Disconnect from CRM��6=   $��23��6[   $
��Left=�6g   $��
10,191,0,0q�6�   $	��Top=�6�   $	��195��6�   6�   �� 5      -   5        $ SolutionNameLabel��5   (   $y�Hidden=�6   $��Solution name:��6C   $
��Left=�6\   $��
227,11,0,0q�6w   $	��Top=�6�   �� 5      -   5        $ SolutionName��5   Q   $y�Hidden=�6=   $
��Left=�6g   $��23��6�   $��
226,36,0,0q�6�   $
 Wrap=�6�   $�� ��6�   $	��Top=�6�   $	��195��6�   �� 5      -   5        $ LoadingLabel��5   (   $y�Hidden=�6   $)��#Wait, the files are being imported.��6>   $
��Left=�6l   $��
10,279,0,0q�6�   $	��Top=�6�   �� 5      -   5         ��Name$ OverwriteFilesCheckBox��5      $��Overwrite files��6�   $
��Left=�6�   $��227,244,0,0q�6�   $	��Top=�6�   $��15��6�   $��98��6�   $y�Hidden=�6  �� 5      -   5        $! ChangeTheCharactersCheckBox��5      $4��.Change the characters in the name if necessary��6�   $
��Left=�6�   $��227,266,0,0q�6�   $	��Top=�6  $��21��6  $	��278��6%  $y�Hidden=�61  �� 5      -   5        $ SettingsImport��5   (   $y�Hidden=�6   $��Setting for import:��6@   $
��Left=�6^   $��227,219,0,0q�6y   $	��Top=�6�   �� 5      -   5        $ SelectSolution��5   )   $y�Hidden=�6   $��Select solution��6`   $
��Left=�6z   $��
227,64,0,0q�6�   $	��Top=�6�   $	��194��6�   $��23��6�   	� 5      -   5         	�Name$ ImportRadio��5   T   $y�Hidden=�6   	 d� 	IsChecked$
	 TrueY�6i   $��Import��6z   $
��Left=�6�   $	��Top=�6�   $��227,140,0,0q�6�   	� 5      -   5        $ ExportRadio��5   T   $y�Hidden=�6   $��Export��6i   $
��Left=�6z   $��227,167,0,0q�6�   $	��Top=�6�   �� 5      -   5        $ SelectActionLabel��5   (   $y�Hidden=�6   $��Select action:��6C   $
��Left=�6\   $��227,110,0,0q�6w   $	��Top=�6�   �� 5      -   5        $ TitleAction��5      $��Import action:��6)   $
��Left=�6B   $��
10,219,0,0q�6]   $	��Top=�6q   
 J� RenderTransformOrigin$
 -0.422,0.097&�6�   $y�Hidden=�6�   �� 5      -   5         ��Name$
 Info��5   *   $y�Hidden=�6   $
��Left=�68   $	��100��6S   $��
10,287,0,0q�6`   $	��Top=�6t   $	��489��6�   �� 5       -   5        $ Export��5    >   $y�Hidden=�6*   $��Export��6N   $
��Left=�6_   $��
10,245,0,0q�6z   $	��Top=�6�   $	��195��6�   $��34��6�   5!      5"      5#      