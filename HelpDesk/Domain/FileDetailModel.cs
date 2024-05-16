namespace HelpDesk.Domain;
public class FileDetailModel
{
    public string FileName { get; set; }
    public string FileSize { get; set; }
    public int UploadProgess { get; set; }
    public byte[] FileBytes { get; set; }
}
