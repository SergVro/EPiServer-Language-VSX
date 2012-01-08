// Guids.cs
// MUST match guids.h

using System;

namespace EPiServer.Labs.LangFilesExtension.VSPackage
{
    static class GuidList
    {
        public const string guidVSPackagePkgString = "cbd0d8c8-039d-483a-ae0e-3b0e45483737";
        public const string guidVSPackageCmdSetString = "8a3b0180-9e8a-47ef-84dd-3869ce4969c9";

        public static readonly Guid guidVSPackageCmdSet = new Guid(guidVSPackageCmdSetString);
    };
}