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
        public async Task<ActionResult<Response>> AskQuestions(Request request)
        {
            Response response = new();       
            foreach (var question in request.Questions)
            {
                string answer = await model.AnswerOneQuestionTask(request.Text, question, token);
                response.Answers.Add(answer);    
            }
            return response;
        }
    }
}
