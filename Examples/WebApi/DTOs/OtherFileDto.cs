namespace WebApi.DTOs
{
    public class OtherFileDto
    {
        public Guid FileId { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public long TotalBytesLength { get; set; }
    }
}