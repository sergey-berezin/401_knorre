using Bert;

namespace BertCs 
{
    internal class Program
    {
        static async Task Main(string[] args) 
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the name of the txt file as a console argument.");
                return;
            }
            
            string fileName = args[0];
            var consoleUI = new ConsoleUI();
            CancellationTokenSource ctf = new CancellationTokenSource();
            var model = new BertModel(fileName, consoleUI, ctf.Token);
            await model.AnswerQuestions(ctf.Token);
        }
    }

    public class ConsoleUI : IUIServices
    {
        public void ReportError(string message) 
        {
            Console.WriteLine(message);
            Environment.Exit(0);
        }

        public void Answer(string message)
        {
            Console.WriteLine(message);
        }

        public string?  GetQuestion()
        {
            return Console.ReadLine();
        }
    }
}