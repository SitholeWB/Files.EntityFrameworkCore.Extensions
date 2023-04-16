using System;
using System.IO;
using System.Security.Cryptography;

namespace Files.EntityFrameworkCore.Extensions
{
	public class FileHelper : IDisposable
	{
		private readonly string _filePath;

		public FileHelper(Stream stream, string name)
		{
			if (!Directory.Exists(FILES_TEMP_FOLDER))
			{
				Directory.CreateDirectory(FILES_TEMP_FOLDER);
			}
			_filePath = Path.Combine(FILES_TEMP_FOLDER, $"{Guid.NewGuid()}_{name}");
			using var fileStream = File.Create(_filePath);
			stream.CopyTo(fileStream);
		}

		public string GetFilePath() => _filePath;

		public static string FILES_TEMP_FOLDER => Path.Combine(Path.GetTempPath(), "FILES_TEMP_DIR");

		public static void CleanTempFolder()
		{
			if (Directory.Exists(FILES_TEMP_FOLDER))
			{
				try
				{
					Directory.Delete(FILES_TEMP_FOLDER, true);
					Directory.CreateDirectory(FILES_TEMP_FOLDER);
				}
				catch { }
			}
		}

		public void Dispose()
		{
			if (File.Exists(_filePath))
			{
				File.Delete(_filePath);
			}
			GC.SuppressFinalize(this);
		}

		public static string SHA512CheckSum(string filePath)
		{
			using var SHA256 = SHA512.Create();
			using var fileStream = File.OpenRead(filePath);
			return BitConverter.ToString(SHA256.ComputeHash(fileStream)).Replace("-", "");
		}
	}
}