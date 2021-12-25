using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SteamrequestsTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            var rsa = new Rsa();
            string pass = "pass";

            WebClient client = new WebClient();
            string jsonKey = client.DownloadString("https://steamcommunity.com/login/getrsakey/?username=brusdamson");
            var rsaKey = JsonConvert.DeserializeObject<JsonRsaKeyModel>(jsonKey);



            rsa.Modulus = rsaKey.publickey_mod;
            rsa.Exponent = rsaKey.publickey_exp;

            string encrypted = rsa.Encrypt(pass);

            Console.WriteLine("Key: {0}",encrypted);

            //Для кук
            HttpClientHandler handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };

            var answer = PostDefault(handler,encrypted, rsaKey.timestamp).Result;
            IEnumerable<string> cookies = answer.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            string json = answer.Content.ReadAsStringAsync().Result;
            object a = JsonConvert.DeserializeObject(json);
            string b = JsonConvert.SerializeObject(a, Formatting.Indented);
            Console.WriteLine(b);
            var responseLogin = JsonConvert.DeserializeObject<ResponseLoginModel>(json);
            //проверка на капчу
            while (responseLogin.success != true)
            {
                if (responseLogin.captcha_needed)
                {

                    Console.WriteLine($"Ссылка на капчу: https://steamcommunity.com/login/rendercaptcha/?gid={responseLogin.captcha_gid}");
                    Console.WriteLine("Введите капчу:");
                    string captcha_key = Console.ReadLine();

                    //Отправка нового запроса, но с решенной уже капчей
                    answer = PostWithCaptcha(handler,encrypted, rsaKey.timestamp, responseLogin.captcha_gid, captcha_key).Result;
                    cookies = answer.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
                    json = answer.Content.ReadAsStringAsync().Result;
                    a = JsonConvert.DeserializeObject(json);
                    b = JsonConvert.SerializeObject(a, Formatting.Indented);
                    responseLogin = JsonConvert.DeserializeObject<ResponseLoginModel>(json);
                    Console.WriteLine(b);

                }
                else if (responseLogin.emailauth_needed)
                {
                    Console.WriteLine($"Введите код Steam Guard. Отправлено на почту.");
                    Console.Write("Steam Guard код: "); 
                    string steam_guard_code = Console.ReadLine();

                    answer = PostWithEmail(handler,encrypted,rsaKey.timestamp,responseLogin.emailsteamid, steam_guard_code).Result;
                    cookies = answer.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
                    json = answer.Content.ReadAsStringAsync().Result;
                    a = JsonConvert.DeserializeObject(json);
                    b = JsonConvert.SerializeObject(a, Formatting.Indented);
                    responseLogin = JsonConvert.DeserializeObject<ResponseLoginModel>(json);
                    Console.WriteLine(b);
                }
            }
            var ans = PostTestCookie(handler).Result;
            Console.WriteLine(ans);
            Console.ReadLine();
        }
        //Если капча
        //"success": false,
        //"message": "Please verify your humanity by re-entering the characters in the captcha.",
        //"requires_twofactor": false,
        //"captcha_needed": true,
        //"captcha_gid": "4192286929931134206"

        //Если эмейл
        //"success": false,
        //"requires_twofactor": false,
        //"message": "",
        //"emailauth_needed": true,
        //"emaildomain": "gmail.com",
        //"emailsteamid": "76561198808411721"
        private static async Task<HttpResponseMessage> PostTestCookie(HttpClientHandler handler)
        {
            HttpClient client = new HttpClient(handler);

            var dict = new Dictionary<string, string>();
            dict.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            dict.Add("X-Requested-With", "XMLHttpRequest");

            return await client.PostAsync("https://steamcommunity.com/tradeoffer/new/send", new FormUrlEncodedContent(dict));
        }
        /// <summary>
        /// Обычная авторизация
        /// </summary>
        /// <param name="encrypted">Зашифрованный RSA ключём пароль</param>
        /// <param name="timeStamp">Time Stamp полученный по адресу https://steamcommunity.com/login/getrsakey/?username=username</param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> PostDefault(HttpClientHandler handler, string encrypted, string timeStamp)
        {
            HttpClient client = new HttpClient(handler);

            var dict = new Dictionary<string, string>();
            dict.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            dict.Add("X-Requested-With", "XMLHttpRequest");
            dict.Add("password", $"{encrypted}");
            dict.Add("username", $"brusdamson");
            dict.Add("captchagid", $"-1");
            dict.Add("rsatimestamp", $"{timeStamp}");
            dict.Add("remember_login", $"true");
            dict.Add("tokentype", $"-1");

            
            return await client.PostAsync("https://steamcommunity.com/login/dologin/", new FormUrlEncodedContent(dict));
        }
        /// <summary>
        /// Авторизация с капчей
        /// </summary>
        /// <param name="encrypted">Зашифрованный RSA ключем пароль</param>
        /// <param name="timeStamp">Time Stamp полученный по адресу https://steamcommunity.com/login/getrsakey/?username=username</param>
        /// <param name="captcha_gid">Gid капчи</param>
        /// <param name="captcha_text">Решенная капча</param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> PostWithCaptcha(HttpClientHandler handler, string encrypted,string timeStamp,string captcha_gid, string captcha_text)
        {
            HttpClient client = new HttpClient(handler);

            var dict = new Dictionary<string, string>();
            dict.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            dict.Add("X-Requested-With", "XMLHttpRequest");
            dict.Add("password", $"{encrypted}");
            dict.Add("username", $"brusdamson");
            dict.Add("captchagid", $"{captcha_gid}");
            dict.Add("rsatimestamp", $"{timeStamp}");
            dict.Add("remember_login", $"true");
            dict.Add("tokentype", $"-1");
            dict.Add("captcha_text", $"{captcha_text}");

            return await client.PostAsync("https://steamcommunity.com/login/dologin/", new FormUrlEncodedContent(dict));
        }
        /// <summary>
        /// Запрос с кодом отправленным на Email
        /// </summary>
        /// <param name="encrypted">Зашифрованный RSA ключем пароль</param>
        /// <param name="timeStamp">Time Stamp полученный по адресу https://steamcommunity.com/login/getrsakey/?username=username</param>
        /// <param name="emailsteamid">Id сообщения с кодом отправленным на Email</param>
        /// <param name="emailauth">Полученный код с Emailt</param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> PostWithEmail(HttpClientHandler handler, string encrypted, string timeStamp, string emailsteamid, string emailauth)
        {
            HttpClient client = new HttpClient(handler);

            var dict = new Dictionary<string, string>();
            dict.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            dict.Add("X-Requested-With", "XMLHttpRequest");
            dict.Add("password", $"{encrypted}");
            dict.Add("username", $"brusdamson");
            dict.Add("captchagid", $"-1");
            dict.Add("rsatimestamp", $"{timeStamp}");
            dict.Add("remember_login", $"true");
            dict.Add("emailsteamid", $"{emailsteamid}");
            dict.Add("emailauth", $"{emailauth}");
            dict.Add("tokentype", $"-1");

            return await client.PostAsync("https://steamcommunity.com/login/dologin/", new FormUrlEncodedContent(dict));
        }

        /// <summary>
        /// Запрос на трейд ДОДЕЛАТЬ!!!!
        /// </summary>
        /// <param name="encrypted"></param>
        /// <param name="timeStamp"></param>
        /// <param name="emailsteamid"></param>
        /// <param name="emailauth"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> PostSendTrade(HttpClientHandler handler, string encrypted, string timeStamp, string emailsteamid, string emailauth)
        {
            HttpClient client = new HttpClient(handler);

            var dict = new Dictionary<string, string>();
            dict.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            dict.Add("X-Requested-With", "XMLHttpRequest");
            dict.Add("password", $"{encrypted}");
            dict.Add("username", $"brusdamson");
            dict.Add("captchagid", $"-1");
            dict.Add("rsatimestamp", $"{timeStamp}");
            dict.Add("remember_login", $"true");
            dict.Add("emailsteamid", $"{emailsteamid}");
            dict.Add("emailauth", $"{emailauth}");
            dict.Add("tokentype", $"-1");

            return await client.PostAsync("https://steamcommunity.com/tradeoffer/new/send", new FormUrlEncodedContent(dict));
        }

    }
}
