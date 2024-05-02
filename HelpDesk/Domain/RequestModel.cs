namespace HelpDesk;

public class RequestModel
{
    public string Theme { get; set; }   
    public ObservableCollection<string> Tags {  get; set; } 
    public string Description { get; set; }

}
