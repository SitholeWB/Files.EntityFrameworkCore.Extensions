using System.Text;

namespace ConsoleApp1
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			string inputText = "I love LORD Christ Jesus";
			byte[] bytes = Encoding.ASCII.GetBytes(inputText);
			var stream = new MemoryStream(bytes);
			var streamOut = new MemoryStream();

			var listBytes = WriteStream(stream);

			foreach (var mainFile in listBytes)
			{
				ReadChunk(streamOut, mainFile.Data);
				//streamOut.Write(mainFile.Data, mainFile.ByteOffset, mainFile.Data.Length);
			}
			string someString = Encoding.ASCII.GetString(streamOut.ToArray());
			Console.WriteLine("Hello, World!");
		}

		private static IList<Chunk> WriteStream(Stream stream)
		{
			var list = new List<Chunk>();
			byte[] chunk = new byte[15];
			int count = 0;
			int countBytes = 0;
			while (true)
			{
				count++;
				int index = 0;
				// There are various different ways of structuring this bit of code. Fundamentally
				// we're trying to keep reading in to our chunk until either we reach the end of the
				// stream, or we've read everything we need.
				while (index < chunk.Length)
				{
					countBytes = chunk.Length - index;
					int bytesRead = stream.Read(chunk, index, countBytes);
					if (bytesRead == 0)
					{
						break;
					}
					index += bytesRead;
				}
				if (index != 0) // Our previous chunk may have been the last one
				{
					//SendChunk(chunk, index); // index is the number of bytes in the chunk
					list.Add(new Chunk
					{
						Data = chunk,
						ByteOffset = index,
						ByteCount = countBytes,
						id = count
					});
				}
				if (index != chunk.Length) // We didn't read a full chunk: we're done
				{
					return list;
				}
			}
		}

		// Attempts to read an entire chunk into the given array; returns the size of chunk actually read.
		private static int ReadChunk(Stream stream, byte[] chunk)
		{
			int index = 0;
			while (index < chunk.Length)
			{
				int bytesRead = stream.Read(chunk, index, chunk.Length - index);
				if (bytesRead == 0)
				{
					break;
				}
				index += bytesRead;
			}
			return index;
		}
	}

	internal class Chunk
	{
		public byte[] Data;
		public int ByteOffset;
		public int ByteCount;
		public int id;
	}
}