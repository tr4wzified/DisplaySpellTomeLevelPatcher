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
                                    if (halfCostPerk.Name != null)
                                    {
                                        bool modified = false;
                                        if ((halfCostPerk.Name != null && halfCostPerk.Name.ToString()!.Contains("Novice")) ||
                                            (halfCostPerk.EditorID != null && halfCostPerk.EditorID.Contains("Novice")))
                                        {
                                            bookToModify.Name = bookToModify.Name!.ToString().Replace("Spell Tome:", "Spell Tome (Novice):");
                                            modified = true;
                                        }
                                        else if ((halfCostPerk.Name != null && halfCostPerk.Name.ToString()!.Contains("Apprentice")) ||
                                            (halfCostPerk.EditorID != null && halfCostPerk.EditorID.Contains("Apprentice")))
                                        {
                                            bookToModify.Name = bookToModify.Name!.ToString().Replace("Spell Tome:", "Spell Tome (Apprentice):");
                                            modified = true;
                                        }
                                        else if ((halfCostPerk.Name != null && halfCostPerk.Name.ToString()!.Contains("Adept")) ||
                                            (halfCostPerk.EditorID != null && halfCostPerk.EditorID.Contains("Adept")))
                                        {
                                            bookToModify.Name = bookToModify.Name!.ToString().Replace("Spell Tome:", "Spell Tome (Adept):");
                                            modified = true;
                                        }
                                        else if ((halfCostPerk.Name != null && halfCostPerk.Name.ToString()!.Contains("Expert")) ||
                                            (halfCostPerk.EditorID != null && halfCostPerk.EditorID.Contains("Expert")))
                                        {
                                            bookToModify.Name = bookToModify.Name!.ToString().Replace("Spell Tome:", "Spell Tome (Expert):");
                                            modified = true;
                                        }
                                        else if ((halfCostPerk.Name != null && halfCostPerk.Name.ToString()!.Contains("Master")) ||
                                            (halfCostPerk.EditorID != null && halfCostPerk.EditorID.Contains("Master")))
                                        {
                                            bookToModify.Name = bookToModify.Name!.ToString().Replace("Spell Tome:", "Spell Tome (Master):");
                                            modified = true;
                                        }

                                        Console.WriteLine(book.Name + " => " + bookToModify.Name);

                                        if (modified)
                                            state.PatchMod.Books.Add(bookToModify);
                                    }
                                }
                            }
                            Console.WriteLine(formLink);
                        }
                    }

                }
                else continue;
            }
        }
    }
}
