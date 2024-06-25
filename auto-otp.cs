//This requires FFXIV Quick Luncher to work. You will need your secret key for your OTP.
//This is based on code that someone else created that I used before but couldn't find it again so recreated it using C#.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        // Path to the application
        string appPath = @"XXXXXXXXXXXXXXXXXXXXXXXXXXX";

        // Start the application
        Process process = new Process();
        process.StartInfo.FileName = appPath;
        process.Start();

        // Wait for the application to load
        Thread.Sleep(5000); // Adjust the sleep duration as needed

        // Generate OTP
        string secretKey = "XXXXXXXXXXXXXXXXXXXXXXXXXXXX";
        string otp = GenerateTotp(secretKey);
        //Console.Write(otp);
        // URL to send the OTP
        string url = $"http://127.0.0.1:4646/ffxivlauncher/{otp}";

        // Send the OTP
        SendOtp(url).Wait();
    }

    static string GenerateTotp(string secretKey)
    {
        byte[] key = Base32Decode(secretKey);
        long timeStep = GetCurrentTimeStepNumber();
        byte[] timeStepBytes = BitConverter.GetBytes(timeStep);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeStepBytes); // Convert to big-endian
        }

        using (HMACSHA1 hmac = new HMACSHA1(key))
        {
            byte[] hash = hmac.ComputeHash(timeStepBytes);
            int offset = hash[hash.Length - 1] & 0xf;
            int binaryCode = (hash[offset] & 0x7f) << 24 |
                             (hash[offset + 1] & 0xff) << 16 |
                             (hash[offset + 2] & 0xff) << 8 |
                             (hash[offset + 3] & 0xff);
            int otp = binaryCode % 1000000; // Generate a 6-digit OTP
            return otp.ToString("D6");
        }
    }

    static long GetCurrentTimeStepNumber()
    {
        long unixEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long timeStepSize = 30; // TOTP time step is 30 seconds
        return unixEpoch / timeStepSize;
    }

    static async System.Threading.Tasks.Task SendOtp(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                //Console.WriteLine("OTP sent successfully.");
            }
            else
            {
                //Console.WriteLine($"Failed to send OTP. Status code: {response.StatusCode}");
            }
        }
    }

    static byte[] Base32Decode(string base32)
    {
        // Base32 decoding implementation
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        int[] values = new int[256];
        for (int i = 0; i < alphabet.Length; i++)
        {
            values[alphabet[i]] = i;
        }

        List<byte> bytes = new List<byte>();
        int bits = 0;
        int value = 0;

        foreach (char c in base32)
        {
            value = (value << 5) | values[c];
            bits += 5;

            if (bits >= 8)
            {
                bytes.Add((byte)(value >> (bits - 8)));
                bits -= 8;
            }
        }

        return bytes.ToArray();
    }
}
