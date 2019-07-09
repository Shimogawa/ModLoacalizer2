using System;
using System.IO;

namespace ModFileCore.ModLoader.Core
{
	internal sealed class EntryReadStream : Stream
	{
		// Token: 0x17000250 RID: 592
		// (get) Token: 0x06001977 RID: 6519 RVA: 0x00413B98 File Offset: 0x00411D98
		private int Start
		{
			get
			{
				return this.entry.Offset;
			}
		}

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x06001978 RID: 6520 RVA: 0x00413BA5 File Offset: 0x00411DA5
		public string Name
		{
			get
			{
				return this.entry.Name;
			}
		}

		// Token: 0x06001979 RID: 6521 RVA: 0x00413BB4 File Offset: 0x00411DB4
		public EntryReadStream(TmodFile file, TmodFile.FileEntry entry, Stream stream, bool leaveOpen)
		{
			this.file = file;
			this.entry = entry;
			this.stream = stream;
			this.leaveOpen = leaveOpen;
			if (this.stream.Position != (long)this.Start)
			{
				this.stream.Position = (long)this.Start;
			}
		}

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x0600197A RID: 6522 RVA: 0x00413C0A File Offset: 0x00411E0A
		public override bool CanRead
		{
			get
			{
				return this.stream.CanRead;
			}
		}

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x0600197B RID: 6523 RVA: 0x00413C17 File Offset: 0x00411E17
		public override bool CanSeek
		{
			get
			{
				return this.stream.CanSeek;
			}
		}

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x0600197C RID: 6524 RVA: 0x00413C24 File Offset: 0x00411E24
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x0600197D RID: 6525 RVA: 0x00413C27 File Offset: 0x00411E27
		public override long Length
		{
			get
			{
				return (long)this.entry.CompressedLength;
			}
		}

		// Token: 0x17000256 RID: 598
		// (get) Token: 0x0600197E RID: 6526 RVA: 0x00413C35 File Offset: 0x00411E35
		// (set) Token: 0x0600197F RID: 6527 RVA: 0x00413C4C File Offset: 0x00411E4C
		public override long Position
		{
			get
			{
				return this.stream.Position - (long)this.Start;
			}
			set
			{
				if (value < 0L || value > this.Length)
				{
					throw new ArgumentOutOfRangeException(string.Format("Position {0} outside range (0-{1})", value, this.Length));
				}
				this.stream.Position = value + (long)this.Start;
			}
		}

		// Token: 0x06001980 RID: 6528 RVA: 0x00413C9C File Offset: 0x00411E9C
		public override void Flush()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001981 RID: 6529 RVA: 0x00413CA3 File Offset: 0x00411EA3
		public override int Read(byte[] buffer, int offset, int count)
		{
			count = Math.Min(count, (int)(this.Length - this.Position));
			return this.stream.Read(buffer, offset, count);
		}

		// Token: 0x06001982 RID: 6530 RVA: 0x00413CCC File Offset: 0x00411ECC
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (origin != SeekOrigin.Current)
			{
				this.Position = ((origin == SeekOrigin.Begin) ? offset : (this.Length - offset));
				return this.Position;
			}
			long num = this.Position + offset;
			if (num < 0L || num > this.Length)
			{
				throw new ArgumentOutOfRangeException(string.Format("Position {0} outside range (0-{1})", num, this.Length));
			}
			return this.stream.Seek(offset, origin) - (long)this.Start;
		}

		// Token: 0x06001983 RID: 6531 RVA: 0x00413D45 File Offset: 0x00411F45
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001984 RID: 6532 RVA: 0x00413D4C File Offset: 0x00411F4C
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001985 RID: 6533 RVA: 0x00413D53 File Offset: 0x00411F53
		public override void Close()
		{
			if (this.stream == null)
			{
				return;
			}
			if (!this.leaveOpen)
			{
				this.stream.Close();
			}
			this.stream = null;
			file.OnStreamClosed(this);
		}

		// Token: 0x0400160B RID: 5643
		private TmodFile file;

		// Token: 0x0400160C RID: 5644
		private TmodFile.FileEntry entry;

		// Token: 0x0400160D RID: 5645
		private Stream stream;

		// Token: 0x0400160E RID: 5646
		private bool leaveOpen;
	}
}
