using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GregFES
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Chiffrer_Click(object sender, RoutedEventArgs e)
        {
            string password;
            type typeValue;
            CryptingSystem cs = new CryptingSystem();
            if (KeyFileDropZone.IsEnabled)
            {
                password = CryptingSystem.CalculateSHA256(LabelKeyFile.Content.ToString());
                typeValue = type.File;
            }
            else
            {
                password = PasswordCrypt.Password;
                typeValue = type.Password;
            }

            if (password != "" || FilePathCrypt.Text != "")
            {
                GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

                cs.FileEncrypt(FilePathCrypt.Text, password, typeValue);

                CryptingSystem.ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
                gch.Free();
            }
            else
            {
                MessageBox.Show("All the fields have to be filled !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Dechiffrer_Click(object sender, RoutedEventArgs e)
        {
            string password;
            CryptingSystem cs = new CryptingSystem();
            if (KeyFileDropZoneDecrypt.Visibility == Visibility.Visible)
            {
                password = CryptingSystem.CalculateSHA256(LabelKeyFileDecrypt.Content.ToString());
            }
            else
            {
                password = PasswordDecrypt.Password;
            }

            if (password != "" || FilePathDecrypt.Text != "")
            {
                GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

                cs.FileDecrypt(FilePathDecrypt.Text, ReplaceLastOccurrence(FilePathDecrypt.Text, ".greg", ""), password);

                CryptingSystem.ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
                gch.Free();
            }
            else
            {
                MessageBox.Show("All the fields have to be filled !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LabelPasswordDecrypt.Visibility = Visibility.Hidden;
            PasswordDecrypt.Visibility = Visibility.Hidden;
            LabelKeyFilePanel_DropDecrypt.Visibility = Visibility.Hidden;
            KeyFileDropZoneDecrypt.Visibility = Visibility.Hidden;
            FilePathDecrypt.Text = "";
        }

        private void Dechiffrer_Parcourir_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog;
            CreateOpenFileDialog(out fileDialog, "GregFES crypted files | *.greg");
            FilePathDecrypt.Text = (bool)fileDialog.ShowDialog() ? fileDialog.FileName : "";
            if (FilePathDecrypt.Text != "")
            {
                GetCryptMethod(FilePathDecrypt.Text);
            }
        }

        private void GetCryptMethod(string inputFile)
        {
            CryptingSystem cs = new CryptingSystem();
            type cryptType = cs.FileDecryptTry(inputFile);

            if(cryptType == type.Password)
            {
                LabelPasswordDecrypt.Visibility = Visibility.Visible;
                PasswordDecrypt.Visibility = Visibility.Visible;
                LabelKeyFilePanel_DropDecrypt.Visibility = Visibility.Hidden;
                KeyFileDropZoneDecrypt.Visibility = Visibility.Hidden;
            } 
            else if(cryptType == type.File)
            {
                LabelPasswordDecrypt.Visibility = Visibility.Hidden;
                PasswordDecrypt.Visibility = Visibility.Hidden;
                LabelKeyFilePanel_DropDecrypt.Visibility = Visibility.Visible;
                KeyFileDropZoneDecrypt.Visibility = Visibility.Visible;
            }
        }

        private void Chiffrer_Parcourir_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog;
            CreateOpenFileDialog(out fileDialog, "");
            FilePathCrypt.Text = (bool)fileDialog.ShowDialog() ? fileDialog.FileName : "";
        }

        static string ReplaceLastOccurrence(string str, string toReplace, string replacement)
        {
            return Regex.Replace(str, $@"^(.*){toReplace}(.*?)$", $"$1{replacement}$2");
        }

        private void CreateOpenFileDialog(out OpenFileDialog fileDialog, string Filters)
        {
            fileDialog = new OpenFileDialog();
            fileDialog.Filter = Filters;
        }

        private void KeyFilePanel_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                LabelKeyFile.Content = files[0];
            }
        }

        private void KeyFilePanel_DropDecrypt(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                LabelKeyFileDecrypt.Content = files[0];
            }
        }

        private void isRadioBtPasswordChecked(object sender, RoutedEventArgs e)
        {
            PasswordCrypt.IsEnabled = true;
            KeyFileDropZone.IsEnabled = false;
        }

        private void isRadioBtFileKeyChecked(object sender, RoutedEventArgs e)
        {
            PasswordCrypt.IsEnabled = false;
            KeyFileDropZone.IsEnabled = true;
        }

        private void Input_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void Input_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                if ((TextBox)(e.Source) == FilePathDecrypt)
                {
                    if (files[0].EndsWith(".greg"))
                    {
                        ((TextBox)(e.Source)).Text = files[0];
                        GetCryptMethod(((TextBox)(e.Source)).Text);
                    }
                    else
                    {
                        MessageBox.Show("Vous devez glisser/déposer un fichier .greg !", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    if (files[0].EndsWith(".greg"))
                    {
                        MessageBox.Show("Vous ne pouvez pas glisser/déposer un fichier .greg !", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        ((TextBox)(e.Source)).Text = files[0];
                    }
                }
            }
        }

        private void Input_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
    }
}
