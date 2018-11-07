using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;


public class Reaction
{
    static Dictionary<string, Reaction> reactions= new Dictionary<string, Reaction>();

    public static List<Reaction> Reactions
    {
        get { return new List<Reaction>(reactions.Values); }
    }

    public static Reaction GetReaction(string name)
    {
        return reactions[name];
    }

    public static void LoadReactionFile(string filename)
    {
        JObject reactions_file = JObject.Parse(Resources.Load<TextAsset>(filename).text);

        if (reactions_file["Molecules"] != null)
        {
            JObject molecules = reactions_file["Molecules"] as JObject;
            foreach (var molecule in molecules)
                Molecule.RegisterNamedMolecule(molecule.Key, new SimpleMolecule(molecule.Value["Formula"].ToString(),
                                                                                Utility.JTokenToFloat(molecule.Value["Enthalpy"]),
                                                                                Utility.JTokenToInt(molecule.Value["Charge"])));
        }


        if (reactions_file["Reactions"] == null)
            return;

        JObject reactions = reactions_file["Reactions"] as JObject;
        foreach (var reaction_pair in reactions)
        {
            bool failed = false;

            string reaction_name = reaction_pair.Key;
            JObject reaction = reaction_pair.Value as JObject;

            Dictionary<Compound, float> reactants = new Dictionary<Compound, float>();
            if (reaction["Reactants"] != null)
            {
                foreach (var reactant in reaction["Reactants"] as JObject)
                    if (Molecule.DoesMoleculeExist(reactant.Key))
                        reactants[new Compound(Molecule.GetMolecule(reactant.Key),
                                               Utility.JTokenToFloat(reactant.Value["Quantity"]))]
                        = Utility.JTokenToFloat(reactant.Value["Slotted"]);
                    else
                        failed = true;
            }
            else
                failed = true;

            Dictionary<Compound, float> products = new Dictionary<Compound, float>();
            if (reaction["Products"] != null)
            {
                foreach (var product in reaction["Products"] as JObject)
                    if (Molecule.DoesMoleculeExist(product.Key))
                        products[new Compound(Molecule.GetMolecule(product.Key),
                                              Utility.JTokenToFloat(product.Value["Quantity"]))]
                        = Utility.JTokenToFloat(product.Value["Slotted"]);
                    else
                        failed = true;
            }
            else
                failed = true;

            Dictionary<Molecule, float> inhibitors = new Dictionary<Molecule, float>();
            if (reaction["Inhibitors"] != null)
                foreach (var inhibitor in reaction["Inhibitors"] as JObject)
                    if (Molecule.DoesMoleculeExist(inhibitor.Key))
                        inhibitors[Molecule.GetMolecule(inhibitor.Key)] = Utility.JTokenToFloat(inhibitor.Value);
                    else
                        failed = true;

            Dictionary<Molecule, float> cofactors = new Dictionary<Molecule, float>();
            if (reaction["Cofactors"] != null)
                foreach (var cofactor in reaction["Cofactors"] as JObject)
                    if (Molecule.DoesMoleculeExist(cofactor.Key))
                        cofactors[Molecule.GetMolecule(cofactor.Key)] = Utility.JTokenToFloat(cofactor.Value);
                    else
                        failed = true;

            if (failed)
                continue;

            new Reaction(reaction_name, reaction["Catalyst Name"].ToString(), 
                         reactants, products,
                         Utility.JTokenToFloat(reaction     ["Cost"],                   0.1f),
                         Utility.JTokenToFloat(reaction     ["Ribozyme"],               0.3f),
                         Utility.JTokenToFloat(reaction     ["Optimal Temperature"],    298),
                         Utility.JTokenToFloat(reaction     ["Temperature Tolerance"],  1),
                         Utility.JTokenToBool(reaction      ["Thermophilic"],           false),
                         Utility.JTokenToBool(reaction      ["Cryophilic"],             false),
                         Utility.JTokenToFloat(reaction     ["Optimal pH"],             7),
                         Utility.JTokenToFloat(reaction     ["pH Tolerance"],           1),
                         Utility.JTokenToFloat(reaction     ["Potential"],              1),
                         Utility.JTokenToFloat(reaction     ["Flexibility"],            1),
                         Utility.JTokenToFloat(reaction     ["Productivity"],           1), 
                         inhibitors, cofactors);
        }
    }

