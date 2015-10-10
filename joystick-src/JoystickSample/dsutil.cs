using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace prp
{
    public class DsUtils
    {
        public static bool IsCorrectDirectXVersion()
        {
            return File.Exists(Path.Combine(Environment.SystemDirectory, @"dpnhpast.dll"));
        }

        public static bool ShowCapPinDialog(ICaptureGraphBuilder2 bld, IBaseFilter flt, IntPtr hwnd)
        {
            int hr;
            object comObj = null;
            ISpecifyPropertyPages spec = null;
            DsCAUUID cauuid = new DsCAUUID();

            try
            {
                Guid cat = PinCategory.Capture;
                Guid type = MediaType.Interleaved;
                Guid iid = typeof(IAMStreamConfig).GUID;
                hr = bld.FindInterface(ref cat, ref type, flt, ref iid, out comObj);
                if (hr != 0)
                {
                    type = MediaType.Video;
                    hr = bld.FindInterface(ref cat, ref type, flt, ref iid, out comObj);
                    if (hr != 0)
                        return false;
                }
                spec = comObj as ISpecifyPropertyPages;
                if (spec == null)
                    return false;

                hr = spec.GetPages(out cauuid);
                hr = OleCreatePropertyFrame(hwnd, 30, 30, null, 1,
                        ref comObj, cauuid.cElems, cauuid.pElems, 0, 0, IntPtr.Zero);
                return true;
            }
            catch (Exception ee)
            {
                Trace.WriteLine("!Ds.NET: ShowCapPinDialog " + ee.Message);
                return false;
            }
            finally
            {
                if (cauuid.pElems != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(cauuid.pElems);

                spec = null;
                if (comObj != null)
                    Marshal.ReleaseComObject(comObj); comObj = null;
            }
        }

        [DllImport("olepro32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int OleCreatePropertyFrame(IntPtr hwndOwner, int x, int y,
            string lpszCaption, int cObjects,
            [In, MarshalAs(UnmanagedType.Interface)] ref object ppUnk,
            int cPages, IntPtr pPageClsID, int lcid, int dwReserved, IntPtr pvReserved);
    }

    [ComVisible(true), ComImport,
    Guid("B196B28B-BAB4-101A-B69C-00AA00341D07"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISpecifyPropertyPages
    {
        [PreserveSig]
        int GetPages(out DsCAUUID pPages);
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DsCAUUID		// CAUUID
    {
        public int cElems;
        public IntPtr pElems;
    }
}
