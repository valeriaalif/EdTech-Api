﻿using EdTech.Interfaces;
using MimeKit;
using System.Text;
using MailKit.Net.Smtp;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;
using EdTech.Entities;



namespace EdTech.Utils
{
    public class Tools : ITools
    {

        private readonly IConfiguration _configuration;
        private readonly IBCryptHelper _bCryptHelper;
        private IHostEnvironment _hostingEnvironment;

        public Tools(IConfiguration configuration, IBCryptHelper bCryptHelper, IHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _bCryptHelper = bCryptHelper;
            _hostingEnvironment = hostingEnvironment;
        }

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public bool SendEmail(string recipient, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                string emailSender = _configuration["Email:SenderAddress"];
                string emailSenderPassword = _configuration["Email:SenderPassword"];
                message.From.Add(new MailboxAddress("EdTech Team", emailSender));
                message.To.Add(new MailboxAddress("Recipient", recipient));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = body;

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.office365.com", 587, false);
                    client.Authenticate(emailSender, emailSenderPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }
        public string MakeHtmlNewUser(User userData, string tempPassword)
        {
            try
            {
                string fileRoute = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlTemplates\\registration.html");
                string htmlFile = System.IO.File.ReadAllText(fileRoute);
                htmlFile = htmlFile.Replace("@@nickname", userData.UserName);
                htmlFile = htmlFile.Replace("@@activationCode", tempPassword);
                string hashedId = Encrypt(userData.UserId.ToString());
                string encodedHashedId = HttpUtility.UrlEncode(hashedId);
                htmlFile = htmlFile.Replace("@@Link", "https://localhost:7009/Authentication/ActivateAccount?q=" + encodedHashedId);

                return htmlFile;
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        public string MakeHtmlPassRecovery(User userData, string temporalPassword)
        {
            try
            {
                string fileRoute = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlTemplates\\Contrasenna.html");
                string htmlFile = System.IO.File.ReadAllText(fileRoute);
                htmlFile = htmlFile.Replace("@@Nombre", userData.UserName);
                htmlFile = htmlFile.Replace("@@Correo", userData.UserEmail);
                htmlFile = htmlFile.Replace("@@TemporalPassword", temporalPassword);
                string hashedId = Encrypt(userData.UserId.ToString());
                string encodedHashedId = HttpUtility.UrlEncode(hashedId);
                htmlFile = htmlFile.Replace("@@Link", "https://localhost:7009/Authentication/UpdateNewPassword?q=" + encodedHashedId);


                return htmlFile;
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }
        public string MakeHtmlEmailAdvertisement(string body, string imageUrl)
        {
            try
            {
                string fileRoute = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlTemplates\\EmailAdvertising.html");
                string htmlFile = System.IO.File.ReadAllText(fileRoute);
                htmlFile = htmlFile.Replace("@@Body", body);
                htmlFile = htmlFile.Replace("@@ImageUrl", imageUrl);

                return htmlFile;
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }
        public string GenerateToken(string userId, string user_Type)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("userId", Encrypt(userId)));
            claims.Add(new Claim("userType", Encrypt(user_Type)));



            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("8Tc2nR3QBamz1ipE3b9aYSiTPYoGXQsy"));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void ObtainClaims(IEnumerable<Claim> values, ref string userId, ref string userType, ref bool isAdmin)
        {
            var claims = values.ToDictionary(c => c.Type, c => c.Value);

            if (claims.TryGetValue("userId", out var userIdClaim))
            {
                userId = Decrypt(userIdClaim);
            }

            if (claims.TryGetValue("userType", out var userTypeClaim))
            {
                userType = Decrypt(userTypeClaim);
                isAdmin = userType == "1" || userType == "2";
            }
        }

        public void ObtainClaimsID(IEnumerable<Claim> values, ref string userId)
        {
            var claims = values.ToDictionary(c => c.Type, c => c.Value);

            if (claims.TryGetValue("userId", out var userIdClaim))
            {
                userId = Decrypt(userIdClaim);
            }
        }


        public string Encrypt(string texto)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("x3nbTRq6Jqec3lIZ");
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(texto);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public string Decrypt(string texto)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(texto);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("x3nbTRq6Jqec3lIZ");
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public string GenerateRandomCode(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }




    }
}