    static Reaction()
    {
        LoadReactionFile("reactions");
    }


    class Attribute
    {
        static Function score_penalty= new NormalDistribution(0, 0.5f);

        ProbabilityDistribution value_distribution;
        float base_weight;

        //Consider removing this after we improve implementaion/ can test performance costs in real world
        float baked_value;
        bool baked = false;

        float percentile;

        public float Value
        {
            get
            {
                if (baked != true)
                {
                    baked_value = value_distribution.GetSample(percentile);
                    baked = true;
                }

                return baked_value;
            }
        }

        public float DefaultValue { get { return value_distribution.GetSample(0.5f); } }
        public float MinValue { get { return value_distribution.GetSample(0.0f); } }
        public float MaxValue { get { return value_distribution.GetSample(1.0f); } }

        public bool IsTrue { get { return Value > 0; } }
        public bool DefaultIsTrue { get { return DefaultValue > 0; } }

        public float BaseWeight { get { return base_weight; } }
        public float Weight
        {
            //This implementation makes changes close to the mean (<1 standard deviation)
            //cheap, but sharply increase cost as you tend towards being an outlier
            get { return base_weight * (1 - score_penalty.Compute(percentile - 0.5f) / score_penalty.Compute(0)); }

            set
            {
                int iterations = 0;
                float ratio = value / Weight;

                while ((ratio > 1.01f || ratio < 0.99f) && iterations < 5)
                {
                    percentile *= ratio;
                    ratio = value / Weight;
                }
            }
        }

        public float Percentile { get { return percentile; } }

        public Attribute(ProbabilityDistribution value_distribution_, float base_weight_)
        {
            value_distribution = value_distribution_;
            base_weight = base_weight_;

            percentile = 0.5f;
        }

        public void Mutate(float rate)
        {
            percentile = Mathf.Lerp(percentile, Random.value, 1 - Mathf.Pow(0.5f, rate* 0.65f));
            baked = false;
        }
    }

    class PotentialFunction : ProbabilityDistribution
    {
        public static float BasePotential { get { return 0.25f; } }
        public static float BaseFlexibility { get { return 0.125f; } }

        ProbabilityDistribution base_distribution;
        Reaction reaction;

        public override float Minimum
        {
            get { return base_distribution.Minimum * reaction.GetMaxWeight(); }
        }

        public override float Maximum
        {
            get { return base_distribution.Maximum * reaction.GetMaxWeight(); }
        }

        public PotentialFunction(float potential, float flexibility, Reaction reaction_)
        {
            potential = Mathf.Clamp(potential, 0, 2);
            flexibility = Mathf.Clamp(flexibility, 0, potential * (BasePotential/ BaseFlexibility));

            base_distribution = new SkewedNormalDistribution(potential * BasePotential,
                                                            flexibility * BaseFlexibility,
                                                            flexibility < 1 ? 1.5f : 1.5f * Mathf.Sqrt(flexibility));

            reaction = reaction_;
        }

        public override float Compute(float value)
        {
            if (value < Minimum)
                return 0;

            return base_distribution.Compute(value/ reaction.GetMaxWeight());
        }
    }

    class ProductivityFunction : ProbabilityDistribution
    {
        Function productivity_factor = new NormalDistribution(0, 0.5f);
        float productivity;

        public override float Minimum
        {
            get { return 1/ 1.5f; }
        }

        public override float Maximum
        {
            get { return 1.5f; }
        }

        public ProductivityFunction(float productivity_)
        {
            productivity = productivity_;
        }

        public override float Compute(float value)
        {
            return productivity_factor.Compute((value < 1 ? (1 / value) : value) - 1);
        }
    }


    string catalyst_name;

    Dictionary<Compound, Attribute> reactants= new Dictionary<Compound, Attribute>();
    Dictionary<Compound, Attribute> products = new Dictionary<Compound, Attribute>();
    float cost;

