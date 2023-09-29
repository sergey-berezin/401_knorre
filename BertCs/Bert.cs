using System.Net.Http;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BERTTokenizers;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;


namespace Bert {

    public class BertModel
    {
        private InferenceSession? Session;
        private Task createSession;
        public BertModel(CancellationToken token)
        {
            createSession = Task.Factory.StartNew(() => CreateSession(token), TaskCreationOptions.LongRunning);
        }
        public async Task DownloadFile(string source, string destination, CancellationToken token)
        {
            bool success = false;
            while (!success)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        using var sourceContent = await client.GetStreamAsync(source);
                        using var fileStream = new FileStream(destination, FileMode.OpenOrCreate);
                        await sourceContent.CopyToAsync(fileStream);

                    }
                    success = true;
                }
                catch (HttpRequestException)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(5000); // wait 5 seconds before retrying
                }
            }
        }
        public async Task CreateSession(CancellationToken token)
        {
            string link = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
            string modelPath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
            try 
            {
                Session = new InferenceSession(modelPath);
            }
            catch(OnnxRuntimeException)
            {
                token.ThrowIfCancellationRequested();
                await DownloadFile(link, modelPath, token);
            }

            Session = new InferenceSession(modelPath);
        }
        public async Task<string> AnswerOneQuestion(string text, string question, CancellationToken token) 
        {
            // var input = "Where does the hobbit live?";
            var sentence = $"{{\"question\": \"{question}\", \"context\": \"{text}\"}}";
            // Create Tokenizer and tokenize the sentence.
            var tokenizer = new BertUncasedLargeTokenizer();
            // Get the sentence tokens.
            var tokens = tokenizer.Tokenize(sentence);
            // Encode the sentence and pass in the count of the tokens in the sentence.
            var encoded = tokenizer.Encode(tokens.Count(), sentence);
            // Break out encoding to InputIds, AttentionMask and TypeIds from list of (input_id, attention_mask, type_id).
            var bertInput = new BertInput()
            {
                InputIds = encoded.Select(t => t.InputIds).ToArray(),
                AttentionMask = encoded.Select(t => t.AttentionMask).ToArray(),
                TypeIds = encoded.Select(t => t.TokenTypeIds).ToArray(),
            };
            
            // Create input tensor.
            var inputIdsTensor = ConvertToTensor(bertInput.InputIds, bertInput.InputIds.Length);
            var attentionMaskTensor = ConvertToTensor(bertInput.AttentionMask, bertInput.InputIds.Length);
            var tokenTypeIdsTensor = ConvertToTensor(bertInput.TypeIds, bertInput.InputIds.Length);
            // Create input data for session.
            var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor), 
                                                    NamedOnnxValue.CreateFromTensor("input_mask", attentionMaskTensor), 
                                                    NamedOnnxValue.CreateFromTensor("segment_ids", tokenTypeIdsTensor) };
            token.ThrowIfCancellationRequested();
            await createSession;
            token.ThrowIfCancellationRequested();
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue>? output;
            lock(Session!)
            {
                output = Session.Run(input);
            }
            List<float> startLogits = (output.ToList().First().Value as IEnumerable<float>)!.ToList();
            List<float> endLogits = (output.ToList().Last().Value as IEnumerable<float>)!.ToList();

            // Get the Index of the Max value from the output lists.
            var startIndex = startLogits.ToList().IndexOf(startLogits.Max()); 
            var endIndex = endLogits.ToList().IndexOf(endLogits.Max());
            // From the list of the original tokens in the sentence
            // Get the tokens between the startIndex and endIndex and convert to the vocabulary from the ID of the token.
            var predictedTokens = tokens
                        .Skip(startIndex)
                        .Take(endIndex + 1 - startIndex)
                        .Select(o => tokenizer.IdToToken((int)o.VocabularyIndex))
                        .ToList();
            var connectedTokens = tokenizer.Untokenize(predictedTokens);
            // Print the result.
            return string.Join(" ", connectedTokens);
        }
        public static Tensor<long> ConvertToTensor(long[] inputArray, int inputDimension)
        {
            // Create a tensor with the shape the model is expecting. Here we are sending in 1 batch with the inputDimension as the amount of tokens.
            Tensor<long> input = new DenseTensor<long>(new[] { 1, inputDimension });

            // Loop through the inputArray (InputIds, AttentionMask and TypeIds)
            for (var i = 0; i < inputArray.Length; i++)
            {
                // Add each to the input Tenor result.
                // Set index and array value of each input Tensor.
                input[0,i] = inputArray[i];
            }
            return input;
        }
    }
    public class BertInput
    {
        public long[]? InputIds { get; set; }
        public long[]? AttentionMask { get; set; }
        public long[]? TypeIds { get; set; }
    }
}