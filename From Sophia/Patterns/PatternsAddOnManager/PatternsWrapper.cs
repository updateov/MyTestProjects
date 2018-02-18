using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeriGen.Patterns.Engine;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using PeriGen.Patterns.Engine.Data;
using System.Threading.Tasks;
using System.Threading;

namespace PatternsAddOnManager
{
    #region Unmanaged DLL encapsulation

    internal static class NativeMethods
    {
        /// <summary>
        /// Implementation of a Critical handle to properly wrap the unmanaged data pointer returned and used by the DLL
        /// </summary>
        public sealed class EngineHandle : CriticalHandleZeroOrMinusOneIsInvalid
        {
            /// <summary>
            /// This default ctor will be called by P/Invoke smart marshalling when returning MySafeHandle in a method call 
            /// </summary>
            private EngineHandle()
            {
                SetHandle((IntPtr)0);
            }

            /// <summary>
            /// Copy constructor. We need this so that we can do our own marshalling (and may be for user supplied handles)
            /// </summary>
            /// <param name="preexistingHandle">Pre existing handle</param>
            internal EngineHandle(IntPtr preexistingHandle)
            {
                SetHandle(preexistingHandle);
            }

            /// <summary>
            /// Release the internal handle
            /// We should not provide a finalizer - SafeHandle's critical finalizer will call ReleaseHandle inside a CER for us. 
            /// </summary>
            /// <returns></returns>
            override protected bool ReleaseHandle()
            {
                if (!IsInvalid && !IsClosed)
                {
                    // Release the handle
                    NativeMethods.EngineUninitialize(this);

                    // Mark the handle as invalid for future users.
                    SetHandleAsInvalid();
                    return true;
                }

                // Return false. 
                return false;
            }
        }

        [DllImport("PatternsDriver.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern EngineHandle AddOnEngineInitialize();

        [DllImport("PatternsDriver.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern int EngineProcessHR(EngineHandle param, byte[] signal, int start, int count);

        [DllImport("PatternsDriver.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern int EngineProcessUP(EngineHandle param, byte[] signal, int start, int count);

        [DllImport("PatternsDriver.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool EngineReadResults(EngineHandle param, StringBuilder output_buffer, int output_size);

        [DllImport("PatternsDriver.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern void EngineUninitialize(EngineHandle param);
    }

    #endregion

}
