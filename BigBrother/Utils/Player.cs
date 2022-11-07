using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBrother.Utils
{
    public class Player
    {
        public string name;
        public int id;


        public Player(string name, int id)
        {
            this.name = name;
            this.id = id;
        }

        public static unsafe bool IsWeaponHidden(Character a)
        {
            return (Memory.PtrToCharacterStruct(a.Address)->DrawData.Flags2 & 1) == 1;
        }
    }
}
