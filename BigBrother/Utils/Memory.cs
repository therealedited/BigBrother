using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBrother.Utils
{
    internal class Memory
    {
        public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.Character* PtrToCharacterStruct(IntPtr charPtr)
        {
            return (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)charPtr;
        }
    }
}