    Dictionary<Molecule, Attribute> inhibitors = new Dictionary<Molecule, Attribute>();
    Dictionary<Molecule, Attribute> cofactors = new Dictionary<Molecule, Attribute>();

    Dictionary<int, Attribute> slot_order = new Dictionary<int, Attribute>();

    Attribute is_ribozyme;
    Attribute optimal_temperature;
    Attribute temperature_tolerance;
    Attribute optimal_pH;
    Attribute pH_tolerance;
   
    Attribute productivity;
    Attribute potential;

    public string Name
    {
        get
        {
            foreach (string name in reactions.Keys)
                if (reactions[name] == this)
                    return name;

            Debug.Assert(false, "Reaction not found in dictionary");
            return "Unnamed";
        }
    }

    public Reaction( string name, string catalyst_name_,
                     Dictionary<Compound, float> reactants_,
                     Dictionary<Compound, float> products_,
                     float cost_,
                     float ribozyme_probability,
                     float mean_optimal_temperature,
                     float relative_temperature_tolerance,
                     bool thermophilic, 
                     bool cryophilic,
                     float mean_optimal_pH,
                     float relative_pH_tolerance,
                     float potential_,
                     float flexibility, 
                     float productivity_,
                     Dictionary<Molecule, float> inhibitors_= null, 
                     Dictionary<Molecule, float> cofactors_= null)
    {
        catalyst_name = catalyst_name_;

        foreach (Compound compound in reactants_.Keys)
            reactants[compound] = new Attribute(new ChoiceFunction(reactants_[compound]), 3);

        foreach (Compound compound in products_.Keys)
            products[compound] = new Attribute(new ChoiceFunction(products_[compound]), 3);

        cost = cost_;

        for (int i = 1; i < 6; i++)
            slot_order[i] = new Attribute(new UniformDistribution(), 0);

        is_ribozyme = new Attribute(new ChoiceFunction(ribozyme_probability), 0);

        if (thermophilic || cryophilic)
        {
            //Change variable name
            bool equally_thermophilic = thermophilic && cryophilic;

            optimal_temperature = new Attribute(new SkewedNormalDistribution(mean_optimal_temperature, 
                                                                             equally_thermophilic ? 30 : 20, 
                                                                             equally_thermophilic ? 1 : cryophilic ? 0.5f : 2.0f), 1);
            temperature_tolerance = new Attribute(new NormalDistribution(0, relative_temperature_tolerance * 5 * (equally_thermophilic ? 3 : 2)), 1);
        }
        else
        {
            optimal_temperature = new Attribute(new NormalDistribution(mean_optimal_temperature, 10), 1);
            temperature_tolerance = new Attribute(new NormalDistribution(0, relative_temperature_tolerance * 5), 1);
        }

        optimal_pH = new Attribute(new NormalDistribution(mean_optimal_pH, 0.5f), 1);
        pH_tolerance = new Attribute(new NormalDistribution(0, relative_pH_tolerance * 0.5f), 1);

        foreach (Molecule molecule in inhibitors_.Keys)
            inhibitors[molecule] = new Attribute(new SkewedNormalDistribution(inhibitors_[molecule], inhibitors_[molecule]/ 2, 2), 2);

        foreach (Molecule molecule in cofactors_.Keys)
            cofactors[molecule] = new Attribute(new SkewedNormalDistribution(inhibitors_[molecule], inhibitors_[molecule]/ 2, 2), 2);


        potential = new Attribute(new PotentialFunction(potential_, flexibility, this), 0);

        productivity = new Attribute(new ProductivityFunction(productivity_), 0);


        reactions[name] = this;
    }

    public Reaction()
    {

    }

    List<Attribute> GetCompetingAttributes()
    {
        List<Attribute> attributes = new List<Attribute>();

        attributes.AddRange(reactants.Values);
        attributes.AddRange(products.Values);

        attributes.Add(optimal_temperature);
        attributes.Add(temperature_tolerance);
        attributes.Add(optimal_pH);
        attributes.Add(pH_tolerance);

        foreach (Attribute attribute in inhibitors.Values)
            if (attribute.Percentile > 0.5f)
                attributes.Add(attribute);

        foreach (Attribute attribute in cofactors.Values)
            if (attribute.Percentile < 0.5f)
                attributes.Add(attribute);

        return attributes;
    }

