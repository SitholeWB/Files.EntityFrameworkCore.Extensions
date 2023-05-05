﻿using System;

namespace Files.EntityFrameworkCore.Extensions
{
	public interface IFileEntity
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
	}
}