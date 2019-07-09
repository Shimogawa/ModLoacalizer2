using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;

namespace ModLocalizer2.ModLoader.Core
{
	public class TmodFile : IEnumerable<KeyValuePair<string, TmodFile.FileEntry>>
	{
		public string Name { get; private set; }

		public Version Version { get; set; }

		public int FileCount => fileTable.Length;

		private readonly string path;
		private readonly IDictionary<string, FileEntry> files = new Dictionary<string, FileEntry>();
		private byte[] signature = new byte[256];
		private byte[] hash;
		private Version fileMLVersion;

		private FileEntry[] fileTable;

		private FileStream fileStream;
		private EntryReadStream sharedEntryReadStream;

		public TmodFile(string path)
		{
			this.path = path;
		}

		internal void Read()
		{
			fileStream = File.OpenRead(path);
			using (BinaryReader reader = new BinaryReader(fileStream))
			{
				if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "TMOD")
				{
					throw new Exception($"Magic Header != \"TMOD\"");
				}
				fileMLVersion = new Version(reader.ReadString());
				hash = reader.ReadBytes(20);
				signature = reader.ReadBytes(256);
				reader.ReadInt32();
				long position = this.fileStream.Position;
				if (!SHA1.Create().ComputeHash(fileStream).SequenceEqual(hash))
				{
					throw new Exception("Hash mismatch, data blob has been modified or corrupted");
				}

				fileStream.Position = position;

				Name = reader.ReadString();
				Version = Version.Parse(reader.ReadString());
				int offset = 0;
				fileTable = new FileEntry[reader.ReadInt32()];
				for (int i = 0; i < FileCount; i++)
				{
					FileEntry fe = new FileEntry(reader.ReadString(), offset, reader.ReadInt32(), reader.ReadInt32());
					fileTable[i] = fe;
					files.Add(fe.Name, fe);
					offset += fe.CompressedLength;
				}

				int rPosition = (int) fileStream.Position;
				for (int i = 0; i < FileCount; i++)
				{
					fileTable[i].Offset += rPosition;
				}

				for (int i = 0; i < FileCount; i++)
				{
					fileTable[i].OriginalData = GetRawBytes(fileTable[i]);
				}
			}

			if (!this.HasFile("Info"))
			{
				throw new Exception(string.Format("Missing {0} file", "Info"));
			}
		}

		public byte[] GetRawBytes(string fileName)
		{
			FileEntry entry;
			if (!files.TryGetValue(Sanitize(fileName), out entry))
			{
				return null;
			}
			return GetRawBytes(entry);
		}

		public byte[] GetRawBytes(FileEntry entry)
		{
			if (entry.OriginalData != null)
			{
				return entry.OriginalData;
			}
			byte[] result;
			using (Stream stream = GetStream(entry, false))
			{
				result = stream.ReadBytes(entry.CompressedLength);
			}
			return result;
		}

		public byte[] GetTrueBytes(string fileName)
		{
			FileEntry entry;
			if (!files.TryGetValue(Sanitize(fileName), out entry))
			{
				return null;
			}
			return GetTrueBytes(entry);
		}

		public byte[] GetTrueBytes(FileEntry entry)
		{
			if (!entry.IsCompressed)
				return GetRawBytes(entry);
			using (Stream s = GetStream(entry))
			using (DeflateStream ds = new DeflateStream(s, CompressionMode.Decompress))
			{
				return ds.ReadBytes(entry.Length);
			}
		}

		public Stream GetStream(FileEntry entry, bool newFileStream = false)
		{
			Stream stream;
			if (entry.OriginalData != null)
			{
				stream = new MemoryStream(entry.OriginalData);
			}
			else
			{
				if (fileStream == null)
				{
					throw new IOException("File not open: " + this.path);
				}
				if (newFileStream)
				{
					throw new NotImplementedException("fuck");
				}
				else
				{
					if (sharedEntryReadStream != null)
					{
						throw new IOException("Previous entry read stream not closed: " + sharedEntryReadStream.Name);
					}
					stream = (sharedEntryReadStream = new EntryReadStream(this, entry, fileStream, true));
				}
			}
//			if (entry.IsCompressed)
//			{
//				stream = new DeflateStream(stream, CompressionMode.Decompress);
//			}
			return stream;
		}

		internal void OnStreamClosed(EntryReadStream stream)
		{
			if (stream == sharedEntryReadStream)
			{
				sharedEntryReadStream = null;
			}
		}

		public byte[] GetPrimaryAssembly(bool monoOnly)
		{
			byte[] data;
			if (monoOnly)
			{
				data = GetTrueBytes("Mono.dll");
			}
			else
			{
				data = GetTrueBytes("Windows.dll");
			}

			if (data == null)
			{
				var f = files.Where(kvp => kvp.Key.EndsWith(".XNA.dll")).ToList();
				if (f.Count < 1)
					return null;
				data = GetTrueBytes(f[0].Value);
			}
			return data;
		}

