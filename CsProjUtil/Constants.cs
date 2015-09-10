using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsProjUtil
{
    static class PackageGuids
    {
        public const string guidCsProjUtilPackageString = "465b40b6-311a-4e37-9556-95fced2de9c6";
        public const string guidCsProjUtilCmdSetString = "279c3f9f-a7bb-4c43-8feb-68b10b3e1a88";

        public static readonly Guid guidCsProjUtilCmdSet = new Guid(guidCsProjUtilCmdSetString);
    };

    static class PackageCommanddIDs
    {
        public const uint AddSolutionDir = 0x100;
        public const uint ConvertHintPathToSolutionDir = 0x200;
        public const uint AddConfigTransform = 0x300;
    };

    static class PackageVersion
    {
        public const string Version = "1.1";
    }
}
