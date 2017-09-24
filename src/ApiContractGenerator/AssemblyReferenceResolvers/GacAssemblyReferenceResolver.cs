using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ApiContractGenerator.AssemblyReferenceResolvers
{
    public sealed class GacAssemblyReferenceResolver : IAssemblyReferenceResolver
    {
        private IAssemblyCache globalAssemblyCache;

        public bool TryGetAssemblyPath(AssemblyName assemblyName, out string path)
        {
            if (assemblyName?.Version == null || assemblyName.GetPublicKeyToken() == null)
            {
                path = null;
                return false;
            }

            if (TryLocate(assemblyName.FullName, out path))
                return true;

            // Ran into this
            if (assemblyName.CultureName == string.Empty)
            {
                var withoutCultureNeutral = new AssemblyName(assemblyName.FullName)
                {
                    CultureName = null
                };

                return TryLocate(withoutCultureNeutral.FullName, out path);
            }

            return false;
        }

        private bool TryLocate(string name, out string path)
        {
            if (globalAssemblyCache == null)
                CreateAssemblyCache(out globalAssemblyCache, 0);

            var assemblyInfo = new ASSEMBLY_INFO(1024);
            var result = globalAssemblyCache.QueryAssemblyInfo(QUERYASMINFO_FLAG.VALIDATE, name, ref assemblyInfo);
            if (result.IsError)
            {
                if (result.Code != WinErrorCode.FileNotFound) throw result.CreateException();
                path = null;
                return false;
            }

            path = assemblyInfo.pszCurrentAssemblyPathBuf.Substring(0, assemblyInfo.cchBuf - 1);
            return true;
        }


        [DllImport("fusion.dll", PreserveSig = false)]
        private static extern void CreateAssemblyCache(out IAssemblyCache ppAsmCache, uint dwReserved);

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
        private interface IAssemblyCache
        {
            void UninstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, IntPtr pRefData, out uint pulDisposition);
            [PreserveSig]
            HResult QueryAssemblyInfo(QUERYASMINFO_FLAG dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, ref ASSEMBLY_INFO pAsmInfo);
            void CreateAssemblyCacheItem(uint dwFlags, IntPtr pvReserved, out IntPtr ppAsmItem, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName);
            object CreateAssemblyScavenger();
            void InstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszManifestFilePath, IntPtr pRefData);
        }

        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Local
        // ReSharper disable NotAccessedField.Local
        #pragma warning disable 414 // Non-accessed field
        #pragma warning disable 169 // Field is never used
        #pragma warning disable 649 // Unassigned readonly field
        #pragma warning disable IDE1006 // Naming Styles

        [Flags]
        private enum QUERYASMINFO_FLAG : uint
        {
            VALIDATE = 1,
            GETSIZE = 2
        }

        private struct ASSEMBLY_INFO
        {
            public ASSEMBLY_INFO(int cchBuf)
            {
                cbAssemblyInfo = Marshal.SizeOf<ASSEMBLY_INFO>();
                dwAssemblyFlags = 0;
                uliAssemblySizeInKB = 0;
                this.cchBuf = cchBuf;
                pszCurrentAssemblyPathBuf = new string('\0', cchBuf);
            }

            private int cbAssemblyInfo;
            private readonly uint dwAssemblyFlags;
            private readonly ulong uliAssemblySizeInKB;

            [MarshalAs(UnmanagedType.LPWStr)]
            public readonly string pszCurrentAssemblyPathBuf;

            public readonly int cchBuf;
        }
    }
}
