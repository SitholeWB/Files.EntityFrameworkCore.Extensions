using System;

namespace Files.EntityFrameworkCore.Extensions
{
	public interface IFileEntity
	{
		public Guid Id { get; set; }
		public Guid FileId { get; set; }
		public string Name { get; set; }
		public string MimeType { get; set; }
		public string Hash { get; set; }
		public DateTimeOffset TimeStamp { get; set; }
		public Guid? NextId { get; set; }
		public int Start { get; set; }
		public int Length { get; set; }
		public byte[] Data { get; set; }
	}
}