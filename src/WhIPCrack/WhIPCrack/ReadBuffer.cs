using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace whIPCrack
{
	public class ReadBuffer
	{
		public ReadBuffer(MemoryMappedViewAccessor memoryMappedViewAccessor)
		{
			accessor = memoryMappedViewAccessor;
		}

		private MemoryMappedViewAccessor accessor;
		private Int64 currentLocation = 0;

        //public Int32 ReadInt32()
        //{
        //    Int32 value = accessor.ReadInt32(currentLocation);
        //    currentLocation += Constants.HeaderSize;
        //    return value;
        //}

		public T Read<T>() where T : struct
		{
			T value;
			accessor.Read<T>(currentLocation, out value);
			currentLocation += Marshal.SizeOf(typeof(T));
			return value;
		}

        public T[] ReadArray<T>() where T : struct
        {
            int length = Read<Int32>();
            T[] values = new T[length];
            accessor.ReadArray<T>(currentLocation, values, 0, values.Length);
            currentLocation += Marshal.SizeOf(typeof(T)) * values.Length;
            return values;
        }
	}
}
