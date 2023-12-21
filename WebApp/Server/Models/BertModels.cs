namespace Server.Models
{


    public class Request {
        public string Question {get; set;}
        public string Text {get; set;} 

        public Request(string text, string question) {
            Text = text;
            Question = question;
        }
    }

    public class Response {
        public string Answer {get; set;}

        public Response (string answer)
        {
            Answer = answer;
        }
    }
}