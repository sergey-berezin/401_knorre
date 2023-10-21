using Bert;
using System.ComponentModel;
using System.Windows.Input;

namespace ViewModel;

public class BertTab
{
    public string text;
    public string? question;
    public string? answer;
    public BertTab(string text)
    {
        this.text = text;
    }

    public async void AnswerQuestion(BertModel model, CancellationToken token)
    {
        if (question is not null)
        {
            answer = await model.AnswerOneQuestionTask(text, question, token);
        }
    }
}
class RelayCommand : ICommand
{
    private readonly Action<object?> execute;

    public RelayCommand(Action<object?> execute)
    {
        this.execute = execute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => execute.Invoke(parameter);
}

public class MainViewModel : INotifyPropertyChanged

{
    private BertModel bertModel;
    private List<BertTab> tabs;
    private int currentTabIndex;
    private CancellationToken token;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyPropertyChanged(string propName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    public ICommand AnswerQuestionCommand { get; private set; }
    public string? text
    {
        get { return currentTabIndex != -1 ? tabs[currentTabIndex].text : null; }
    }
    public string? question
    {
        get { return currentTabIndex != -1 ? tabs[currentTabIndex].question : null; }
        set { tabs[currentTabIndex].question = value; }
    }
    public string? answer
    {
        get { return currentTabIndex != -1 ? tabs[currentTabIndex].answer : null; }
        set { tabs[currentTabIndex].answer = value; }
    }
    public MainViewModel()
    {
        CancellationTokenSource ctf = new CancellationTokenSource();
        token = ctf.Token;
        bertModel = new BertModel(token);
        tabs = new List<BertTab>();
        currentTabIndex = -1;
        AnswerQuestionCommand = new RelayCommand(AnswerQuestion);
    }

    public void AddTab()
    {
        string fileName = "C:\\Users\\knorr\\Desktop\\CMC\\FourthYear\\C#\\FirstTask\\BertCsFirst\\BertApp\\Hobbit.txt";
        string text = File.ReadAllText(fileName);
        tabs.Add(new BertTab(text));
        currentTabIndex = tabs.Count - 1;
    }

    public void AnswerQuestion(object? sender)
    {
        Task.Factory.StartNew(() =>
        {
            tabs[currentTabIndex].AnswerQuestion(bertModel, token);
            NotifyPropertyChanged("answer");
        });

    }


}