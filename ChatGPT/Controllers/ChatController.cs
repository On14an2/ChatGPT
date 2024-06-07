using ChatGPT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;


namespace ChatGPT.Controllers
{
    public class ChatController : Controller
    {
        private const string GptApiKey = "sk-proj-oCG1dhoAI5UjKEwWqm56T3BlbkFJ9qP97FCWKkdU8T9RXPrr";
        private const string GptEndpoint = "https://api.openai.com/v1/chat/completions";


        private readonly ChatAppContext _context;

        public ChatController(ChatAppContext context)
        {
            _context = context;
        }

        private Chat chat = new Chat();
        private Chat aboba = new Chat();
        
        
        private static readonly List<Mess> chatHistory = new List<Mess>();

        static string questionTemplate = @"{
        ""model"": ""gpt-3.5-turbo"",
        ""messages"": [
            {
                ""role"": ""system"",
                ""content"": ""You are a helpful assistant.""
            },
            {
                ""role"": ""user"",
                ""content"": ""{0}""
            }
        ]
        }";
        
        [HttpGet]
        public async Task<IActionResult> GetUserChats()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var chats = await _context.Chats
                                      .Where(c => c.UserId == Convert.ToInt32(userId))
                                      .ToListAsync();
            return Json(chats);
        }
        public ActionResult Index()
        {
            return View(chatHistory);
        }
      


        [HttpPost]
        public async Task<JsonResult> GetChatGptResponse(string message)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            GetAnswer(message);

            string jsonContent = questionTemplate.Replace("{0}", message);

            chatHistory.Add(new Mess { role = "user", content = message });

            chat.UserId = Convert.ToInt32(userId);
            chat.Role = "user";
            chat.Message = message;
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
                Headers =
            {
                { HttpRequestHeader.ContentType.ToString(), "application/json" },
                { HttpRequestHeader.Authorization.ToString(), "Bearer " + "sk-proj-oCG1dhoAI5UjKEwWqm56T3BlbkFJ9qP97FCWKkdU8T9RXPrr"},
            },
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            var response = await httpClient.SendAsync(request);
            string answer = await GetClearAnswerFromResponse(response);

            chatHistory.Add(new Mess { role = "system", content = answer });

            aboba.UserId = Convert.ToInt32(userId);
            aboba.Role = "system";
            aboba.Message = answer;
            _context.Chats.Add(aboba);
            await _context.SaveChangesAsync();
            
            return Json(chatHistory);
            //return answer;
        }
        static async Task<string> GetClearAnswerFromResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var parsedResponse = JObject.Parse(responseContent);

                //var choices = parsedResponse["choices"];
                //if (choices == null) return "*** 1";
                //if (choices.Count() == 0) return "*** 2";
                //if (choices[0] == null) return "*** 3";
                //var message = choices![0]!["message"];
                //if (message == null) return "*** 4";
                //if (message["content"] == null) return "*** 5";
                //var answer = message["content"]!.ToString();

                //return answer;

                var choices = parsedResponse["choices"];
                var message = choices?[0]?["message"];
                return message?["content"]?.ToString() ?? "Error: Unable to get response content";
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return $"Error: {response.StatusCode}, Content: {errorContent}";
        }
        static string CleanseString(string input)
        {
            return JsonConvert.ToString(input);
        }
        static async Task<string> GetAnswer(string question)
        {
            //question = CleanseString(question);
            string jsonContent = questionTemplate.Replace("{0}", question);

            var httpClient = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
                Headers =
            {
                { HttpRequestHeader.ContentType.ToString(), "application/json" },
                { HttpRequestHeader.Authorization.ToString(), "Bearer " + "sk-proj-oCG1dhoAI5UjKEwWqm56T3BlbkFJ9qP97FCWKkdU8T9RXPrr"},
            },
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            var response = await httpClient.SendAsync(request);
            string answer = await GetClearAnswerFromResponse(response);

            return answer;
        }
        [HttpPost]
        static async Task<IActionResult> GetAsync(string message)
        {
            using var httpClient = new HttpClient();
            using HttpResponseMessage response = await httpClient.GetAsync(GptEndpoint);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{jsonResponse}\n");
            return new NotFoundResult();
        }
    }
} 

