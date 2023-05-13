using System;

namespace Files.EntityFrameworkCore.Extensions
{
	public class FilesExtensionsResponse
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string MimeType { get; set; }
		public DateTimeOffset TimeStamp { get; set; }
		public long TotalBytesLength { get; set; }
	}
}