		/// <summary>
		/// The data here should NEVER be compressed.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		/// <param name="newName"></param>
		public void ReplaceFile(string fileName, byte[] data, string newName = null)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			if (!HasFile(fileName)) 
				throw new ArgumentException("No such file.");
			int originLen = data.Length;
			if (data.Length > 1024 && ShouldCompress(fileName))
			{
				data = Compress(data);
			}
			
			string n = string.IsNullOrEmpty(newName) ? fileName : newName;
			var e = new FileEntry(n, -1, originLen, data.Length);
			e.OriginalData = data;
			files[fileName] = e;
			fileTable = null;
		}

		private byte[] Compress(byte[] data)
		{
			using (MemoryStream ms = new MemoryStream(data.Length))
			{
				using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress))
				{
					ds.Write(data, 0, data.Length);
				}

				byte[] arr = ms.ToArray();
				if (arr.Length < (int) (data.Length * 0.9f))
				{
					data = arr;
				}
			}

			return data;
		}

		public IEnumerator<KeyValuePair<string, FileEntry>> GetEnumerator()
		{
			return files.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void RemoveFile(string fileName)
		{
			files.Remove(Sanitize(fileName));
			fileTable = null;
		}

		//		public void AddFile(string fileName, byte[] data)
		//		{
		//			byte[] dataCopy = new byte[data.Length];
		//			data.CopyTo(dataCopy, 0);
		//			files[Sanitize(fileName)] = dataCopy;
		//		}

		private static bool ShouldCompress(string fileName)
		{
			return !fileName.EndsWith(".png") && !fileName.EndsWith(".mp3") && !fileName.EndsWith(".ogg");
		}

		public void Save(string path)
		{
			using (FileStream fs = File.Create(path))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(fs))
				{
					binaryWriter.Write(Encoding.ASCII.GetBytes("TMOD"));
					binaryWriter.Write(fileMLVersion.ToString());
					int num = (int)fs.Position;
					binaryWriter.Write(new byte[280]);
					int num2 = (int)fs.Position;
					binaryWriter.Write(Name);
					binaryWriter.Write(Version.ToString());
					fileTable = files.Values.ToArray();
					binaryWriter.Write(fileTable.Length);
					foreach (var fe in fileTable)
					{
						binaryWriter.Write(fe.Name);
						binaryWriter.Write(fe.Length);
						binaryWriter.Write(fe.CompressedLength);
					}
					int pos = (int)fs.Position;
					foreach (FileEntry fileEntry2 in fileTable)
					{
						binaryWriter.Write(fileEntry2.OriginalData);
						fileEntry2.Offset = pos;
						pos += fileEntry2.CompressedLength;
					}
					fs.Position = num2;
					hash = SHA1.Create().ComputeHash(fs);
					fs.Position = num;
					binaryWriter.Write(hash);
					fs.Seek(256L, SeekOrigin.Current);
					binaryWriter.Write((int)(fs.Length - num2));
				}
			}
		}

		public bool HasFile(string fileName)
		{
			return files.ContainsKey(Sanitize(fileName));
		}

		private static string Sanitize(string path)
		{
			return path.Replace('\\', '/');
		}

		public IDisposable Open()
		{
			Read();
			return new DisposeWrapper(Close);
		}

		public void Close()
		{
			if (fileStream != null)
			{
				fileStream.Close();
			}
			fileStream = null;
		}

		public class FileEntry
		{
			public string Name { get; internal set; }

			public int Offset { get; internal set; }

			public int Length { get; internal set; }

			public int CompressedLength { get; internal set; }

			public byte[] OriginalData { get; internal set; }

			public bool IsCompressed => Length != CompressedLength;

			public FileEntry(string name, int offset, int length, int compressedLength)
			{
				Name = name;
				Offset = offset;
				Length = length;
				CompressedLength = compressedLength;
				OriginalData = null;
			}

			public override string ToString()
			{
				return $"{nameof(Name)}: {Name}, {nameof(Offset)}: {Offset}, {nameof(Length)}: {Length}, {nameof(CompressedLength)}: {CompressedLength}, {nameof(IsCompressed)}: {IsCompressed}";
			}
		}

		private class DisposeWrapper : IDisposable
		{
			private Action disposeAction;

			public DisposeWrapper(Action disposeAction)
			{
				this.disposeAction = disposeAction;
			}

			public void Dispose()
			{
				if (disposeAction == null)
					return;
				disposeAction();
			}
		}
	}
}