    //This is not the actual maximum, it just tries to measure 
    //the rough scale of points needed for builds of this reaction
    float GetMaxWeight()
    {
        List<Attribute> attributes = GetCompetingAttributes();

        float max_weight = 0;

        foreach (Attribute attribute in attributes)
            max_weight += attribute.BaseWeight;

        return max_weight;
    }

    float GetRibozymePenalty()
    {
        float penalty = 1;

        if (is_ribozyme.IsTrue)
            penalty *= 0.85f;

        if (is_ribozyme.IsTrue != is_ribozyme.DefaultIsTrue)
            penalty *= 0.85f;

        return penalty;
    }

    float GetProductivityPenalty()
    {
        return productivity.DefaultValue / productivity.Value;
    }

    float GetTotalWeight()
    {
        List<Attribute> attributes = GetCompetingAttributes();

        float total_weight = 0;

        foreach (Attribute attribute in attributes)
            total_weight += attribute.Weight;

        return total_weight *
               GetRibozymePenalty() *
               GetProductivityPenalty();
    }

    public abstract class ActivityFunction : GenericFunction<Solution> { }

    class NormalActivityFunction : ActivityFunction
    {
        SkewedNormalDistribution distribution;
        float base_mean;

        System.Func<Solution, float> solution_trait_function;

        public NormalActivityFunction(float mean, float range, float skew, System.Func<Solution, float> solution_trait_function_)
        {
            distribution = new SkewedNormalDistribution(mean, range, skew);
            base_mean = mean;

            solution_trait_function = solution_trait_function_;
        }

        public override float Compute(Solution solution)
        {
            return distribution.Compute(solution_trait_function(solution)) / distribution.Compute(base_mean);
        }
    }
    
    class InhibitionFunction : ActivityFunction
    {
        Molecule inhibitor;
        float full_inhibition_concentration;

        public InhibitionFunction(Molecule inhibitor_, float full_inhibition_concentration_)
        {
            inhibitor = inhibitor_;
            full_inhibition_concentration = full_inhibition_concentration_;
        }

        public override float Compute(Solution solution)
        {
            return 1- Mathf.Min(solution.GetConcentration(inhibitor) / full_inhibition_concentration, 1);
        }
    }

    class CofactorActivityFunction : ActivityFunction
    {
        Molecule cofactor;
        float full_activation_concentration;

        public CofactorActivityFunction(Molecule cofactor_, float full_activation_concentration_)
        {
            cofactor = cofactor_;
            full_activation_concentration = full_activation_concentration_;
        }

        public override float Compute(Solution solution)
        {
            return Mathf.Min(solution.GetConcentration(cofactor) / full_activation_concentration, 1);
        }
    }

    class ConstantActivityFunction : ActivityFunction
    {
        float constant;

        public ConstantActivityFunction(float constant_)
        {
            constant = constant_;
        }

        public override float Compute(Solution solution)
        {
            return constant;
        }
    }

    class CompoundActivityFunction : ActivityFunction
    {
        List<ActivityFunction> activity_functions;

        public CompoundActivityFunction(List<ActivityFunction> activity_functions_)
        {
            activity_functions = activity_functions_;
        }

        public override float Compute(Solution solution)
        {
            float activity = 1;

            foreach (ActivityFunction activity_function in activity_functions)
                activity *= activity_function.Compute(solution);

            return activity;
        }
    }

    class CatalystImplementation : Catalyst
    {
        Dictionary<Compound, int> slot_reactants= new Dictionary<Compound, int>(), 
                                  slot_products= new Dictionary<Compound, int>();
        List<Compound> cytozol_reactants= new List<Compound>(), 
                       cytozol_products= new List<Compound>();
        ActivityFunction activity_function;

        float ATP_balance;

