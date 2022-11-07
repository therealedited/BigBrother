using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBrother.Utils
{
    internal class Companion
    {
        public static unsafe uint GetCompanionOwnerID(IntPtr companion)
        {
            return Memory.PtrToCharacterStruct(companion)->CompanionOwnerID;
        }
    }
}
