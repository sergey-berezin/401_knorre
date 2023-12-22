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
        [HttpPost]
        public async Task<ActionResult<Response>> AskQuestions(Request request)
        {
            Response response = new();
            CancellationTokenSource ctf = new CancellationTokenSource();
            CancellationToken token = ctf.Token;
            BertModel model = new BertModel(token);
            foreach (var question in request.Questions)
            {
                string answer = await model.AnswerOneQuestionTask(request.Text, question, token);
                response.Answers.Add(answer);    
            }
            return response;
        }
    }
}