        public CatalystImplementation(Reaction reaction)
        {
            List<int> available_slots = new List<int> { 1, 2, 3, 4, 5 };
            available_slots.Sort(delegate (int a, int b) { return reaction.slot_order[a].Value.CompareTo(reaction.slot_order[b].Value); });
            available_slots.Insert(0, 0);

            float enthalpy = 0;
            foreach (Compound compound in reaction.reactants.Keys)
            {
                if (reaction.reactants[compound].IsTrue)
                    slot_reactants[compound] = Utility.RemoveElementAt(available_slots, 0);
                else
                    cytozol_reactants.Add(compound);
                
                enthalpy += compound.Molecule.Enthalpy;
            }

            foreach (Compound compound in reaction.products.Keys)
            {
                if (reaction.products[compound].IsTrue)
                    slot_products[compound] = Utility.RemoveElementAt(available_slots, 0);
                else
                    cytozol_products.Add(compound);

                enthalpy -= compound.Molecule.Enthalpy;
            }

            float kJ_per_ATP = (Molecule.ADP.Enthalpy + Molecule.Phosphate.Enthalpy) -
                                (Molecule.ATP.Enthalpy + Molecule.Water.Enthalpy);

            float efficiency = 0.7f;

            float kJ_lost = Mathf.Abs(enthalpy * (1 - efficiency)) - kJ_per_ATP * reaction.cost;
            enthalpy += kJ_lost;

            ATP_balance = enthalpy / kJ_per_ATP;


            List<ActivityFunction> activity_functions = Utility.CreateList<ActivityFunction>(new ConstantActivityFunction(reaction.productivity.Value));

            activity_functions.Add(new NormalActivityFunction(reaction.optimal_temperature.Value,
                                                                    8 * Mathf.Abs(reaction.temperature_tolerance.Value) * (reaction.is_ribozyme.IsTrue ? 0.7f : 1),
                                                                    0.15f,
                                                                    delegate (Solution solution) { return solution.Temperature; }));
            activity_functions.Add(new NormalActivityFunction(reaction.optimal_pH.Value,
                                                                    Mathf.Abs(reaction.pH_tolerance.Value) * (reaction.is_ribozyme.IsTrue ? 0.7f : 1),
                                                                    1,
                                                                    delegate (Solution solution) { return solution.pH; }));

            foreach (Molecule molecule in reaction.inhibitors.Keys)
                activity_functions.Add(new InhibitionFunction(molecule, reaction.inhibitors[molecule].Value));
            foreach (Molecule molecule in reaction.cofactors.Keys)
                activity_functions.Add(new CofactorActivityFunction(molecule, reaction.cofactors[molecule].Value));

            activity_function = new CompoundActivityFunction(activity_functions);
        }

        public Action Catalyze(Cell.Slot slot)
        {
            float activity = activity_function.Compute(slot.Cell.Organism.Cytozol);

            Dictionary<Cell.Slot, Compound> slot_reactants = new Dictionary<Cell.Slot, Compound>();
            foreach (Compound compound in this.slot_reactants.Keys)
                slot_reactants[slot.Cell.Slots[slot.Index + this.slot_reactants[compound]]] = new Compound(compound.Molecule, compound.Quantity* activity);

            Dictionary<Cell.Slot, Compound> slot_products = new Dictionary<Cell.Slot, Compound>();
            foreach (Compound compound in this.slot_products.Keys)
                slot_products[slot.Cell.Slots[slot.Index + this.slot_products[compound]]] = new Compound(compound.Molecule, compound.Quantity * activity);

            List<Compound> cytozol_reactants = new List<Compound>();
            foreach (Compound compound in this.cytozol_reactants)
                cytozol_reactants.Add(new Compound(compound.Molecule, compound.Quantity * activity));

            List<Compound> cytozol_products = new List<Compound>();
            foreach (Compound compound in this.cytozol_products)
                cytozol_products.Add(new Compound(compound.Molecule, compound.Quantity * activity));

            return new CompositeAction(slot, 
                                       new ReactionAction(slot, 
                                                          slot_reactants, 
                                                          slot_products,
                                                          cytozol_reactants, 
                                                          cytozol_products), 
                                       ATP_balance > 0 ?
                                       (Action) new ATPProductionAction(slot, ATP_balance) :
                                       (Action) new ATPConsumptionAction(slot, -ATP_balance));
        }
    }

