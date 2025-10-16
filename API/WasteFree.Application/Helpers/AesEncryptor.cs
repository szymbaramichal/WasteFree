using System.Security.Cryptography;

namespace WasteFree.Business.Helpers;

public static class AesEncryptor
{
    private static byte[] GetKey(string password, byte[] salt)
    {
        using var rfc2898 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        return rfc2898.GetBytes(32);
    }

    public static string Encrypt(string plainText, string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        using var aes = Aes.Create();
        aes.Key = GetKey(password, salt);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        ms.Write(salt, 0, salt.Length);
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
            sw.Write(plainText);

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText, string password)
    {
        byte[] allBytes = Convert.FromBase64String(cipherText);

        byte[] salt = new byte[16];
        byte[] iv = new byte[16];
        Array.Copy(allBytes, 0, salt, 0, 16);
        Array.Copy(allBytes, 16, iv, 0, 16);

        using var aes = Aes.Create();
        aes.Key = GetKey(password, salt);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(allBytes, 32, allBytes.Length - 32);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}