using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Vanara.PInvoke;

namespace NativeInterop.Win32
{
    public class DpiHelper
    {
        /*
        * OS reports DPI scaling values in relative terms, and not absolute terms.
        * eg. if current DPI value is 250%, and recommended value is 200%, then
        * OS will give us integer 2 for DPI scaling value (starting from recommended
        * DPI scaling move 2 steps to the right in this list).
        * values observed (and extrapolated) from system settings app (immersive control panel).
        */
        public readonly static uint[] DpiVals =
        {
            100,
            125,
            150,
            175,
            200,
            225,
            250,
            300,
            350,
            400,
            450,
            500
        };

        /*
        * @brief : Use QueryDisplayConfig() to get paths, and modes.
        * @param[out] pathsV : reference to a vector which will contain paths
        * @param[out] modesV : reference to a vector which will contain modes
        * @param[in] flags : determines the kind of paths to retrieve (only active paths by default)
        * return : false in case of failure, else true
        */
        public static bool GetPathsAndModes(
            out IReadOnlyList<Gdi32.DISPLAYCONFIG_PATH_INFO> pathsV,
            out IReadOnlyList<Gdi32.DISPLAYCONFIG_MODE_INFO> modesV,
            User32.QDC flags = User32.QDC.QDC_ONLY_ACTIVE_PATHS
        )
        {
            pathsV = Array.Empty<Gdi32.DISPLAYCONFIG_PATH_INFO>();
            modesV = Array.Empty<Gdi32.DISPLAYCONFIG_MODE_INFO>();

            uint numPaths = 0;
            uint numModes = 0;
            var status = Vanara.PInvoke.User32.GetDisplayConfigBufferSizes(
                flags,
                out numPaths,
                out numModes
            );
            if (status != Win32Error.ERROR_SUCCESS)
            {
                return false;
            }

            Gdi32.DISPLAYCONFIG_PATH_INFO[] paths = new Gdi32.DISPLAYCONFIG_PATH_INFO[numPaths];
            Gdi32.DISPLAYCONFIG_MODE_INFO[] modes = new Gdi32.DISPLAYCONFIG_MODE_INFO[numModes];

            status = Vanara.PInvoke.User32.QueryDisplayConfig(
                flags,
                ref numPaths,
                paths,
                ref numModes,
                modes
            );
            if (status != Win32Error.ERROR_SUCCESS)
            {
                return false;
            }

            pathsV = paths;
            modesV = modes;

            return true;
        }

        //out own enum, similar to DISPLAYCONFIG_DEVICE_INFO_TYPE enum in wingdi.h
        public enum DISPLAYCONFIG_DEVICE_INFO_TYPE_CUSTOM : int
        {
            DISPLAYCONFIG_DEVICE_INFO_GET_DPI_SCALE = -3, //returns min, max, suggested, and currently applied DPI scaling values.
            DISPLAYCONFIG_DEVICE_INFO_SET_DPI_SCALE = -4 //set current dpi scaling value for a display
        }

        /*
        * struct DISPLAYCONFIG_SOURCE_DPI_SCALE_GET
        * @brief used to fetch min, max, suggested, and currently applied DPI scaling values.
        * All values are relative to the recommended DPI scaling value
        * Note that DPI scaling is a property of the source, and not of target.
        */
        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_SOURCE_DPI_SCALE_GET
        {
            public Gdi32.DISPLAYCONFIG_DEVICE_INFO_HEADER header;

            /*
            * @brief min value of DPI scaling is always 100, minScaleRel gives no. of steps down from recommended scaling
            * eg. if minScaleRel is -3 => 100 is 3 steps down from recommended scaling => recommended scaling is 175%
            */
            public int minScaleRel;

            /*
            * @brief currently applied DPI scaling value wrt the recommended value. eg. if recommended value is 175%,
            * => if curScaleRel == 0 the current scaling is 175%, if curScaleRel == -1, then current scale is 150%
            */
            public int curScaleRel;

            /*
            * @brief maximum supported DPI scaling wrt recommended value
            */
            public int maxScaleRel;
        }