    Reaction Mutate()
    {
        Reaction mutant = new Reaction();

        mutant.reactants = reactants;
        mutant.products = products;

        mutant.is_ribozyme = is_ribozyme;
        mutant.optimal_temperature = optimal_temperature;
        mutant.temperature_tolerance = temperature_tolerance;
        mutant.optimal_pH = optimal_pH;
        mutant.pH_tolerance = pH_tolerance;
        mutant.inhibitors = inhibitors;
        mutant.cofactors = cofactors;

        mutant.productivity = productivity;

        mutant.potential = potential;


        float rate = 1;

        List<object> genes = Utility.CreateList<object>(mutant.is_ribozyme,
                                                        mutant.optimal_temperature,
                                                        mutant.temperature_tolerance,
                                                        mutant.optimal_pH,
                                                        mutant.pH_tolerance,
                                                        mutant.productivity,
                                                        mutant.potential);
        genes.AddRange(new List<Attribute>(mutant.reactants.Values).Cast<object>());
        genes.AddRange(new List<Attribute>(mutant.products.Values).Cast<object>());
        genes.AddRange(new List<Attribute>(mutant.inhibitors.Values).Cast<object>());
        genes.AddRange(new List<Attribute>(mutant.cofactors.Values).Cast<object>());
        genes.Add(new List<Attribute>(mutant.slot_order.Values));

        //Select primary mutant and apply mutation
        object element= MathUtility.RemoveRandomElement(genes);
        if (element is Attribute)
            (element as Attribute).Mutate(rate);
        else if (element is List<Attribute>)
            foreach (Attribute attribute in (element as List<Attribute>))
                attribute.Mutate(rate);
        
        //Apply secondary mutations
        foreach (Attribute attribute in genes)
            attribute.Mutate(rate / 4);
        foreach (List<Attribute> attributes in genes)
            foreach(Attribute attribute in attributes)
                attribute.Mutate(rate/ 4);

        float mutant_total_weight = mutant.GetTotalWeight();
        float mutant_potential = mutant.potential.Value * mutant.GetRibozymePenalty() * mutant.GetProductivityPenalty();
        if (mutant_total_weight > mutant_potential)
            foreach (Attribute attribute in mutant.GetCompetingAttributes())
                attribute.Weight /= mutant_total_weight / mutant_potential;

        return mutant;
    }

    public interface MutantCatalyst : Catalyst
    {
        MutantCatalyst Mutate();
    }

    class MutantRibozyme : Ribozyme, MutantCatalyst
    {
        Reaction reaction;
        Catalyst catalyst;

        public MutantRibozyme(string name, Reaction reaction_) 
            : base(name, (int)(8 * 
                               reaction_.potential.Value / 
                               reaction_.GetMaxWeight() / 
                               PotentialFunction.BasePotential))
        {
            reaction = reaction_;
            catalyst = new CatalystImplementation(reaction);
        }

        public override Action Catalyze(Cell.Slot slot)
        {
            return catalyst.Catalyze(slot);
        }

        public MutantCatalyst Mutate()
        {
            return reaction.Mutate().Catalyst;
        }
    }

    class MutantEnzyme : Enzyme, MutantCatalyst
    {
        Reaction reaction;
        Catalyst catalyst;

        public MutantEnzyme(string name, Reaction reaction_)
            : base(name, (int)(16 *
                               reaction_.potential.Value /
                               reaction_.GetMaxWeight() /
                               PotentialFunction.BasePotential))
        {
            reaction = reaction_;
            catalyst = new CatalystImplementation(reaction);
        }

        public override Action Catalyze(Cell.Slot slot)
        {
            return catalyst.Catalyze(slot);
        }

        public MutantCatalyst Mutate()
        {
            return reaction.Mutate().Catalyst;
        }
    }

    MutantCatalyst catalyst = null;
    public MutantCatalyst Catalyst
    {
        get
        {
            if(catalyst== null)
            {
                if (is_ribozyme.IsTrue)
                    catalyst = new MutantRibozyme(catalyst_name, this);
                else
                    catalyst = new MutantEnzyme(catalyst_name, this);
            }

            return catalyst;
        }
    }
}
