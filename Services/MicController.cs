using System;
using System.Runtime.InteropServices;
using HotkeyStatusNotifier.Core;

namespace HotkeyStatusNotifier.Services;

public sealed class MicController
{
    private static readonly Guid AudioEndpointVolumeIid = new("5CDF2C82-841E-4546-9722-0CF74078229A");
    private const int ECapture = 1;
    private const int EConsole = 0;
    private const int ECommunications = 1;
    private const int ClsCtxInprocServer = 1;

    public bool TryGetMuted(out bool muted)
    {
        muted = false;
        try
        {
            foreach (var role in new[] { EConsole, ECommunications })
            {
                var device = GetDefaultCaptureDevice(role);
                if (device == null) continue;
                try
                {
                    var iid = AudioEndpointVolumeIid;
                    var hr = device.Activate(ref iid, ClsCtxInprocServer, IntPtr.Zero, out var volumeObj);
                    if (hr != 0 || volumeObj == null)
                    {
                        Log($"GetMuted: Activate failed hr=0x{hr:X8} role={role}");
                        continue;
                    }
                    var volume = (IAudioEndpointVolume)volumeObj;
                    try
                    {
                        hr = volume.GetMute(out var m);
                        if (hr != 0)
                        {
                            Log($"GetMuted: GetMute failed hr=0x{hr:X8} role={role}");
                            continue;
                        }
                        muted = m;
                        return true;
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(volume);
                    }
                }
                finally
                {
                    Marshal.FinalReleaseComObject(device);
                }
            }
            Log("GetMuted: no usable capture endpoint");
            return false;
        }
        catch (Exception ex)
        {
            Log($"GetMuted exception: {ex}");
            return false;
        }
    }

    public bool TrySetMuted(bool muted)
    {
        var anySuccess = false;
        try
        {
            foreach (var role in new[] { EConsole, ECommunications })
            {
                var device = GetDefaultCaptureDevice(role);
                if (device == null) continue;
                try
                {
                    var iid = AudioEndpointVolumeIid;
                    var hr = device.Activate(ref iid, ClsCtxInprocServer, IntPtr.Zero, out var volumeObj);
                    if (hr != 0 || volumeObj == null)
                    {
                        Log($"SetMuted: Activate failed hr=0x{hr:X8} role={role}");
                        continue;
                    }
                    var volume = (IAudioEndpointVolume)volumeObj;
                    try
                    {
                        hr = volume.SetMute(muted, Guid.Empty);
                        if (hr != 0)
                        {
                            Log($"SetMuted: SetMute failed hr=0x{hr:X8} role={role}");
                        }
                        else
                        {
                            anySuccess = true;
                        }
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(volume);
                    }
                }
                finally
                {
                    Marshal.FinalReleaseComObject(device);
                }
            }
            return anySuccess;
        }
        catch (Exception ex)
        {
            Log($"SetMuted exception: {ex}");
            return false;
        }
    }

    private static IMMDevice? GetDefaultCaptureDevice(int role)
    {
        try
        {
            var enumerator = (IMMDeviceEnumerator)(object)new MMDeviceEnumeratorComObject();
            try
            {
                var hr = enumerator.GetDefaultAudioEndpoint(ECapture, role, out var device);
                if (hr != 0)
                {
                    Log($"GetDefaultAudioEndpoint failed hr=0x{hr:X8} role={role}");
                    return null;
                }
                return device;
            }
            finally
            {
                Marshal.FinalReleaseComObject(enumerator);
            }
        }
        catch (Exception ex)
        {
            Log($"GetDefaultCaptureDevice exception: {ex}");
            return null;
        }
    }

    private static void Log(string message)
    {
        try
        {
            System.IO.File.AppendAllText(SettingsService.ErrorLogPath,
                $"[{DateTime.Now:O}] MicController: {message}\n");
        }
        catch
        {
        }
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    private class MMDeviceEnumeratorComObject
    {
    }

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        [PreserveSig] int EnumAudioEndpoints(int dataFlow, int dwStateMask, out IntPtr ppDevices);
        [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppEndpoint);
        [PreserveSig] int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);
        [PreserveSig] int RegisterEndpointNotificationCallback(IntPtr pClient);
        [PreserveSig] int UnregisterEndpointNotificationCallback(IntPtr pClient);
    }

    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        [PreserveSig] int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        [PreserveSig] int OpenPropertyStore(int stgmAccess, out IntPtr ppProperties);
        [PreserveSig] int GetId(out IntPtr ppstrId);
        [PreserveSig] int GetState(out int pdwState);
    }

    [ComImport, Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioEndpointVolume
    {
        [PreserveSig] int RegisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig] int UnregisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig] int GetChannelCount(out uint pnChannelCount);
        [PreserveSig] int SetMasterVolumeLevel(float fLevelDB, [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);
        [PreserveSig] int SetMasterVolumeLevelScalar(float fLevel, [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);
        [PreserveSig] int GetMasterVolumeLevel(out float pfLevelDB);
        [PreserveSig] int GetMasterVolumeLevelScalar(out float pfLevel);
        [PreserveSig] int SetChannelVolumeLevel(uint nChannel, float fLevelDB, [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);
        [PreserveSig] int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);
        [PreserveSig] int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
        [PreserveSig] int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
        [PreserveSig] int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);
        [PreserveSig] int GetMute([MarshalAs(UnmanagedType.Bool)] out bool pbMute);
    }
}
