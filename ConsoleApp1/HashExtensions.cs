using System.Security.Cryptography;

public static class HashExtensions
{
    static public string CalculateMD5(this Stream stream) => CalculateHash(stream, MD5.Create());
    static public string CalculateSHA1(this Stream stream) => CalculateHash(stream, SHA1.Create());
    static public string CalculateSHA256(this Stream stream) => CalculateHash(stream, SHA256.Create());

    static private string CalculateHash(Stream stream, HashAlgorithm hashAlgorithm)
    {
        stream.Position = 0;
        var hashBytes = hashAlgorithm.ComputeHash(stream);
        string result = Convert.ToHexStringLower(hashBytes);
        return result;
    }

}
