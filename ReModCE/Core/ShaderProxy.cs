using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReModCE.Core
{
        internal class ShaderFilterApi
        {
            public const string DllName = "DxbcShaderFilter";

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void TriBool(bool limitLoops, bool limitGeometry, bool limitTesselation);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void OneFloat(float value);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void OneInt(int value);

            private readonly TriBool _ourSetFilterState;
            private readonly OneFloat _ourSetTess;
            private readonly OneInt _ourSetLoops;
            private readonly OneInt _ourSetGeom;

            public ShaderFilterApi(IntPtr hmodule)
            {
                _ourSetFilterState = Marshal.GetDelegateForFunctionPointer<TriBool>(GetProcAddress(hmodule, nameof(SetFilteringState)));
                _ourSetTess = Marshal.GetDelegateForFunctionPointer<OneFloat>(GetProcAddress(hmodule, nameof(SetMaxTesselationPower)));
                _ourSetLoops = Marshal.GetDelegateForFunctionPointer<OneInt>(GetProcAddress(hmodule, nameof(SetLoopLimit)));
                _ourSetGeom = Marshal.GetDelegateForFunctionPointer<OneInt>(GetProcAddress(hmodule, nameof(SetGeometryLimit)));
            }

            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            public void SetFilteringState(bool limitLoops, bool limitGeometry, bool limitTesselation) => _ourSetFilterState(limitLoops, limitGeometry, limitTesselation);
            public void SetMaxTesselationPower(float maxTesselation) => _ourSetTess(maxTesselation);
            public void SetLoopLimit(int limit) => _ourSetLoops(limit);
            public void SetGeometryLimit(int limit) => _ourSetGeom(limit);
        }
}
