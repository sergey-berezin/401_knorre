using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using System.Threading;
using Bert;

namespace Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BertController : ControllerBase
    {
        static public CancellationTokenSource ctf = new CancellationTokenSource();
        static public CancellationToken token = ctf.Token;    
        static public BertModel model = new BertModel(token);
        [HttpPost]
        public ActionResult<Response> AskQuestions(Request request)
        {
            Response response = new();
            List<Task<string>> tasks = new();    
            foreach (var question in request.Questions)
            {
                var task = model.AnswerOneQuestionTask(request.Text, question, token);
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            foreach (var task in tasks) 
            {
                response.Answers.Add(task.Result);
            }
            return response;
        }
    }
}
