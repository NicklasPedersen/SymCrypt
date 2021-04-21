using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;

namespace SymCrypt
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

        byte[] GenerateRandomBytes(int length)
        {
            using (var crypto = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[length];
                crypto.GetBytes(bytes);
                return bytes;
            }
        }



        byte[] encrypted = null;
        byte[] key = null;
        byte[] IV = null;
        SymmetricAlgorithm algorithm = null;
        private void Encrypt(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ascii_plaintext.Text))
            {
                MessageBox.Show("No plaintext to encrypt");
                return;
            }
            
            // generate the hex plaintext based on the ascii representation
            byte[] plaintext = Encoding.UTF8.GetBytes(ascii_plaintext.Text);
            hex_plaintext.Text = BitConverter.ToString(plaintext).Replace("-", string.Empty);
            algorithm = SymmetricAlgorithm.Create(selection_box.Text);
            algorithm.GenerateKey();
            algorithm.GenerateIV();
            key = algorithm.Key;
            IV = algorithm.IV;
            key_box.Text = Convert.ToBase64String(key);
            IV_box.Text = Convert.ToBase64String(IV);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(plaintext, 0, plaintext.Length);
                }
                encrypted = ms.ToArray();
            }
            sw.Stop();
            encryption_time.Text = sw.Elapsed.TotalMilliseconds.ToString();
            ascii_encrypted.Text = Convert.ToBase64String(encrypted);
            hex_encrypted.Text = BitConverter.ToString(encrypted).Replace("-", string.Empty);
        }

        private void Decrypt(object sender, RoutedEventArgs e)
        {
            if (encrypted == null)
            {
                MessageBox.Show("Nothing to decrypt");
                return;
            }
            string plaintext;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (MemoryStream ms = new MemoryStream(encrypted))
            {
                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        plaintext = sr.ReadToEnd();
                    }
                }
            }
            sw.Stop();
            decryption_time.Text = sw.Elapsed.TotalMilliseconds.ToString();
            ascii_decrypted.Text = plaintext;
            // generate the hex plaintext based on the ascii representation
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            hex_decrypted.Text = BitConverter.ToString(plaintextBytes).Replace("-", string.Empty);
        }
    }
}
