using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Newtonsoft.Json;
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

        public static string GenerateSpellTomeName(string spellTomeName, string level)
        {
            return spellTomeName.ToString().Replace("Spell Tome:", $"Spell Tome ({level}):");
        }

        public static string GenerateScrollName(string scrollName, string level)
        {
            return scrollName.ToString().Replace("Scroll of", $"Scroll ({level}):");
        }

        public static string GetSpellNameFromSpellTome(string spellTomeName)
        {
            try
            {
                return spellTomeName.Split(": ")[1];
            }
            catch (IndexOutOfRangeException)
            {
                return "";
            }
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
                if (book.Name?.String == null) continue;
                if (!book.Keywords?.Contains(Skyrim.Keyword.VendorItemSpellTome) ?? true) continue;
                if (book.Teaches is not IBookSpellGetter spellTeach) continue;
                if (!spellTeach.Spell.TryResolve(state.LinkCache, out var spell)) continue;
                if (!spell.HalfCostPerk.TryResolve(state.LinkCache, out var halfCostPerk)) continue;

                string spellName = GetSpellNameFromSpellTome(book.Name.String);
                if (spellName == "")
                {
                    System.Console.WriteLine($"{book.FormKey}: Could not get spell name from: {book.Name.String}");
                    continue;
                }

                System.Console.WriteLine();
                System.Console.WriteLine($"{book.FormKey}: Searching for level with perk: EDID {halfCostPerk.EditorID} and Name {halfCostPerk.Name?.String}");

                foreach (string skillLevel in skillLevels)
                {
                    if ((!halfCostPerk.Name?.String?.Contains(skillLevel) ?? true)
                        && (!halfCostPerk.EditorID?.Contains(skillLevel) ?? true))
                    {
                        continue;
                    }

                    System.Console.WriteLine($"{book.FormKey}: Registering {spellName} as {skillLevel}");
                    string generatedName = GenerateSpellTomeName(book.Name.String, skillLevel);
                    if (generatedName == book.Name.String) continue;

                    spellLevelDictionary[spellName] = skillLevel;
                    Book bookToAdd = book.DeepCopy();
                    bookToAdd.Name = generatedName;
                    state.PatchMod.Books.Set(bookToAdd);
                    break;
                }
            }

            /*
            foreach (var scroll in state.LoadOrder.PriorityOrder.Scroll().WinningOverrides())
            {
                if (scroll.Name?.String == null) continue;

                string scrollSpellName = GetSpellNameFromScroll(scroll.Name.String);
                if (spellLevelDictionary.TryGetValue(scrollSpellName, out var skillLevel))
                {
                    Scroll scrollToAdd = scroll.DeepCopy();
                    scrollToAdd.Name = GenerateScrollName(scroll.Name.String, skillLevel);
                    state.PatchMod.Scrolls.Set(scrollToAdd);
                }
            }
            */
        }
    }
}