        /*
        * struct DISPLAYCONFIG_SOURCE_DPI_SCALE_SET
        * @brief set DPI scaling value of a source
        * Note that DPI scaling is a property of the source, and not of target.
        */
        public struct DISPLAYCONFIG_SOURCE_DPI_SCALE_SET
        {
            public Gdi32.DISPLAYCONFIG_DEVICE_INFO_HEADER header;

            /*
            * @brief The value we want to set. The value should be relative to the recommended DPI scaling value of source.
            * eg. if scaleRel == 1, and recommended value is 175% => we are trying to set 200% scaling for the source
            */
            public int scaleRel;
        }

        /*
        * struct DPIScalingInfo
        * @brief DPI info about a source
        * mininum :     minumum DPI scaling in terms of percentage supported by source. Will always be 100%.
        * maximum :     maximum DPI scaling in terms of percentage supported by source. eg. 100%, 150%, etc.
        * current :     currently applied DPI scaling value
        * recommended : DPI scaling value reommended by OS. OS takes resolution, physical size, and expected viewing distance
        *               into account while calculating this, however exact formula is not known, hence must be retrieved from OS
        *               For a system in which user has not explicitly changed DPI, current should eqaul recommended.
        * bInitDone :   If true, it means that the members of the struct contain values, as fetched from OS, and not the default
        *               ones given while object creation.
        */
        public struct DPIScalingInfo
        {
            public uint mininum;
            public uint maximum;
            public uint current;
            public uint recommended;
            public bool bInitDone;

            public static readonly DPIScalingInfo Default = new DPIScalingInfo()
            {
                mininum = 100,
                maximum = 100,
                recommended = 100,
                current = 100,
                bInitDone = false,
            };
        }

        [DllImport("user32.dll")]
        public static extern Win32Error DisplayConfigGetDeviceInfo(
            ref DISPLAYCONFIG_SOURCE_DPI_SCALE_GET deviceName
        );

        [DllImport("user32.dll")]
        public static extern Win32Error DisplayConfigGetDeviceInfo(
            ref Gdi32.DISPLAYCONFIG_TARGET_DEVICE_NAME deviceName
        );

        public struct DisplayName
        {
            public string name;
            public bool internalDisplay;
        }

        public static DpiHelper.DPIScalingInfo GetDPIScalingInfo(ulong adapterID, uint sourceID)
        {
            DPIScalingInfo dpiInfo = DPIScalingInfo.Default;

            DpiHelper.DISPLAYCONFIG_SOURCE_DPI_SCALE_GET requestPacket =
                new DISPLAYCONFIG_SOURCE_DPI_SCALE_GET();
            requestPacket.header.type = unchecked(
                (Gdi32.DISPLAYCONFIG_DEVICE_INFO_TYPE)DpiHelper.DISPLAYCONFIG_DEVICE_INFO_TYPE_CUSTOM.DISPLAYCONFIG_DEVICE_INFO_GET_DPI_SCALE
            );
            requestPacket.header.size =
                (uint)Marshal.SizeOf<DpiHelper.DISPLAYCONFIG_SOURCE_DPI_SCALE_GET>();
            Debug.Assert(0x20 == Marshal.SizeOf<DpiHelper.DISPLAYCONFIG_SOURCE_DPI_SCALE_GET>()); //if this fails => OS has changed somthing, and our reverse enginnering knowledge about the API is outdated
            requestPacket.header.adapterId = adapterID;
            requestPacket.header.id = sourceID;

            var res = DisplayConfigGetDeviceInfo(ref requestPacket);
            if (res == Win32Error.ERROR_SUCCESS)
            {
                //success
                if (requestPacket.curScaleRel < requestPacket.minScaleRel)
                {
                    requestPacket.curScaleRel = requestPacket.minScaleRel;
                }
                else if (requestPacket.curScaleRel > requestPacket.maxScaleRel)
                {
                    requestPacket.curScaleRel = requestPacket.maxScaleRel;
                }

                int minAbs = Math.Abs((int)requestPacket.minScaleRel);
                if (DpiVals.Length >= (minAbs + requestPacket.maxScaleRel + 1))
                {
                    //all ok
                    dpiInfo.current = DpiVals[minAbs + requestPacket.curScaleRel];
                    dpiInfo.recommended = DpiVals[minAbs];
                    dpiInfo.maximum = DpiVals[minAbs + requestPacket.maxScaleRel];
                    dpiInfo.bInitDone = true;
                }
                else
                {
                    return dpiInfo;
                }
            }
            else
            {
                return dpiInfo;
            }

            return dpiInfo;
        }

