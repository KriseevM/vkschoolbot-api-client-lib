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
        public BotAPI(string api_base_url)
        {

            client = new HttpClient();
            this.api_base_url = api_base_url;

        }

        

        public async Task<HomeworkData> GetHomeworkAsync(int ID)
        {
            string url = $"{api_base_url}/getHomework.php?id={ID}&key={key}";
            Console.WriteLine(url);
            var result = JsonConvert.DeserializeObject<HomeworkData>(await GetRequestAsync(new Uri(url)));
             
            return result;
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
                handleError(decodedData);
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

        private static void handleError(Dictionary<string, string> decodedData)
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
        private static void handleError(string error, int errorcode)
        {
            switch (errorcode)
            {
                case 1:
                case 2:
                    throw new ServerException(error);
                case 3:
                case 4:
                case 5:
                case 6:
                    throw new AuthenticationException(error);
                case 7:
                    throw new MissingArgumentException(error);
                default:
                    throw new BotAPIException("Произошла неизвестная ошибка");

            }
        }

        private async Task<string> PostRequestAsync(string url, FormUrlEncodedContent content)
        {
            
            var result = await client.PostAsync(api_base_url+url, content);
            string data = await result.Content.ReadAsStringAsync();
            
            result.EnsureSuccessStatusCode();
            
            return data;

        }
        private async Task<string> GetRequestAsync(string url)
        {
            var result = await client.GetAsync(api_base_url + url);
            string data = await result.Content.ReadAsStringAsync();

            result.EnsureSuccessStatusCode();

            return data;
        }
        private async Task<string> GetRequestAsync(Uri uri)
        {
            var result = await client.GetAsync(uri);
            string data = await result.Content.ReadAsStringAsync();

            result.EnsureSuccessStatusCode();

            return data;
        }
    }
}
