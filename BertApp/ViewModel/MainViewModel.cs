using Bert;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

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
public class BertTab : INotifyPropertyChanged
{
    private BertModel model;
    [JsonProperty("quesion")]
    private string? question;
    [JsonIgnore]
    private CancellationTokenSource ctf;
    [JsonIgnore]
    private CancellationToken token;
    [JsonIgnore]
    private MainViewModel controller;
    [JsonProperty("text")]
    public string Text { get; set; }
    [JsonIgnore]
    public string? Question
    {
        get => question;
        set
        {
            AnswerQuestionCommand.RaiseCanExecuteChanged();
            question = value;
        }
    }
    [JsonProperty("answer")]
    public string? Answer { get; set; }
    [JsonProperty("file_name")]
    public string FileName { get; set; }
    [JsonIgnore]
    public string TextName 
    { 
        get => Path.GetFileNameWithoutExtension(FileName);
    }
    [JsonIgnore]
    public bool IsAnswering { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;
    [JsonIgnore]
    public RelayCommand AnswerQuestionCommand { get; private set; }
    [JsonIgnore]
    public RelayCommand CloseTabCommand { get; private set; }
    public void NotifyPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    public BertTab(string text, string fileName, BertModel bertModel, MainViewModel controller)
    {
        ctf = new CancellationTokenSource();
        token = ctf.Token;
        IsAnswering = false;
        this.controller = controller;
        Text = text;
        FileName = fileName;
        model = bertModel;
        AnswerQuestionCommand = new RelayCommand(AnswerQuestionTask, CanAnswer);
        CloseTabCommand = new RelayCommand(CloseTab);
    }
    public async void AnswerQuestionTask(object? sender)
    {
        IsAnswering = true;
        AnswerQuestionCommand.RaiseCanExecuteChanged();
        if (Question is not null)
        {
            Answer = await model.AnswerOneQuestionTask(Text, Question, token);
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
        var tabToDelete = controller.Tabs.FirstOrDefault(f => f.FileName == FileName);
        if (tabToDelete != null)
        {
            controller.Tabs.Remove(tabToDelete);
        }
    }
}
public class MainViewModel : INotifyPropertyChanged
{
    private BertModel bertModel;
    private IUIServices uiServices;
    private CancellationToken token;
    public ICommand NewTabCommand { get; private set; }
    public event PropertyChangedEventHandler? PropertyChanged;
    public void NotifyPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    public ObservableCollection<BertTab> Tabs { get; set; }
    public MainViewModel(IUIServices uiServices)
    {
        try
        {
            string? prevSessionDescription = File.ReadAllText("session.json");
            var prevSessionTabs = JsonConvert.DeserializeObject<ObservableCollection<BertTab>>(prevSessionDescription);
            if (prevSessionTabs != null)
            {
                Tabs = prevSessionTabs;
            } else
            {
                Tabs = new ObservableCollection<BertTab>();
            }
        }
        catch (FileNotFoundException)
        {
            Tabs = new ObservableCollection<BertTab>();
        }
        catch (JsonSerializationException)
        {
            Tabs = new ObservableCollection<BertTab>();
        }
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
            Tabs.Add(new BertTab(text, Path.GetFileName(filePath), bertModel, this));
            NotifyPropertyChanged("Tabs");
        }
    }
    public void SaveCurrentState(object? sender, CancelEventArgs e)
    {
        string output = JsonConvert.SerializeObject(Tabs);
        File.WriteAllText("session.json", output);
    }

}