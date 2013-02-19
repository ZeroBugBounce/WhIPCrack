using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace whIPCrack
{
	public class WriteBuffer
	{
		public WriteBuffer(MemoryMappedViewAccessor memoryMappedViewAccessor)
		{
			accessor = memoryMappedViewAccessor;
		}

		private MemoryMappedViewAccessor accessor;
		private Int64 currentLocation = 0;

		public void WriteArray<T>(T[] values) where T : struct
		{
			accessor.WriteArray<T>(currentLocation, values, 0, values.Length);
		}

		public void Write<T>(T structure) where T : struct
		{
			accessor.Write<T>(currentLocation, ref structure);
			currentLocation += Marshal.SizeOf(typeof(T));
		}

		public void Write(Int32 value)
		{
			accessor.Write(currentLocation, value);
			currentLocation += 4;
		}

		public void Write(Byte[] value)
		{
			accessor.WriteArray<Byte>(currentLocation, value, 0, value.Length);
			currentLocation += value.Length;
		}
	}
}
