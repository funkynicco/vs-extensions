// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace Company.VSPackage1
{
    static class PkgCmdIDList
    {
        public const uint cmdidMyCommand = 0x100;
        public const uint cmdidMyCommand2 = 0x101;
        public const uint TopLevelMenu = 0x102;

        public const uint cmdidOpenSolutionFile = 0x103;

        // move 5 lines up and down with select
        public const uint cmdidMove5LinesUpSelect = 0x104;
        public const uint cmdidMove5LinesDownSelect = 0x105;
    };
}