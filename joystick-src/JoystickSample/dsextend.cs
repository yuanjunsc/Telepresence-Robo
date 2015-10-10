using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace prp
{
    [ComVisible(true), ComImport,
    Guid("56a868a9-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IGraphBuilder
    {
        #region "IFilterGraph Methods"
        [PreserveSig]
        int AddFilter(
            [In] IBaseFilter pFilter,
            [In, MarshalAs(UnmanagedType.LPWStr)]			string pName);

        [PreserveSig]
        int RemoveFilter([In] IBaseFilter pFilter);

        [PreserveSig]
        int EnumFilters([Out] out IEnumFilters ppEnum);

        [PreserveSig]
        int FindFilterByName(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string pName,
            [Out]										out IBaseFilter ppFilter);

        [PreserveSig]
        int ConnectDirect([In] IPin ppinOut, [In] IPin ppinIn,
           [In, MarshalAs(UnmanagedType.LPStruct)]			AMMediaType pmt);

        [PreserveSig]
        int Reconnect([In] IPin ppin);

        [PreserveSig]
        int Disconnect([In] IPin ppin);

        [PreserveSig]
        int SetDefaultSyncSource();
        #endregion

        [PreserveSig]
        int Connect([In] IPin ppinOut, [In] IPin ppinIn);

        [PreserveSig]
        int Render([In] IPin ppinOut);

        [PreserveSig]
        int RenderFile(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string lpcwstrFile,
            [In, MarshalAs(UnmanagedType.LPWStr)]			string lpcwstrPlayList);

        [PreserveSig]
        int AddSourceFilter(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string lpcwstrFileName,
            [In, MarshalAs(UnmanagedType.LPWStr)]			string lpcwstrFilterName,
            [Out]										out IBaseFilter ppFilter);

        [PreserveSig]
        int SetLogFile(IntPtr hFile);

        [PreserveSig]
        int Abort();

        [PreserveSig]
        int ShouldOperationContinue();
    }

    [ComVisible(true), ComImport,
   Guid("93E5A4E0-2D50-11d2-ABFA-00A0C9C6E38D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICaptureGraphBuilder2
    {
        [PreserveSig]
        int SetFiltergraph([In] IGraphBuilder pfg);

        [PreserveSig]
        int GetFiltergraph([Out] out IGraphBuilder ppfg);

        [PreserveSig]
        int SetOutputFileName(
            [In]											ref Guid pType,
            [In, MarshalAs(UnmanagedType.LPWStr)]			string lpstrFile,
            [Out]										out IBaseFilter ppbf,
            [Out]										out IFileSinkFilter ppSink);

        [PreserveSig]
        int FindInterface(
            [In]											ref Guid pCategory,
            [In]											ref Guid pType,
            [In]											IBaseFilter pbf,
            [In]											ref Guid riid,
            [Out, MarshalAs(UnmanagedType.IUnknown)]		out	object ppint);

        [PreserveSig]
        int RenderStream(
            [In]										ref Guid pCategory,
            [In]										ref Guid pType,
            [In, MarshalAs(UnmanagedType.IUnknown)]			object pSource,
            [In]											IBaseFilter pfCompressor,
            [In]											IBaseFilter pfRenderer);

        [PreserveSig]
        int ControlStream(
            [In]										ref Guid pCategory,
            [In]										ref Guid pType,
            [In]											IBaseFilter pFilter,
            [In]											IntPtr pstart,
            [In]											IntPtr pstop,
            [In]											short wStartCookie,
            [In]											short wStopCookie);

        [PreserveSig]
        int AllocCapFile(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string lpstrFile,
            [In]											long dwlSize);

        [PreserveSig]
        int CopyCaptureFile(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string lpwstrOld,
            [In, MarshalAs(UnmanagedType.LPWStr)]			string lpwstrNew,
            [In]											int fAllowEscAbort,
            [In]											IAMCopyCaptureFileProgress pFilter);


        [PreserveSig]
        int FindPin(
            [In]											object pSource,
            [In]											int pindir,
            [In]										ref Guid pCategory,
            [In]										ref Guid pType, [In, MarshalAs(UnmanagedType.Bool)]			bool fUnconnected,
             [In]											int num,
             [Out]										out IPin ppPin);
    }

    [ComVisible(true), ComImport,
Guid("a2104830-7c70-11cf-8bce-00aa00a3f1a6"),
InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFileSinkFilter
    {
        [PreserveSig]
        int SetFileName(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string pszFileName,
            [In, MarshalAs(UnmanagedType.LPStruct)]			AMMediaType pmt);

        [PreserveSig]
        int GetCurFile(
            [Out, MarshalAs(UnmanagedType.LPWStr)]		out	string pszFileName,
            [Out, MarshalAs(UnmanagedType.LPStruct)]		AMMediaType pmt);
    }

    [ComVisible(true), ComImport,
Guid("670d1d20-a068-11d0-b3f0-00aa003761c5"),
InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAMCopyCaptureFileProgress
    {
        [PreserveSig]
        int Progress(int iProgress);
    }


    [ComVisible(true), ComImport,
    Guid("C6E13340-30AC-11d0-A18C-00A0C9118956"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAMStreamConfig
    {
        [PreserveSig]
        int SetFormat(
            [In, MarshalAs(UnmanagedType.LPStruct)]			AMMediaType pmt);

        [PreserveSig]
        int GetFormat(
            [Out, MarshalAs(UnmanagedType.LPStruct)]		AMMediaType pmt);

        [PreserveSig]
        int GetNumberOfCapabilities(out int piCount, out int piSize);

        [PreserveSig]
        int GetStreamCaps(int iIndex,
            [Out, MarshalAs(UnmanagedType.LPStruct)]	out AMMediaType ppmt,
                                                            IntPtr pSCC);
    }
}
