using Files.EntityFrameworkCore.Extensions;

namespace WebApi.Entities
{
	public class UserImage : IFileEntity
	{
		public Guid Id { get; set; }
		public Guid FileId { get; set; }
		public string Name { get; set; }
		public string MimeType { get; set; }
		public string Hash { get; set; }
		public DateTimeOffset TimeStamp { get; set; }
		public Guid? NextId { get; set; }
		public long Start { get; set; }
		public long ChunkBytesLength { get; set; }
		public long TotalBytesLength { get; set; }
		public byte[] Data { get; set; }
	}
}