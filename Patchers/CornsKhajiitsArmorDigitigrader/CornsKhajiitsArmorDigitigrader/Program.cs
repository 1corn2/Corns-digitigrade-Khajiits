using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;

namespace CornsKhajiitsArmorDigitigrader
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "YourPatcher.esp")
                .Run(args);
        }

        const string MY_MARKER = "corns_khajiits";

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Console.WriteLine(state.LoadOrder.PriorityOrder);
            var mainmodkey = ModKey.FromFileName("corns_khajiits.esp");
            var lcache = state.LinkCache;
            foreach (var armorAddon in state.LoadOrder.PriorityOrder.ArmorAddon().WinningContextOverrides())
            {
                // skip mods where we already win anyway
                if (armorAddon.ModKey.Name.Contains(MY_MARKER)) continue;
                if (armorAddon.Record.EditorID != null && (armorAddon.Record.EditorID.Contains("Boots") ||
                                                           armorAddon.Record.EditorID.Contains("Shoes")))
                {
                    Console.WriteLine(armorAddon.ModKey + " " + armorAddon.Record.EditorID);
                    // footwear where we have not won - check if we care about it
                    IArmorAddonGetter? ourgetter = null;
                    foreach (var parentmod in lcache.ResolveAllContexts<IArmorAddon, IArmorAddonGetter>(armorAddon
                                 .Record.ToLink().FormKey))
                    {
                        Console.WriteLine(parentmod.ModKey);
                        if (parentmod.ModKey.Name.Contains(MY_MARKER))
                        {
                            ourgetter = parentmod.Record;
                            break;
                        }
                    }

                    if (ourgetter != null)
                    {
                        // footwear we care about and where we do not win
                        var winner = armorAddon.Record;
                        var racerec = winner.Race.Resolve(lcache);
                        var winnermodifier = state.PatchMod.ArmorAddons.GetOrAddAsOverride(winner);
                        if (racerec.Name != null) // better ignore invalid records like this
                        {
                            if ((racerec.Name.String ?? "").Contains("Khajiit"))
                            {
                                // This armoraddon has Khajiit as a primary race, and we tried to modify it, but lost =>
                                // Grab the winner, but overwrite everything we care about with our stuff
                                Console.WriteLine("Fixing khajiit footwear...");
                            }
                            else
                            {
                                // This does not have Khajiit as a primary race, so we just want to make sure
                                // it does not add back Khajiits as alternate races
                                Console.WriteLine("Fixing non-khajiit footwear...");
                                winnermodifier.AdditionalRaces.Remove(Skyrim.Race.KhajiitRace);
                                winnermodifier.AdditionalRaces.Remove(Skyrim.Race.KhajiitRaceVampire);
                            }
                        }
                    }

                    Console.WriteLine("======");
                }
            }
        }
    }
}