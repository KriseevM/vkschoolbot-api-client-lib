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
        private string apiBaseUrl;
        HttpClient client;
        /// <summary>
        /// Создание объекта API и авторизация
        /// </summary>
        /// <param name="apiBaseUrl">Базовый URL API. Методы будут вызываться по адресу apiBaseUrl/method</param>
        public BotAPI(string apiBaseUrl)
        {

            client = new HttpClient();
            this.apiBaseUrl = apiBaseUrl;

        }
        public async Task UpdateChangesAsync(ChangesData newChanges)
        {
            string url = $"/updateChanges.php";
            JObject contentJObject = JObject.FromObject(newChanges);
            contentJObject.Add("key", key);
            StringContent content = new StringContent(contentJObject.ToString(), Encoding.UTF8, "application/json");
            var request = PostRequestAsync(url, content);
            string successJsonSchema = 
            @"{
                'description': 'Subjects',
                'type':'object',
                'properties':
                {
                    'result': {'type':'boolean', required: true}
                }
            }";
            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);

            ValidateMethodOutput(schema, result);
        }
        public async Task<ScheduleData> GetScheduleAsync()
        {
            string url = $"/getSchedule.php?key={key}";
            var request = GetRequestAsync(url);
            string successJsonSchema = 
            @"{
                'description': 'Subjects',
                'type':'object',
                'properties':
                {
                    'NumericSchedule':
                    {
                        'type': 'array',
                        'items':
                        {
                            'type':'array','items':
                            {
                                'type':'integer'
                            }
                        },
                        'required':true
                    },
                    'TextSchedule':
                    {
                        'type':'array',
                        'items':{'type':'string'}, 
                        'required':true
                    }
                }
            }";

            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);

            var finalRes = ValidateMethodOutput<ScheduleData>(schema, result);
            return finalRes;
        }
        public async Task<Dictionary<int, string>> GetSubjectsAsync()
        {
            string url = $"/getSubjects.php?key={key}";
            var request = GetRequestAsync(url);
            string successJsonSchema = 
            @"{
                'description': 'Subjects',
                'type':'object',
                'patternProperties':
                {
                    '^\\d+$': {'type':'string'}
                },
                'additionalProperties': false
            }";

            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);

            var finalRes = ValidateMethodOutput<Dictionary<int, string>>(schema, result);
            return finalRes;
        } 
        public async Task<ChangesData> GetChangesAsync()
        {
            string url = $"/getChanges.php?key={key}";
            var request = GetRequestAsync(url);
            string successJsonSchema = 
            @"{
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
            string url = $"/getHomework.php?id={ID}&key={key}";
            var request = GetRequestAsync(url);
            string successJsonSchema = 
            @"{
                'description': 'Homework data',
                'type':'object',
                'properties':
                {
                    'ID': {'type':'integer', 'required':true},
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
                
                string errorJsonSchema = 
                @"{
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
                    throw new BotAPIException("Incorrect server response");
                }
            }
        }
        private void ValidateMethodOutput(JSchema schema, JObject result)
        {
            
            if (!result.IsValid(schema))
            {
                
                string errorJsonSchema = 
                @"{
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
                    
                }
                else
                {
                    throw new BotAPIException("Incorrect server response");
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
            string successJsonSchema = 
            @"{
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
                case 8:
                    throw new ServerException(error);
                case 3:
                case 4:
                case 5:
                case 6:
                    throw new AuthenticationException(error);
                case 7:
                    throw new MissingArgumentException(error);
                default:
                    throw new BotAPIException("Unknown error");

            }
        }

        private async Task<string> PostRequestAsync(string url, HttpContent content)
        {
            
            var result = await client.PostAsync(apiBaseUrl+url, content);
            string data = await result.Content.ReadAsStringAsync();
            
            result.EnsureSuccessStatusCode();
            
            return data;

        }
        private async Task<string> GetRequestAsync(string url)
        {
            var result = await client.GetAsync(apiBaseUrl + url);
            string data = await result.Content.ReadAsStringAsync();

            result.EnsureSuccessStatusCode();

            return data;
        }
    }
}
