using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

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

        
        public async Task<ChangesData> GetChangesAsync()
        {
            string url = $"{api_base_url}/getChanges.php?key={key}";
            var request = GetRequestAsync(new Uri(url));
            string successJsonSchema = @"{
                'description': 'Changes data',
                'type':'object',
                'properties':
                {
                    'TextChanges': {'type':'string', 'required':true},
                    'NumericChanges':{'type':'array', 'items':{'type':'integer'}, 'required':true}
                }
            }";

            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);

            var finalRes = ValidateMethodOutput<ChangesData>(schema, result);
            return finalRes;
        }
        public async Task<HomeworkData> GetHomeworkAsync(int ID)
        {
            string url = $"{api_base_url}/getHomework.php?id={ID}&key={key}";
            var request = GetRequestAsync(new Uri(url));
            string successJsonSchema = @"{
                'description': 'Homework data',
                'type':'object',
                'properties':
                {
                    'ID': {'type':'integer', 'required':true},
                    'Subject':{'type':'string', 'required':true},
                    'Homework':{'type':'string', 'required':true}
                }
            }";
            
            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);
            
            var finalRes = ValidateMethodOutput<HomeworkData>(schema, result);
            return finalRes;

        }

        private T ValidateMethodOutput<T>(JSchema schema, JObject result)
        {
            
            if (result.IsValid(schema))
            {
                return result.ToObject<T>();
            }
            else
            {
                
                string errorJsonSchema = @"{
    'description':'Error',
    'type':'object',
    'properties':
    {
       'error':{'type':'string', 'required':true},
       'errorcode':{'type':'integer', 'required':true}
    }
}";
                JSchema errorSchema = JSchema.Parse(errorJsonSchema);
                if (result.IsValid(errorSchema))
                {
                    handleError((string)result.GetValue("error"), (int)result.GetValue("errorcode"));
                    return default(T);
                }
                else
                {
                    throw new BotAPIException("Не удалось обработать ответ сервера");
                }
            }
        }

        public async Task AuthAsync(string username, string password)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user", username),
                new KeyValuePair<string, string>("pass", password)
            });

            var request = PostRequestAsync("/auth.php", content);
            string successJsonSchema = @"{
                'description': 'Homework data',
                'type':'object',
                'properties':
                {
                    'key':{'type':'string', 'required':true}
                }
            }";

            JSchema schema = JSchema.Parse(successJsonSchema);
            JObject result = JObject.Parse(await request);
            key = ValidateMethodOutput<Dictionary<string, string>>(schema, result)["key"];
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
