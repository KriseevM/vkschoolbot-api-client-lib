using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace SchoolBotAPI
{
    public class BotAPI
    {
        private string key;
        private string api_base_url;
        HttpClient client;
        /// <summary>
        /// Создание объекта API и авторизация
        /// </summary>
        /// <param name="api_base_url">Базовый URL API. Методы будут вызываться по адресу api_url/method</param>
        /// <param name="username">Имя пользователя для авторизации</param>
        /// <param name="password">Пароль пользователя для авторизации</param>
        public BotAPI(string api_base_url)
        {

            client = new HttpClient();
            this.api_base_url = api_base_url;

        }



        public async Task AuthAsync(string username, string password)
        {



            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user", username),
                new KeyValuePair<string, string>("pass", password)
            });

            string data = await PostRequestAsync("/auth.php", content);
            
            Dictionary<string, string> decodedData = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            if (decodedData.ContainsKey("error"))
            {
                switch (decodedData["errorcode"])
                {
                    case "1":
                    case "2":
                        throw new ServerException(decodedData["error"]);
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                        throw new AuthenticationException(decodedData["error"]);
                    case "7":
                        throw new MissingArgumentException(decodedData["error"]);
                    default:
                        throw new BotAPIException("Произошла неизвестная ошибка");

                }
            }
            else if (decodedData.ContainsKey("key"))
            {
                key = decodedData["key"];
            }
            else
            {
                throw new BotAPIException("Не удалось распознать ответ сервера: " + data);
            }
        }

        private async Task<string> PostRequestAsync(string url, FormUrlEncodedContent content)
        {
            
            var result = await client.PostAsync(api_base_url+url, content);
            string data = await result.Content.ReadAsStringAsync();
            
            result.EnsureSuccessStatusCode();
            
            return data;

        }
    }
}
