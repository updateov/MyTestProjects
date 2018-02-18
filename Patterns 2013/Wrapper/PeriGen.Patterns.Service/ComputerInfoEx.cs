using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace PeriGen.Patterns.Service
{
	/// <summary>
	/// On Server 2003, the easy way to retrieve the number of cores is not working so
	/// it has to be done using Interop...
	/// </summary>
	public static class ComputerInfoEx
	{
		static class SafeNativeMethods
		{
			[DllImport("kernel32.dll", SetLastError = true)]
			internal static extern bool GetLogicalProcessorInformation([Out] SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] infos, ref uint infoSize);

			internal const int ERROR_INSUFFICIENT_BUFFER = 122;

			internal enum PROCESSOR_CACHE_TYPE
			{
				UnifiedCache = 0,
				InstructionCache = 1,
				DataCache = 2,
				TraceCache = 3
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct CACHE_DESCRIPTOR
			{
				byte Level;
				byte Associativity;
				UInt16 LineSize;
				UInt32 Size;
				[MarshalAs(UnmanagedType.U4)]
				PROCESSOR_CACHE_TYPE Type;
			}

			internal enum RelationProcessorCore
			{
				Unknown = -1,
				RelationProcessorCore = 0,
				RelationNumaNode = 1,
				RelationCache = 2,
				RelationProcessorPackage = 3
			}

			[StructLayout(LayoutKind.Explicit)]
			internal struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION
			{
				[FieldOffset(0)]
				public uint ProcessorMask;
				[FieldOffset(4), MarshalAs(UnmanagedType.U4)]
				public RelationProcessorCore Relationship;
				[FieldOffset(8)]
				public byte Flags;
				[FieldOffset(8)]
				public CACHE_DESCRIPTOR Cache;
				[FieldOffset(8)]
				public UInt32 NodeNumber;
				[FieldOffset(8)]
				public UInt64 Reserved1;
				[FieldOffset(16)]
				public UInt64 Reserved2;
			}
		}

		/// <summary>
		/// Number of physical cores on the machine
		/// Return a negative number if it failed to retrieve the core count
		/// </summary>
		[SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)] 
		public static int GetCoreCount()
		{
			// Easy one if available...
			try
			{
				int cores = 0;
				using (var smm = new System.Management.ManagementObjectSearcher("Select * from Win32_Processor"))
				{
					foreach (var item in smm.Get())
					{
						cores += int.Parse(item["NumberOfCores"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
					}
				}
				return cores;
			}
			catch (Exception)
			{
				// So not there... use the heavy duty interop way
			}

			// Only for x86!
			if (Marshal.SizeOf(typeof(IntPtr)) == 8)
				return -1;

			// Get the buffer size information
			uint returnLength = 0;
			ComputerInfoEx.SafeNativeMethods.SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] processors = null;
			if (ComputerInfoEx.SafeNativeMethods.GetLogicalProcessorInformation(processors, ref returnLength))
				return -2;

			// Check that it failed for the proper reason
			if (Marshal.GetLastWin32Error() != ComputerInfoEx.SafeNativeMethods.ERROR_INSUFFICIENT_BUFFER)
				return -3;

			// Allocate the proper buffer
			processors = new ComputerInfoEx.SafeNativeMethods.SYSTEM_LOGICAL_PROCESSOR_INFORMATION[(int)(Math.Ceiling(returnLength / (double)Marshal.SizeOf(typeof(ComputerInfoEx.SafeNativeMethods.SYSTEM_LOGICAL_PROCESSOR_INFORMATION))))];
			for (int i = 0; i < processors.Length; ++i)
			{
				processors[i].Relationship = ComputerInfoEx.SafeNativeMethods.RelationProcessorCore.Unknown;
			}

			// Do the real query
			if (!ComputerInfoEx.SafeNativeMethods.GetLogicalProcessorInformation(processors, ref returnLength))
				return -4;

			return processors.Where(p => p.Relationship == ComputerInfoEx.SafeNativeMethods.RelationProcessorCore.RelationProcessorCore).Count();
		}


		static string _FDQN = ReadFDQN();

		/// <summary>
		/// Full qualified domain name
		/// </summary>
		public static string FDQN { get { return _FDQN; } }

		/// <summary>
		/// Construct the FDQN
		/// </summary>
		/// <returns></returns>
		static string ReadFDQN()
		{
			try
			{
				string domainName = string.Empty;
				try
				{
					domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName.ToUpperInvariant();
				}
				catch
				{
					// Just ignore
				}

				string hostName = string.Empty;
				try
				{
					hostName = System.Net.Dns.GetHostName().ToUpperInvariant();
				}
				catch
				{
					// Just ignore
				}

				if (!string.IsNullOrEmpty(domainName) && (!hostName.EndsWith(domainName)))
				{
					return hostName + "." + domainName ;
				}

				return hostName;
			}
			catch
			{
				// Just ignore
			}
			return string.Empty;
		}
	}
}
