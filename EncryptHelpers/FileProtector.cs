using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace BitProtector.EncryptHelpers
{
    class FileProtector
    {
        public static void EncryptFile(string inputFilePath, string outputFilePath, SecureString password)
        {
            try
            {
                string encryptedFilePath = Path.ChangeExtension(outputFilePath, "bin");

                byte[] salt = GenerateRandomSalt();
                byte[] derivedKey = DeriveKeyFromPassword(password, salt);

                using Aes aesAlg = Aes.Create();
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.GenerateIV();
                aesAlg.Key = derivedKey;

                // Encrypt the file content and add metadata in one step
                using (FileStream inputFileStream = new FileStream(inputFilePath, FileMode.Open))
                using (FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Create))
                using (CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    // Write IV and salt to the beginning of the encrypted file
                    encryptedFileStream.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    encryptedFileStream.Write(salt, 0, salt.Length);

                    // Add the original file extension as metadata
                    string originalExtension = Path.GetExtension(inputFilePath);
                    byte[] extensionBytes = Encoding.UTF8.GetBytes(originalExtension);
                    encryptedFileStream.Write(extensionBytes, 0, extensionBytes.Length);

                    // Encrypt the file content
                    inputFileStream.CopyTo(cryptoStream);
                }

                //Finished
                MessageBox.Show("File encrypted", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        public static void DecryptFile(string encryptedFilePath, string decryptedFilePath, SecureString password)
        {
            try
            {
                byte[] iv;
                byte[] salt;
                byte[] extensionBytes;
                string originalExtension;

                using (FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Open))
                {
                    iv = new byte[16]; // IV length
                    encryptedFileStream.Read(iv, 0, iv.Length);

                    // Read and restore the salt from metadata
                    salt = new byte[16]; // Salt length
                    encryptedFileStream.Read(salt, 0, salt.Length);

                    // Read and restore the original file extension from metadata
                    extensionBytes = new byte[4]; // Choose an appropriate size
                    encryptedFileStream.Read(extensionBytes, 0, extensionBytes.Length);
                    originalExtension = Encoding.UTF8.GetString(extensionBytes);

                    byte[] derivedKey = DeriveKeyFromPassword(password, salt);

                    using Aes aesAlg = Aes.Create();
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Key = derivedKey;
                    aesAlg.IV = iv;

                    // Decrypt the file content
                    using (FileStream decryptedFileStream = new FileStream(decryptedFilePath, FileMode.Create))
                    using (CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, aesAlg.CreateDecryptor(), CryptoStreamMode.Read)) // Use CryptoStreamMode.Read
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            decryptedFileStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }

                // Rename the decrypted file with the original extension
                string decryptedFilePathWithExtension = Path.ChangeExtension(decryptedFilePath, originalExtension);
                File.Move(decryptedFilePath, decryptedFilePathWithExtension);

                // Finished
                MessageBox.Show("File decrypted", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private static byte[] GenerateRandomSalt()
        {
            byte[] salt = new byte[16]; // Salt length
            using RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(salt);
            return salt;
        }


        private static byte[] DeriveKeyFromPassword(SecureString password, byte[] salt)
        {
            using Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetString(ProtectedData.Protect(Encoding.UTF8.GetBytes(password.ToInsecureString()), null, DataProtectionScope.CurrentUser)),
                salt,
                iterations: 10000,
                HashAlgorithmName.SHA256
            );
            return deriveBytes.GetBytes(32); // Key length
        }
    }


    // Utility extension method to convert SecureString to regular string (not recommended in production)
    public static class SecureStringExtensions
    {
        public static string ToInsecureString(this SecureString secureString)
        {
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(secureString);
            try
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
        }


        public static SecureString ConvertToSecureString(this string input)
        {
            SecureString secureString = new SecureString();

            foreach (char c in input)
            {
                secureString.AppendChar(c);
            }

            return secureString;
        }
    }
}
