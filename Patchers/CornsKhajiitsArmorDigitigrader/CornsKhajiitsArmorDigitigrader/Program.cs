using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
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
				.SetTypicalOpen(GameRelease.SkyrimSE, "CornsPatcher.esp")
				.Run(args);
		}

		private const string MyMarker = "corns_khajiits";

		public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
		{
			var lcache = state.LinkCache;
			foreach (var armorAddon in state.LoadOrder.PriorityOrder.ArmorAddon().WinningContextOverrides())
			{
				// skip mods where we already win anyway
				if (armorAddon.ModKey.Name.Contains(MyMarker)) continue;
				if (armorAddon.Record.EditorID == null || (!armorAddon.Record.EditorID.Contains("Boots") &&
				                                           !armorAddon.Record.EditorID.Contains("Shoes"))) continue;

				// footwear where we have not won - check if we care about it
				var ourgetter =
					(from parentmod in
							lcache.ResolveAllContexts<IArmorAddon, IArmorAddonGetter>(
								armorAddon.Record.ToLink().FormKey)
						where parentmod.ModKey.Name.Contains(MyMarker)
						select parentmod.Record).FirstOrDefault();

				if (ourgetter == null) continue;
				// footwear we care about and where we do not win
				var winner = armorAddon.Record;
				var racerec = winner.Race.Resolve(lcache);
				if (racerec.Name == null || (racerec.Name.String ?? "").Contains("Khajiit")) continue;
				// better ignore invalid records like this
				// This does not have Khajiit as a primary race, so we just want to make sure
				// it does not add back Khajiits as alternate races
				Console.WriteLine("Fixing non-khajiit footwear: " + armorAddon.Record);
				var winnermodifier = state.PatchMod.ArmorAddons.GetOrAddAsOverride(winner);
				winnermodifier.AdditionalRaces.Remove(Skyrim.Race.KhajiitRace);
				winnermodifier.AdditionalRaces.Remove(Skyrim.Race.KhajiitRaceVampire);
			}
		}
	}
}