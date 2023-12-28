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
    public  double CalculateSimilarity(string str1, string str2)
    {
        int maxLength = Math.Max(str1.Length, str2.Length);
        int distance = ComputeLevenshteinDistance(str1, str2);

        // Phần trăm giống nhau được tính dựa trên khoảng cách Levenshtein
        double similarity = 1.0 - (double)distance / maxLength;

        return similarity * 100.0; // Chuyển đổi thành phần trăm
    }
    private static int ComputeLevenshteinDistance(string s, string t)
    {
        int[,] distance = new int[s.Length + 1, t.Length + 1];

        for (int i = 0; i <= s.Length; i++)
            distance[i, 0] = i;

        for (int j = 0; j <= t.Length; j++)
            distance[0, j] = j;

        for (int i = 1; i <= s.Length; i++)
        {
            for (int j = 1; j <= t.Length; j++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
            }
        }

        return distance[s.Length, t.Length];
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
