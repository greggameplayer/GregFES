using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Text;

namespace GregFES
{
    public class CryptingSystem : ICryptingSystem
    {
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr destination, int Length);

        public static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        public static string CalculateSHA256(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        public void FileEncrypt(string inputFile, string password, type typeValue)
        {
            byte[] salt = GenerateRandomSalt();
            byte[] cryptType = new byte[4];

            if(typeValue == type.Password)
            {
                cryptType = Encoding.UTF8.GetBytes("pass");
            }
            else if(typeValue == type.File)
            {
                cryptType = Encoding.UTF8.GetBytes("file");
            }

            FileStream fsCrypt = new FileStream(inputFile + ".greg", FileMode.Create);

            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            AES.Mode = CipherMode.CBC;

            fsCrypt.Write(salt, 0, salt.Length);
            fsCrypt.Write(cryptType, 0, cryptType.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            byte[] buffer = new byte[1048576];

            int read;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Application.DoEvents();
                    cs.Write(buffer, 0, read);
                }

                fsIn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
            }
            finally
            {
                cs.Close();
                fsCrypt.Close();
                File.Delete(inputFile);
                MessageBox.Show("Votre fichier a été correctement chiffré !", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void FileDecrypt(string inputFile, string outputFile, string password)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];
            byte[] cryptType = new byte[4];

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);
            fsCrypt.Read(cryptType, 0, cryptType.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CBC;

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];

            try
            {
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Application.DoEvents();
                    fsOut.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                Console.WriteLine($"Cryptographic error : {ex_CryptographicException.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
            }

            try
            {
                cs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error by closing CryptoStream : {ex.Message}");
            }
            finally
            {
                fsOut.Close();
                fsCrypt.Close();

                if (File.ReadAllText(outputFile) == "")
                {
                    File.Delete(outputFile);
                    MessageBox.Show("Le mot de passe est incorrect !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    File.Delete(inputFile);
                    MessageBox.Show("Votre fichier a été correctement déchiffré !", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public type FileDecryptTry(string inputFile)
        {
            byte[] salt = new byte[32];
            byte[] cryptType = new byte[4];

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);
            fsCrypt.Read(cryptType, 0, cryptType.Length);

            fsCrypt.Close();

            if(Encoding.UTF8.GetString(cryptType) == "pass")
            {
                return type.Password;
            }
            else if(Encoding.UTF8.GetString(cryptType) == "file")
            {
                return type.File;
            }
            return type.None;
        }
    }
}
