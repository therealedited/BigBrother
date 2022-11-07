using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBrother.Utils
{
    internal class SoundComboBoxItem
    {
        public string? name { get; set; }
        public Sounds sound { get; set; }

        public SoundComboBoxItem(Sounds sound, string name)
        {
            this.sound = sound;
            this.name = name;
        }
    }
}
