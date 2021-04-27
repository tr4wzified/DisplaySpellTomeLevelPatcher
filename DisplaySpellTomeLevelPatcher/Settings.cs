using Mutagen.Bethesda.Synthesis.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplaySpellTomeLevelPatcher
{
    public class Settings
    {
        [SynthesisTooltip(@"Specify your own format here! Available variables: <level> (ex. Adept), <spell> (ex. Clairvoyance), <plugin> (ex. Skyrim), <school> (ex. Alteration). Default: Spell Tome (<level>): <spell>")]
        public string Format { get; set; } = "Spell Tome (<level>): <spell> from <plugin> with school <school>";
    }
}