        public static DisplayName GetDisplayName(ulong adapterID, uint sourceID, uint targetId)
        {
            Gdi32.DISPLAYCONFIG_TARGET_DEVICE_NAME deviceName =
                new Gdi32.DISPLAYCONFIG_TARGET_DEVICE_NAME();
            deviceName.header.size = (uint)Marshal.SizeOf<Gdi32.DISPLAYCONFIG_TARGET_DEVICE_NAME>();
            deviceName.header.type =
                Gdi32.DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
            deviceName.header.adapterId = adapterID;
            deviceName.header.id = targetId;
            if (DisplayConfigGetDeviceInfo(ref deviceName) != Win32Error.ERROR_SUCCESS)
            {
                throw new Exception("Failed to query config of display!");
            }
            else
            {
                return new DisplayName()
                {
                    name = deviceName.monitorFriendlyDeviceName,
                    internalDisplay =
                        deviceName.outputTechnology
                        == Gdi32.DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL,
                };
            }
        }

        [DllImport("User32.dll")]
        private static extern Win32Error DisplayConfigSetDeviceInfo(
            ref DISPLAYCONFIG_SOURCE_DPI_SCALE_SET requestPacket
        );

        public static bool SetDPIScaling(ulong adapterID, uint sourceID, uint dpiPercentToSet)
        {
            DPIScalingInfo dPIScalingInfo = GetDPIScalingInfo(adapterID, sourceID);

            if (dpiPercentToSet == dPIScalingInfo.current)
            {
                return true;
            }

            if (dpiPercentToSet < dPIScalingInfo.mininum)
            {
                dpiPercentToSet = dPIScalingInfo.mininum;
            }
            else if (dpiPercentToSet > dPIScalingInfo.maximum)
            {
                dpiPercentToSet = dPIScalingInfo.maximum;
            }

            int idx1 = -1;
            int idx2 = -1;

            int i = 0;
            foreach (var val in DpiVals)
            {
                if (val == dpiPercentToSet)
                {
                    idx1 = i;
                }

                if (val == dPIScalingInfo.recommended)
                {
                    idx2 = i;
                }

                i++;
            }

            if ((idx1 == -1) || (idx2 == -1))
            {
                //Error cannot find dpi value
                return false;
            }

            int dpiRelativeVal = idx1 - idx2;

            DpiHelper.DISPLAYCONFIG_SOURCE_DPI_SCALE_SET setPacket =
                new DpiHelper.DISPLAYCONFIG_SOURCE_DPI_SCALE_SET();
            setPacket.header.adapterId = adapterID;
            setPacket.header.id = sourceID;
            setPacket.header.size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DPI_SCALE_SET>();
            Debug.Assert(0x18 == Marshal.SizeOf<DpiHelper.DISPLAYCONFIG_SOURCE_DPI_SCALE_SET>()); //if this fails => OS has changed somthing, and our reverse enginnering knowledge about the API is outdated
            setPacket.header.type = unchecked(
                (Gdi32.DISPLAYCONFIG_DEVICE_INFO_TYPE)DpiHelper.DISPLAYCONFIG_DEVICE_INFO_TYPE_CUSTOM.DISPLAYCONFIG_DEVICE_INFO_SET_DPI_SCALE
            );
            setPacket.scaleRel = dpiRelativeVal;

            var res = DisplayConfigSetDeviceInfo(ref setPacket);
            if (res == Win32Error.ERROR_SUCCESS)
            {
                return true;
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
