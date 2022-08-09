using Mutagen.Bethesda.Synthesis.Settings;
using System.Collections.Generic;

namespace DisplaySpellTomeLevelPatcher
{
    public class Settings
    {
        [SynthesisTooltip(@"Specify your own format here! Available variables: <level> (ex. Adept), <spell> (ex. Clairvoyance), <plugin> (ex. ForgottenMagic_Redone), <mod> (name of the mod instead of the plugin name, ex. Forgotten Magic Redone), <school> (ex. Alteration). Default: Spell Tome (<level>): <spell>")]
        public string Format { get; set; } = "Spell Tome (<level>): <spell>";

        [SynthesisTooltip(@"Specify your own format for mod names (<mod>) here! When a plugin name wasn't found here, the patcher will try and automatically convert the plugin name to the mod name - results may vary.")]
        public Dictionary<string, string> PluginModNamePairs { get; set; } = new()
        {
            { "Skyrim.esm", "Skyrim" },
            { "Dawnguard.esm", "Dawnguard" },
            { "Dragonborn.esm", "Dragonborn" },
            { "HearthFires.esm", "HearthFires" },
            { "Apocalypse - Magic of Skyrim.esp", "Apocalypse" },
            { "Arcanum.esp", "Arcanum" },
            { "Triumvirate - Mage Archetypes.esp", "Triumvirate" },
            { "ForgottenMagic_Redone.esp", "Forgotten Magic Redone" },
            { "Phenderix Magic Evolved.esp", "Phenderix Magic Evolved" },
            { "ShadowSpellPackage.esp", "Shadow Spell Package" },
            { "PathOfTheAntiMage.esp", "Path of the Anti-Mage" }
        };

        [SynthesisTooltip(@"These are the level names that will be used for <level>. You can optionally shorten them or replace them with another name here. Default: Novice, Apprentice, Adept, Expert, Master")]
        public List<string> LevelNames { get; set; } = new() {
            "Novice",
            "Apprentice",
            "Adept",
            "Expert",
            "Master"
        };
    }
}
