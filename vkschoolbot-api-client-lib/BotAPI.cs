﻿using System;
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
        public async Task<bool> UpdateChangesAsync(ChangesData newChanges)
        {
            string url = $"/updateChanges.php";
            StringContent content = new StringContent(JsonConvert.SerializeObject(newChanges), Encoding.UTF8, "application/json");
            var request = PostRequestAsync(url, content);
            string successJsonSchema = 
            @"{
                'type':'object',
                'properties':
                {
                    'updated': {'type':'boolean', required: true}
                }
            }";
            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);
            ValidateMethodOutput(schema, result);
            return result.Value<bool>("updated");
        }
        public async Task<bool> UpdateHomeworkAsync(HomeworkData newHomework)
        {
            string url = $"/updateHomework.php";
            StringContent content = new StringContent(JsonConvert.SerializeObject(newHomework), Encoding.UTF8, "application/json");
            var request = PostRequestAsync(url, content);
            string successJsonSchema = 
            @"{
                'type':'object',
                'properties':
                {
                    'updated': {'type':'boolean', required: true}
                }
            }";
            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);
            ValidateMethodOutput(schema, result);
            return result.Value<bool>("updated");
        }
        public async Task<bool> UpdateTimetableAsync(TimetableData newTimetable)
        {
            string url = $"/updateSchedule.php";
            StringContent content = new StringContent(JsonConvert.SerializeObject(newTimetable), Encoding.UTF8, "application/json");
            var request = PostRequestAsync(url, content);
            string successJsonSchema = 
            @"{
                'type':'object',
                'properties':
                {
                    'updated': {'type':'boolean', required: true}
                }
            }";
            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);
            ValidateMethodOutput(schema, result);
            return result.Value<bool>("updated");
        }
        public async Task<TimetableData> GetTimetableAsync()
        {
            string url = $"/getSchedule.php";
            var request = GetRequestAsync(url);
            string successJsonSchema = 
            @"{
                'type':'object',
                'properties':
                {
                    'NumericTimetable':
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
                    'TextTimetable':
                    {
                        'type':'array',
                        'items':{'type':'string'}, 
                        'required':true
                    }
                }
            }";

            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);
            var finalRes = ValidateMethodOutput<TimetableData>(schema, result);
            return finalRes;
        }
        public async Task<SubjectListEntry[]> GetSubjectsAsync()
        {
            string url = $"/getSubjects.php";
            var request = GetRequestAsync(url);
            string successJsonSchema = 
            @"{
                'type':'array',
                'items':
                {
                    'type':'object',
                    'properties': {
                        'ID': {
                            'type':'integer',
                            'required':true
                        },
                        'Name': {
                            'type': 'string',
                            'required': true
                        }
                    }
                }
            }";

            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JArray.Parse(await request);
            var finalRes = ValidateMethodOutput<SubjectListEntry[]>(schema, result);
            return finalRes;
        } 
        public async Task<ChangesData> GetChangesAsync()
        {
            string url = $"/getChanges.php";
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
            string url = $"/getHomework.php?id={ID}";
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
        public async Task<int> AddSubjectsAsync(params string[] subjectNames)
        {
            string url = $"/addSubjects.php";
            JObject contentJObject = new JObject();
            contentJObject.Add("names", JArray.FromObject(subjectNames));
            string contentString = contentJObject.ToString();
            StringContent content = new StringContent(contentString, Encoding.UTF8, "application/json");
            var request = PostRequestAsync(url, content);
            string successJsonSchema =
            @"{
                'type':'object',
                'properties':
                {
                    'added_subjects': {'type':'integer', required: true}
                }
            }";
            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);
            ValidateMethodOutput(schema, result);
            return result.Value<int>("added_subjects"); 
        }
        public async Task<int> DeleteSubjectsAsync(params int[] subjectIDs)
        {
            string url = $"/deleteSubjects.php";
            JObject contentJObject = new JObject();
            contentJObject.Add("IDs", JArray.FromObject(subjectIDs));
            string contentString = contentJObject.ToString();
            StringContent content = new StringContent(contentString, Encoding.UTF8, "application/json");
            var request = PostRequestAsync(url, content);
            string successJsonSchema =
            @"{
                'type':'object',
                'properties':
                {
                    'deleted_subjects': {'type':'integer', required: true}
                }
            }";
            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);
            ValidateMethodOutput(schema, result);
            return result.Value<int>("deleted_subjects");
        }
        public async Task<bool> AddUserAsync(string user, string pass, int pr_level)
        {
            string url = $"/addUser.php";
            JObject contentJObject = new JObject();
            contentJObject.Add("user", user);
            contentJObject.Add("pass", pass);
            contentJObject.Add("pr", pr_level);
            string contentString = contentJObject.ToString();
            StringContent content = new StringContent(contentString, Encoding.UTF8, "application/json");
            var request = PostRequestAsync(url, content);
            string successJsonSchema =
            @"{
                'type':'object',
                'properties':
                {
                    'added': {'type':'boolean', required: true}
                }
            }";
            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);
            ValidateMethodOutput(schema, result);
            return result.Value<bool>("added");
        }
        public async Task<bool> DeleteUserAsync(string user)
        {
            string url = $"/deleteUser.php?user={user}";
            var request = GetRequestAsync(url);
            string successJsonSchema =
            @"{
                'type':'object',
                'properties':
                {
                    'deleted': {'type':'boolean', required: true}
                }
            }";
            JSchema schema = JSchema.Parse(successJsonSchema);
            var result = JObject.Parse(await request);
            ValidateMethodOutput(schema, result);
            return result.Value<bool>("added");
        }
        private T ValidateMethodOutput<T>(JSchema schema, JContainer result)
        {
            
            if (result.IsValid(schema))
            {
                return result.ToObject<T>();
            }
            else
            {
                
                string errorJsonSchema = 
                @"{
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
                    handleError((string)((JObject)result).GetValue("error"), (int)((JObject)result).GetValue("errorcode"));
                    return default(T);
                }
                else
                {
                    throw new BotAPIException("Incorrect server response");
                }
            }
        }
        private void ValidateMethodOutput(JSchema schema, JContainer result)
        {
            
            if (!result.IsValid(schema))
            {
                
                string errorJsonSchema = 
                @"{
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
                    handleError((string)((JObject)result).GetValue("error"), (int)((JObject)result).GetValue("errorcode"));
                    
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
                'description': 'Key (Auth result)',
                'type':'object',
                'properties':
                {
                    'key':{'type':'string', 'required':true}
                }
            }";

            JSchema schema = JSchema.Parse(successJsonSchema);
            JObject result = JObject.Parse(await request);
            key = ValidateMethodOutput<Dictionary<string, string>>(schema, result)["key"];
            client.DefaultRequestHeaders.Add("key", key);
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
                case 9:
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
