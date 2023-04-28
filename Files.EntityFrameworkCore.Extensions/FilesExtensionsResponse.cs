using System.IO;

namespace Files.EntityFrameworkCore.Extensions
{
	public class FilesExtensionsResponse
	{
		public Stream Stream { get; set; }
		public string Name { get; set; }
		public long Length { get; set; }
		public string MimeType { get; set; }
	}
}