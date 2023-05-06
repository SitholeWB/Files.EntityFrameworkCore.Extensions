using System;

namespace Files.EntityFrameworkCore.Extensions.Tests
{
	public class TextStreamExtendedEntity : IFileEntity
	{
		public Guid Id { get; set; }
		public Guid FileId { get; set; }
		public string Name { get; set; }
		public string MimeType { get; set; }
		public DateTimeOffset TimeStamp { get; set; }
		public Guid? NextId { get; set; }
		public int ChunkBytesLength { get; set; }
		public long TotalBytesLength { get; set; }
		public byte[] Data { get; set; }
		public string Origin { get; set; }
		public string AddedBy { get; set; }
	}
}