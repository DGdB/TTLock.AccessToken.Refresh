using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.Configuration;

namespace TTLock.AccessToken.Refresh
{
    internal class Program
    {
        public class ScienerResponse
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public int uid { get; set; }
            public int openid { get; set; }
            public string scope { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
        }
        public static string CreateMD5(string input)
        {
            StringBuilder sb = new StringBuilder();

            // Initialize a MD5 hash object
            using (MD5 md5 = MD5.Create())
            {
                // Compute the hash of the given string
                byte[] hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert the byte array to string format
                foreach (byte b in hashValue)
                {
                    sb.Append($"{b:X2}");
                }
            }

            return sb.ToString();
        
        }

        static void Main(string[] args)
        {
            // vars
            string client_id = ConfigurationManager.AppSettings.Get("client_id");
            string client_secret = ConfigurationManager.AppSettings.Get("client_secret");
            string username = ConfigurationManager.AppSettings.Get("username");
            string password = ConfigurationManager.AppSettings.Get("password");

            // request
            string url = "https://api.sciener.com/oauth2/token";
            var request = (HttpWebRequest)WebRequest.Create(url);

            var postData = "client_id=" + Uri.EscapeDataString(client_id);
            postData += "&client_secret=" + Uri.EscapeDataString(client_secret);
            postData += "&username=" + Uri.EscapeDataString(username);
            postData += "&password=" + Uri.EscapeDataString(CreateMD5(password));
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            // read response
            var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(response.CharacterSet));

            ScienerResponse scienerResponse = JsonSerializer.Deserialize<ScienerResponse>(reader.ReadToEnd());
            reader.Close();
            
            //output
            Console.WriteLine(scienerResponse.access_token);
            Console.ReadKey();
        }
    }
}
