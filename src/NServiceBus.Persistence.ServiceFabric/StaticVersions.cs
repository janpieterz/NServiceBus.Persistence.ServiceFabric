﻿namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    static class StaticVersions
    {
        public static readonly Version PersistenceVersion;

        static StaticVersions()
        {
            var assembly = Assembly.GetExecutingAssembly();
            PersistenceVersion = GetFileVersion(assembly);
        }

        internal static Version GetFileVersion(this Assembly assembly)
        {
            var version = FileVersionInfo.GetVersionInfo(assembly.Location);
            return new Version(
                version.FileMajorPart,
                version.FileMinorPart,
                version.FileBuildPart,
                version.FilePrivatePart);
        }
    }
}