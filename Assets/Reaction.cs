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

    public static void LoadReactions(string filename)
    {
        JObject reactions_file = JObject.Parse(Resources.Load<TextAsset>(filename).text);

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
        LoadReactions("reactions");
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

        public Attribute Copy()
        {
            Attribute attribute = new Attribute(value_distribution, base_weight);
            attribute.percentile = percentile;

            return attribute;
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


    string catalyst_name, description = "";

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
            temperature_tolerance = new Attribute(new NormalDistribution(0, relative_temperature_tolerance * 15), 1);
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

    class CatalystImplementation : InstantCatalyst
    {
        Reaction reaction;

        Dictionary<Compound, int> slot_reactants= new Dictionary<Compound, int>(), 
                                  slot_products= new Dictionary<Compound, int>();
        List<Compound> cytosol_reactants= new List<Compound>(), 
                       cytosol_products= new List<Compound>();
        ActivityFunction activity_function;

        float ATP_balance;

        public override Example Example
        {
            get
            {
                Organism organism = new Organism();
                Cell cell = organism.GetCells()[0];

                if (ATP_balance > 0)
                {
                    organism.Cytosol.AddCompound(Molecule.ADP, ATP_balance * 10);
                    organism.Cytosol.AddCompound(Molecule.Phosphate, ATP_balance * 10);
                }
                else
                    organism.Cytosol.AddCompound(Molecule.ATP, -ATP_balance * 10);

                foreach (Compound compound in cytosol_reactants)
                    organism.Cytosol.AddCompound(compound.Molecule, compound.Quantity * 10);

                //Need to determine type of catalyst
                //Assuming ribozyme for now
                cell.Slots[0].AddCompound(new Ribozyme(this), 1);

                foreach(Compound compound in slot_reactants.Keys)
                    cell.Slots[slot_reactants[compound]].AddCompound(compound);

                return new Example(organism, 1);
            }
        }

        public override int Power
        {
            get
            {
                return (int)(8 *
                             reaction.potential.Value /
                             reaction.GetMaxWeight() /
                             PotentialFunction.BasePotential);
            }
        }

        public CatalystImplementation(string name, Reaction reaction_) : base(name, 2, reaction_.description)
        {
            reaction = reaction_;

            //Until we actually have a way to mutate reactions in game, 
            //need to have least disruptive slot order
            //This ordering may also be useful later as a possible mutation
            bool simple_slot_order = true;
            List<int> available_slots = new List<int> { 1, 2, 3, 4, 5 };
            if(!simple_slot_order)
                available_slots.Sort((a, b) => (reaction.slot_order[a].Value.CompareTo(reaction.slot_order[b].Value)));

            float enthalpy = 0;
            foreach (Compound compound in reaction.reactants.Keys)
            {
                if (reaction.reactants[compound].IsTrue)
                    slot_reactants[compound] = Utility.RemoveElementAt(available_slots, 0);
                else
                    cytosol_reactants.Add(compound);
                
                enthalpy += compound.Molecule.Enthalpy;
            }

            foreach (Compound compound in reaction.products.Keys)
            {
                if (reaction.products[compound].IsTrue)
                    slot_products[compound] = Utility.RemoveElementAt(available_slots, 0);
                else
                    cytosol_products.Add(compound);

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
                                                                4 * (0.5f + Mathf.Abs(reaction.temperature_tolerance.Value)) * (reaction.is_ribozyme.IsTrue ? 0.7f : 1),
                                                                0.15f,
                                                                (solution) => (solution.Temperature)));
            activity_functions.Add(new NormalActivityFunction(reaction.optimal_pH.Value,
                                                                1.0f * (0.25f + Mathf.Abs(reaction.pH_tolerance.Value)) * (reaction.is_ribozyme.IsTrue ? 0.7f : 1),
                                                                1,
                                                                (solution) => (solution.pH)));

            foreach (Molecule molecule in reaction.inhibitors.Keys)
                activity_functions.Add(new InhibitionFunction(molecule, reaction.inhibitors[molecule].Value));
            foreach (Molecule molecule in reaction.cofactors.Keys)
                activity_functions.Add(new CofactorActivityFunction(molecule, reaction.cofactors[molecule].Value));

            activity_function = new CompoundActivityFunction(activity_functions);
        }

        protected override Action GetAction(Cell.Slot slot)
        {
            float activity = activity_function.Compute(slot.Cell.Organism.Cytosol);

            Dictionary<Cell.Slot, Compound> slot_reactants = new Dictionary<Cell.Slot, Compound>();
            foreach (Compound compound in this.slot_reactants.Keys)
                slot_reactants[slot.Cell.Slots[slot.Index + this.slot_reactants[compound]]] = new Compound(compound.Molecule, compound.Quantity * activity);

            Dictionary<Cell.Slot, Compound> slot_products = new Dictionary<Cell.Slot, Compound>();
            foreach (Compound compound in this.slot_products.Keys)
                slot_products[slot.Cell.Slots[slot.Index + this.slot_products[compound]]] = new Compound(compound.Molecule, compound.Quantity * activity);

            List<Compound> cytosol_reactants = new List<Compound>();
            foreach (Compound compound in this.cytosol_reactants)
                cytosol_reactants.Add(new Compound(compound.Molecule, compound.Quantity * activity));

            List<Compound> cytosol_products = new List<Compound>();
            foreach (Compound compound in this.cytosol_products)
                cytosol_products.Add(new Compound(compound.Molecule, compound.Quantity * activity));

            return new ReactionAction(slot, 
                                      slot_reactants,
                                      slot_products,
                                      cytosol_reactants,
                                      cytosol_products, 
                                      ATP_balance * activity);
        }

        public override Catalyst Mutate()
        {
            return reaction.Mutate().Catalyst;
        }

        public override Catalyst Copy()
        {
            return new CatalystImplementation(Name, reaction).CopyStateFrom(this);
        }
    }

    Reaction Mutate()
    {
        Reaction mutant = new Reaction();
        mutant.catalyst_name = catalyst_name;

        mutant.reactants = reactants;
        mutant.products = products;

        mutant.is_ribozyme = is_ribozyme.Copy();
        mutant.optimal_temperature = optimal_temperature.Copy();
        mutant.temperature_tolerance = temperature_tolerance.Copy();
        mutant.optimal_pH = optimal_pH.Copy();
        mutant.pH_tolerance = pH_tolerance.Copy();

        mutant.inhibitors = inhibitors;
        mutant.cofactors = cofactors;

        mutant.productivity = productivity.Copy();

        mutant.potential = potential.Copy();


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
        foreach (Attribute attribute in genes.OfType<Attribute>())
            attribute.Mutate(rate / 4);
        foreach (List<Attribute> attributes in genes.OfType<List<Attribute>>())
            foreach(Attribute attribute in attributes)
                attribute.Mutate(rate/ 4);

        float mutant_total_weight = mutant.GetTotalWeight();
        float mutant_potential = mutant.potential.Value * mutant.GetRibozymePenalty() * mutant.GetProductivityPenalty();
        if (mutant_total_weight > mutant_potential)
            foreach (Attribute attribute in mutant.GetCompetingAttributes())
                attribute.Weight /= mutant_total_weight / mutant_potential;

        return mutant;
    }

    class ReactionRibozyme : Ribozyme
    {
        Reaction reaction;

        public ReactionRibozyme(string name, Reaction reaction_) 
            : base(new CatalystImplementation(name, reaction_))
        {
            reaction = reaction_;
        }

        public override Catalyst Mutate()
        {
            return Catalyst.Mutate();
        }
    }

    class ReactionEnzyme : Enzyme
    {
        Reaction reaction;

        public ReactionEnzyme(string name, Reaction reaction_)
            : base(new CatalystImplementation(name, reaction_))
        {
            reaction = reaction_;
        }

        public override Catalyst Mutate()
        {
            return Catalyst.Mutate();
        }
    }

    Catalyst catalyst = null;
    public Catalyst Catalyst
    {
        get
        {
            if(catalyst== null)
            {
                if (is_ribozyme.IsTrue)
                    catalyst = new ReactionRibozyme(catalyst_name, this);
                else
                    catalyst = new ReactionEnzyme(catalyst_name, this);
            }

            return catalyst;
        }
    }
}
