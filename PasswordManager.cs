using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class PasswordManager
{
    private const string Salt = "static_salt_"; 
    private const string FilePath = "encrypted_password.txt";
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("A1B2C3D4E5F6G7H8");

    public void CreateAndStorePassword(string password)
    {
        var encryptedPassword = EncryptPassword(password);
        File.WriteAllText(FilePath, encryptedPassword);
    }

    public string GetEncryptedPassword()
    {
        return File.ReadAllText(FilePath);
    }

    private string EncryptPassword(string password)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = new byte[16]; 

            var saltedPassword = password + Salt;
            byte[] bytes = Encoding.UTF8.GetBytes(saltedPassword);

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                byte[] encryptedBytes = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                return Convert.ToBase64String(encryptedBytes);
            }
        }
    }
}



