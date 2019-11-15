using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public static class Balance
{
    public static class Actions
    {
        public static class Reaction
        {
            public static float Cost { get; set; }

            static Reaction() { Load(); }
        }

        public static class Transcription
        {
            public static float Cost { get; set; }
            public static float EnergyChange { get; set; }

            public static float UnitLength { get; set; }
            public static float InterpretaseCostMultiplier { get; set; }

            static Transcription() { Load(); }
        }

        public static class CompoundMovement
        {
            public static float Cost { get; set; }
            public static float EnergyChange { get; set; }

            static CompoundMovement() { Load(); }
        }

        public static class MembraneTransport
        {
            public static float Cost { get; set; }
            public static float EnergyChange { get; set; }

            public static Dictionary<Molecule, float> RateMultipliers { get; set; }

            static MembraneTransport() { Load(); }
        }

        public static class CellConstruction
        {
            public static float Cost { get; set; }
            public static float EnergyChange { get; set; }

            public static Dictionary<Molecule, float> Materials { get; set; }

            static CellConstruction() { Load(); }
        }

        public static class CellSeparation
        {
            public static float Cost { get; set; }
            public static float EnergyChange { get; set; }

            public static float Endowment { get; set; }

            static CellSeparation() { Load(); }
        }

        public static class Grabbing
        {
            public static float Cost { get; set; }
            public static float EnergyChange { get; set; }

            static Grabbing() { Load(); }
        }

        public static class Spinning
        {
            public static float Cost { get; set; }
            public static float EnergyChange { get; set; }

            static Spinning() { Load(); }
        }
    }


    static bool is_loaded = false;

    public static void Load()
    {
        if (is_loaded)
            return;
        is_loaded = true;

        JObject json_balance_object = JObject.Parse(FileUtility.ReadTextFile("Assets/balance.json"));

        float cost_multiplier = Utility.JTokenToFloat(json_balance_object["Actions"]["Cost Multiplier"]);

        Actions.Reaction.Cost = 1 * cost_multiplier;

        Actions.Transcription.Cost = Utility.JTokenToFloat(json_balance_object["Actions"]["Transcription"]["Cost"]) * cost_multiplier;
        Actions.Transcription.EnergyChange = Utility.JTokenToFloat(json_balance_object["Actions"]["Transcription"]["Energy Change"]);
        Actions.Transcription.UnitLength = Utility.JTokenToFloat(json_balance_object["Actions"]["Transcription"]["Unit Length"]);
        Actions.Transcription.InterpretaseCostMultiplier = Utility.JTokenToFloat(json_balance_object["Actions"]["Transcription"]["Interpretase Cost Multiplier"]);

        Actions.CompoundMovement.Cost = Utility.JTokenToFloat(json_balance_object["Actions"]["Compound Movement"]["Cost"]) * cost_multiplier;
        Actions.CompoundMovement.EnergyChange = Utility.JTokenToFloat(json_balance_object["Actions"]["Compound Movement"]["Energy Change"]);

        Actions.MembraneTransport.Cost = Utility.JTokenToFloat(json_balance_object["Actions"]["Membrane Transport"]["Cost"]) * cost_multiplier;
        Actions.MembraneTransport.EnergyChange = Utility.JTokenToFloat(json_balance_object["Actions"]["Membrane Transport"]["Energy Change"]);
        Actions.MembraneTransport.RateMultipliers = new Dictionary<Molecule, float>();
        foreach (var json_pair in json_balance_object["Actions"]["Membrane Transport"]["Rate Multipliers"] as JObject)
            Actions.MembraneTransport.RateMultipliers[Molecule.GetMolecule(json_pair.Key)] = Utility.JTokenToFloat(json_pair.Value);

        Actions.CellConstruction.Cost = Utility.JTokenToFloat(json_balance_object["Actions"]["Cell Construction"]["Cost"]) * cost_multiplier;
        Actions.CellConstruction.EnergyChange = Utility.JTokenToFloat(json_balance_object["Actions"]["Cell Construction"]["Energy Change"]);
        Actions.CellConstruction.Materials = new Dictionary<Molecule, float>();
        foreach (var json_pair in json_balance_object["Actions"]["Cell Construction"]["Materials"] as JObject)
            Actions.CellConstruction.Materials[Molecule.GetMolecule(json_pair.Key)] = Utility.JTokenToFloat(json_pair.Value);

        Actions.CellSeparation.Cost = Utility.JTokenToFloat(json_balance_object["Actions"]["Cell Separation"]["Cost"]) * cost_multiplier;
        Actions.CellSeparation.EnergyChange = Utility.JTokenToFloat(json_balance_object["Actions"]["Cell Separation"]["Energy Change"]);
        Actions.CellSeparation.Endowment = Utility.JTokenToFloat(json_balance_object["Actions"]["Cell Separation"]["Endowment"]);

        Actions.Grabbing.Cost = Utility.JTokenToFloat(json_balance_object["Actions"]["Grabbing"]["Cost"]) * cost_multiplier;
        Actions.Grabbing.EnergyChange = Utility.JTokenToFloat(json_balance_object["Actions"]["Grabbing"]["Energy Change"]);

        Actions.Spinning.Cost = Utility.JTokenToFloat(json_balance_object["Actions"]["Spinning"]["Cost"]) * cost_multiplier;
        Actions.Spinning.EnergyChange = Utility.JTokenToFloat(json_balance_object["Actions"]["Spinning"]["Energy Change"]);
    }
}
