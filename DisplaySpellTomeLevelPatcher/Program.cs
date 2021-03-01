using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;

namespace DisplaySpellTomeLevelPatcher
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            return SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .Run(args, new RunPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "DisplaySpellTomeLevelPatcher.esp",
                        BlockAutomaticExit = true,
                        TargetRelease = GameRelease.SkyrimSE
                    }
                });
        }

        public static readonly string[] skillLevels = {
            "Novice",
            "Apprentice",
            "Adept",
            "Expert",
            "Master"
        };

        public static string AppendToSpellTome(TranslatedString spellTome, string level)
        {
            return spellTome.ToString().Replace("Spell Tome:", $"Spell Tome ({level}):");
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            foreach (var book in state.LoadOrder.PriorityOrder.WinningOverrides<IBookGetter>())
            {
                if (book.Keywords != null && book.Keywords.Contains(Skyrim.Keyword.VendorItemSpellTome))
                {
                    Book bookToModify = book.DeepCopy();
                    if (bookToModify.Teaches != null)
                    {
                        foreach (var formLink in bookToModify.Teaches.ContainedFormLinks)
                        {
                            if (state.LinkCache.TryResolve<ISpellGetter>(formLink.FormKey, out var spell))
                            {
                                if (state.LinkCache.TryResolve<IPerkGetter>(spell.HalfCostPerk.FormKey, out var halfCostPerk))
                                {
                                    foreach(string skillLevel in skillLevels)
                                    {
                                        if ((halfCostPerk.Name != null && halfCostPerk.Name.ToString()!.Contains(skillLevel)) ||
                                            (halfCostPerk.EditorID != null && halfCostPerk.EditorID.Contains(skillLevel)))
                                        {
                                            bookToModify.Name = AppendToSpellTome(bookToModify.Name!, skillLevel);
                                            state.PatchMod.Books.Add(bookToModify);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                else continue;
            }
        }
    }
}
