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
using System.Text.RegularExpressions;

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
       
        public static Dictionary<string, string> spellLevelDictionary = new Dictionary<string, string>();

        public static string GenerateSpellTomeName(TranslatedString spellTomeName, string level)
        {
            return spellTomeName.ToString().Replace("Spell Tome:", $"Spell Tome ({level}):");
        }

        public static string GenerateScrollName(TranslatedString scrollName, string level)
        {
            return scrollName.ToString().Replace("Scroll of", $"Scroll ({level}):");
        }

        public static string GetSpellNameFromSpellTome(string spellTomeName)
        {
            return spellTomeName.Split(": ")[1];
        }

        public static string GetSpellNameFromScroll(string scrollName)
        {
            string[] splitScrollName = scrollName.Split(' ');
            string scrollSpellName = string.Join(' ', splitScrollName.Skip(2).ToArray());
            return scrollSpellName;
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            foreach (var book in state.LoadOrder.PriorityOrder.Book().WinningOverrides())
            {
                if (book.Keywords != null && book.Keywords.Contains(Skyrim.Keyword.VendorItemSpellTome))
                {
                    if (book.Teaches != null)
                    {
                        foreach (var formLink in book.Teaches.ContainedFormLinks)
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
                                            spellLevelDictionary.Add(GetSpellNameFromSpellTome(book.Name!.ToString()!), skillLevel);

                                            Book bookToAdd = book.DeepCopy();
                                            bookToAdd.Name = GenerateSpellTomeName(bookToAdd.Name!, skillLevel);
                                            state.PatchMod.Books.Add(bookToAdd);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                else continue;
            }

            foreach(var scroll in state.LoadOrder.PriorityOrder.Scroll().WinningOverrides())
            {
                if (scroll.Name == null) continue;

                string scrollSpellName = GetSpellNameFromScroll(scroll.Name.ToString()!);
                if (spellLevelDictionary.TryGetValue(scrollSpellName, out var skillLevel))
                {
                    Scroll scrollToAdd = scroll.DeepCopy();
                    scrollToAdd.Name = GenerateScrollName(scrollToAdd.Name!, skillLevel);
                    state.PatchMod.Scrolls.Add(scrollToAdd);
                }
            }
            // debug
            foreach(KeyValuePair<string, string> keyValuePair in spellLevelDictionary)
            {
                Console.WriteLine($"{keyValuePair.Key}: { keyValuePair.Value}");
            }
        }
    }
}
