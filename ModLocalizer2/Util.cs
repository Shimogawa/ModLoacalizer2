using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLocalizer2
{
	public static class Util
	{
		public static byte[] ReadBytes(this Stream stream, long len)
		{
			byte[] array = new byte[len];
			stream.ReadBytes(array);
			return array;
		}

		public static byte[] ReadBytes(this Stream stream, int len)
		{
			return stream.ReadBytes((long)len);
		}

		public static void ReadBytes(this Stream stream, byte[] buf)
		{
			int num = 0;
			int num2;
			while ((num2 = stream.Read(buf, num, buf.Length - num)) > 0)
			{
				num += num2;
			}
			if (num != buf.Length)
			{
				throw new IOException(string.Format("Stream did not contain enough bytes ({0}) < ({1})", num, buf.Length));
			}
		}
	}
}
