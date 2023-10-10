using Bert;

internal class ConsoleApp
    {
        static async Task Main(string[] args) 
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the name of the txt file as a console argument.");
                return;
            }
            
            string fileName = args[0];
            CancellationTokenSource ctf = new CancellationTokenSource();
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"File {fileName} does not exist.");
            }
            string text = File.ReadAllText(fileName);
            var model = new BertModel(ctf.Token);
            string? question;
            List<Task> questions = new List<Task>();
            while ((question = Console.ReadLine()) != null && question != "") 
            {
                var answeringTask = model.AnswerOneQuestionTask(text, question, ctf.Token);
                answeringTask.ContinueWith(t => {Console.WriteLine($"Answer: {t.Result}");});
                questions.Add(answeringTask);
            }

            await Task.WhenAll(questions);
        }
    }

