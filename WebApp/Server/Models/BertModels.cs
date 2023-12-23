namespace Server.Models
{


    public class Request {
        public List<string> Questions {get; set;}
        public string Text {get; set;} 

        public Request(string text) {
            Text = text;
            Questions = new();
        }
    }

    public class Response {
        public List<string> Answers {get; set;}

        public Response ()
        {
            Answers = new();
        }
    }
}