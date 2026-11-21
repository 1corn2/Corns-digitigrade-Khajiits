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

		private const string MyMarker = "corns_khajiits";

		public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
		{
			Console.WriteLine("======");
			var mainmodkey = ModKey.FromFileName("corns_khajiits.esp");
			var lcache = state.LinkCache;
			foreach (var armorAddon in state.LoadOrder.PriorityOrder.ArmorAddon().WinningContextOverrides())
			{
				// skip mods where we already win anyway
				if (armorAddon.ModKey.Name.Contains(MyMarker)) continue;
				if (armorAddon.Record.EditorID == null || (!armorAddon.Record.EditorID.Contains("Boots") &&
				                                           !armorAddon.Record.EditorID.Contains("Shoes"))) continue;

				Console.WriteLine(armorAddon.ModKey + " " + armorAddon.Record.EditorID);
				// footwear where we have not won - check if we care about it
				IArmorAddonGetter? ourgetter = null;
				foreach (var parentmod in lcache.ResolveAllContexts<IArmorAddon, IArmorAddonGetter>
					         (armorAddon.Record.ToLink().FormKey))
				{
					Console.WriteLine(parentmod.ModKey);
					if (!parentmod.ModKey.Name.Contains(MyMarker)) continue;
					// our mod found in parents, break
					ourgetter = parentmod.Record;
					break;
				}

				if (ourgetter != null)
				{
					// footwear we care about and where we do not win
					var winner = armorAddon.Record;
					var racerec = winner.Race.Resolve(lcache);
					if (racerec.Name != null) // better ignore invalid records like this
					{
						if (!(racerec.Name.String ?? "").Contains("Khajiit"))
						{
							// This does not have Khajiit as a primary race, so we just want to make sure
							// it does not add back Khajiits as alternate races
							Console.WriteLine("Fixing non-khajiit footwear...");
							var winnermodifier = state.PatchMod.ArmorAddons.GetOrAddAsOverride(winner);
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