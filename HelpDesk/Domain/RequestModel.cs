namespace HelpDesk;
public class RequestModel
{
    public string Theme { get; set; }   
    public ObservableCollection<string> Tags {  get; set; } 
    public string Description { get; set; }
    public byte[] fileBytes { get; set; }
    public ObservableCollection<FileDetailModel> FileDetail = new ObservableCollection<FileDetailModel>();
    // TODO Присвоение UID значения из коннектора
    public string UID { get; } = "13";
    public string Email { get; } = "example@mail.ru";
    public long IssueId { get; set; }
    public Dictionary<long, AuthorModel> Comments = new Dictionary<long, AuthorModel>();
    public string Status { get; set; }
    public string CommentMessage { get; set; }
    public string Contractor {  get; set; }
    public string Key { get; set; }
}
