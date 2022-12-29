using System.Security.Cryptography;
using System.Text;

namespace SmartContract;

public class Calculation
{
    public Miner Miner     { get; set; }
    public User Requester  { get; set; }
    public string Data     { get; set; }
    public string Result   { get; set; }

    public bool Valid()
    {
        if (Result != ComputeSha256Hash(Data))
            return false;

        return Result.Substring(0, 3) == "000";
    }
    
    static string ComputeSha256Hash(string rawData)
    {
        var sha256Hash = SHA256.Create();  
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));  
  
        var builder = new StringBuilder();
        foreach (var byteData in bytes)
            builder.Append(byteData.ToString("x2"));
        
        return builder.ToString();
    }  
}