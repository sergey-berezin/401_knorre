using Bert;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ViewModel;

public interface IUIServices
{
    string? ChooseFileToOpen();
}
public class RelayCommand : ICommand
{
    private readonly Action<object?> execute;
    private readonly Func<object?, bool>? canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanExecute(object? parameter) => canExecute == null || canExecute(parameter);

    public void Execute(object? parameter) => execute.Invoke(parameter);
}

public class BertTabState
{
    
    public string? text;
    public string? fileName;
    public string? question;
    public string? answer;

}
public class BertTab : INotifyPropertyChanged
{
    private CancellationTokenSource ctf;
    private CancellationToken token;
    public MainViewModel? controller;
    public BertTabState tabState;
    public string Text
    {
        get => tabState.text!;
        set { tabState.text = value; }
    }
    public string? Question
    {
        get => tabState.question;
        set
        {
            AnswerQuestionCommand.RaiseCanExecuteChanged();
            tabState.question = value;
        }
    }
    public string? Answer
    {
        get => tabState.answer;
        set { tabState.answer = value; }
    }
    public string FileName
    {
        get => tabState.fileName!;
        set { tabState.fileName = value; }
    }
    public string TextName
    {
        get => Path.GetFileNameWithoutExtension(FileName);
    }
    public bool IsAnswering { get; set; }
    public List<AnsweredQuestion>? Answered
    {
        get
        {
            if (controller == null)
            {
                return null;
            }
            var textAnsweredQuestions = controller.AllAnsweredQuestions.Where(t => t.text == Text).FirstOrDefault();
            if (textAnsweredQuestions == null)
            {
                TextAnsweredQuestion textAnsweredQuestion = new TextAnsweredQuestion(Text);
                controller.AllAnsweredQuestions.Add(textAnsweredQuestion);
                return controller.AllAnsweredQuestions.Where(t => t.text == Text).First().answeredQuestions;
            } else
            {
                return textAnsweredQuestions.answeredQuestions;
            }
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public RelayCommand AnswerQuestionCommand { get; private set; }
    public RelayCommand CloseTabCommand { get; private set; }
    public void NotifyPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    public BertTab(string text, string fileName, MainViewModel controller)
    {
        tabState = new BertTabState();
        ctf = new CancellationTokenSource();
        token = ctf.Token;
        IsAnswering = false;
        this.controller = controller;
        Text = text;
        FileName = fileName;
        AnswerQuestionCommand = new RelayCommand(AnswerQuestionTask, CanAnswer);
        CloseTabCommand = new RelayCommand(CloseTab);
    }
    public BertTab(BertTabState state, MainViewModel controller) 
    {
        tabState = state;
        ctf = new CancellationTokenSource();
        token = ctf.Token;
        this.controller = controller;
        AnswerQuestionCommand = new RelayCommand(AnswerQuestionTask, CanAnswer);
        CloseTabCommand = new RelayCommand(CloseTab);
    }
    public async void AnswerQuestionTask(object? sender)
    {
        IsAnswering = true;
        AnswerQuestionCommand.RaiseCanExecuteChanged();
        if (Question != null & controller != null)
        {
            var questionHistory = Answered!.Where(q => q.question == Question).FirstOrDefault();
            if (questionHistory != null)
            {
                Answer = questionHistory.answer;
            } else
            {
                Answer = await controller!.bertModel.AnswerOneQuestionTask(Text, Question!, token);
                var answeredQuestion = new AnsweredQuestion(Question!, Answer);
                Answered!.Add(answeredQuestion);
            }
            NotifyPropertyChanged("Answer");
        }
        
        IsAnswering = false;
        AnswerQuestionCommand.RaiseCanExecuteChanged();
    }
    public bool CanAnswer(object? sender)
    {
        return (Question is not null) && (!IsAnswering);
    }
    public void CloseTab(object? sender)
    {
        ctf.Cancel();
        var tabToDelete = controller!.Tabs.FirstOrDefault(f => f.FileName == FileName);
        if (tabToDelete != null)
        {
            controller.Tabs.Remove(tabToDelete);
        }
    }
}

public class AnsweredQuestion
{
    public string question;
    public string answer;

    public AnsweredQuestion(string question, string answer)
    {
        this.question = question;
        this.answer = answer;
    }
}
public class TextAnsweredQuestion
{
    public string text;
    public List<AnsweredQuestion> answeredQuestions;

    public TextAnsweredQuestion(string text)
    {
        this.text = text;
        answeredQuestions = new List<AnsweredQuestion>();
    }
}
public class MainViewModel : INotifyPropertyChanged
{
    public BertModel bertModel;
    private IUIServices uiServices;
    private CancellationToken token;
    public ICommand NewTabCommand { get; private set; }
    public event PropertyChangedEventHandler? PropertyChanged;
    public void NotifyPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    public ObservableCollection<BertTab> Tabs { get; set; }
    public List<TextAnsweredQuestion> AllAnsweredQuestions { get; set; }
    public MainViewModel(IUIServices uiServices)
    {
        Tabs = new ObservableCollection<BertTab>();
        AllAnsweredQuestions = new List<TextAnsweredQuestion>();
        try
        {
            string? prevSessionDescription = File.ReadAllText("session.json");
            var prevSessionTabs = JsonConvert.DeserializeObject<List<BertTabState>>(prevSessionDescription);
            if (prevSessionTabs != null)
            {
                foreach (var tabState in prevSessionTabs)
                {
                    Tabs.Add(new BertTab(tabState, this));
                }
            }
        }
        catch (FileNotFoundException) { }
        catch (JsonSerializationException) { }
        try
        {
            string? prevSessionDescription = File.ReadAllText("answered_questions.json");
            var allAnsweredQuestions = JsonConvert.DeserializeObject<List<TextAnsweredQuestion>>(prevSessionDescription);
            if (allAnsweredQuestions != null)
            {
                AllAnsweredQuestions = allAnsweredQuestions;
            }
            
        }
        catch (FileNotFoundException) { }
        catch (JsonSerializationException) { }
        this.uiServices = uiServices;
        bertModel = new BertModel(token);
        NewTabCommand = new RelayCommand(AddTab);
    }
    public void AddTab(object? sender)
    {
        string? filePath = uiServices.ChooseFileToOpen();
        if (filePath != null)
        {
            string text = File.ReadAllText(filePath);
            Tabs.Add(new BertTab(text, Path.GetFileName(filePath), this));
            NotifyPropertyChanged("Tabs");
        }
    }
    public void SaveCurrentState(object? sender, CancelEventArgs e)
    {
        string session = JsonConvert.SerializeObject(Tabs.Select(tab => tab.tabState));
        File.WriteAllText("session.json", (session));
        string allAnsweredQuestions = JsonConvert.SerializeObject(AllAnsweredQuestions);
        File.WriteAllText("answered_questions.json", allAnsweredQuestions);
    }

}