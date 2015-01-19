// Guids.cs
// MUST match guids.h
using System;

namespace Company.VSPackage1
{
    static class GuidList
    {
        public const string guidVSPackage1PkgString = "0bb1206c-5a66-4093-8abd-1ac706f50043";
        public const string guidVSPackage1CmdSetString = "7feb51b0-4eda-4efc-9a42-a145a61ea191";

        public static readonly Guid guidVSPackage1CmdSet = new Guid(guidVSPackage1CmdSetString);

        //public static readonly Guid guidTopLevelMenu = new Guid("0C7E2B5D-036B-422E-8346-26AE4F1D5651");
    };
}