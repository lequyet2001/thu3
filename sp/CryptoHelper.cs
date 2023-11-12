using System;
using System.Security.Cryptography;
using System.Text;

public class CryptoHelper
{
    private static RandomNumberGenerator rng = RandomNumberGenerator.Create();

   
    // Hàm tạo ID ngẫu nhiên
    public  string GenerateRandomId(int length = 10)
    {
        byte[] randomBytes = new byte[length];
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Substring(0, length);
    }

    // Hàm mã hóa theo chuỗi truyền vào
    public  string Encrypt(string plaintext, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = keyBytes;
            aesAlg.GenerateIV();
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using (var msEncrypt = new System.IO.MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plaintext);
                    }
                    byte[] encryptedBytes = msEncrypt.ToArray();
                    byte[] combinedBytes = new byte[aesAlg.IV.Length + encryptedBytes.Length];
                    Array.Copy(aesAlg.IV, 0, combinedBytes, 0, aesAlg.IV.Length);
                    Array.Copy(encryptedBytes, 0, combinedBytes, aesAlg.IV.Length, encryptedBytes.Length);
                    return Convert.ToBase64String(combinedBytes);
                }
            }
        }
    }

    // Hàm giải mã
    public string Decrypt(string encryptedText, string key)
    {
        byte[] combinedBytes = Convert.FromBase64String(encryptedText);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] iv = new byte[16];
        byte[] encryptedBytes = new byte[combinedBytes.Length - iv.Length];
        Array.Copy(combinedBytes, 0, iv, 0, iv.Length);
        Array.Copy(combinedBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = keyBytes;
            aesAlg.IV = iv;
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using (var msDecrypt = new System.IO.MemoryStream(encryptedBytes))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
    
}
