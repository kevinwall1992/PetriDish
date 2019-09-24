using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

public class Reaction
{
    static Dictionary<string, List<Reaction>> reactions= new Dictionary<string, List<Reaction>>();

    public static List<Reaction> Reactions
    {
        get
        {
            List<Reaction> reaction_list = new List<Reaction>();

            foreach (string name in reactions.Keys)
                reaction_list.AddRange(reactions[name]);

            return reaction_list;
        }
    }

    public static List<Reaction> GetReactions(string name)
    {
        return reactions[name];
    }

    public static void LoadReactions(string filename)
    {
        JObject reactions_file = JObject.Parse(Resources.Load<TextAsset>("Reactions/" + filename).text);

        if (reactions_file["Reactions"] == null)
            return;

        JObject reactions = reactions_file["Reactions"] as JObject;
        foreach (var reaction_pair in reactions)
        {
            bool failed = false;

            string reaction_name = reaction_pair.Key;
            JObject reaction = reaction_pair.Value as JObject;

            List<Compound> reactants = new List<Compound>();
            if (reaction["Reactants"] != null)
            {
                foreach (var reactant in reaction["Reactants"] as JObject)
                    if (Molecule.DoesMoleculeExist(reactant.Key))
                        reactants.Add(new Compound(Molecule.GetMolecule(reactant.Key), Utility.JTokenToFloat(reactant.Value)));
                    else
                        failed = true;
            }
            else
                failed = true;

            List<Compound> products = new List<Compound>();
            if (reaction["Products"] != null)
            {
                foreach (var product in reaction["Products"] as JObject)
                    if (Molecule.DoesMoleculeExist(product.Key))
                        products.Add(new Compound(Molecule.GetMolecule(product.Key), Utility.JTokenToFloat(product.Value)));
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

            Dictionary<Molecule, float> activators = new Dictionary<Molecule, float>();
            if (reaction["Activators"] != null)
                foreach (var cofactor in reaction["Activators"] as JObject)
                    if (Molecule.DoesMoleculeExist(cofactor.Key))
                        activators[Molecule.GetMolecule(cofactor.Key)] = Utility.JTokenToFloat(cofactor.Value);
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
                         inhibitors, activators);
        }
    }

    public static Catalyst CreateBlankCatalyst()
    {
        return new ReactionCatalyst();
    }

    static Reaction()
    {
        LoadReactions("default");
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

        public float Percentile
        {
            get { return percentile; }
            set { percentile = value; }
        }

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

    List<Compound> reactants = new List<Compound>();
    List<Compound> products = new List<Compound>();
    float cost;

    Dictionary<Molecule, Attribute> inhibitors = new Dictionary<Molecule, Attribute>();
    Dictionary<Molecule, Attribute> activators = new Dictionary<Molecule, Attribute>();

    Dictionary<Cell.Slot.Relation, Attribute> direction_precedence = new Dictionary<Cell.Slot.Relation, Attribute>();

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
                foreach(Reaction reaction in reactions[name])
                    if (reaction == this)
                        return name;

            Debug.Assert(false, "Reaction not found in dictionary");
            return "Unnamed";
        }
    }

    public Reaction( string name, string catalyst_name_,
                     List<Compound> reactants_,
                     List<Compound> products_,
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
                     Dictionary<Molecule, float> activators_= null)
    {
        catalyst_name = catalyst_name_;


        reactants = reactants_;
        products = products_;

        cost = cost_;

        List<Cell.Slot.Relation> directions = Utility.CreateList(Cell.Slot.Relation.Across,
                                                                 Cell.Slot.Relation.Left,
                                                                 Cell.Slot.Relation.Right);

        foreach (Cell.Slot.Relation direction in directions)
            direction_precedence[direction] = new Attribute(new UniformDistribution(), 0);

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

        foreach (Molecule molecule in activators_.Keys)
            activators[molecule] = new Attribute(new SkewedNormalDistribution(inhibitors_[molecule], inhibitors_[molecule]/ 2, 2), 2);


        potential = new Attribute(new PotentialFunction(potential_, flexibility, this), 0);

        productivity = new Attribute(new ProductivityFunction(productivity_), 0);


        if (!reactions.ContainsKey(name))
            reactions[name] = new List<Reaction>();
        reactions[name].Add(this);
    }

    public Reaction()
    {

    }

    List<Attribute> GetCompetingAttributes()
    {
        List<Attribute> attributes = new List<Attribute>();

        attributes.Add(optimal_temperature);
        attributes.Add(temperature_tolerance);
        attributes.Add(optimal_pH);
        attributes.Add(pH_tolerance);

        foreach (Attribute attribute in inhibitors.Values)
            if (attribute.Percentile > 0.5f)
                attributes.Add(attribute);

        foreach (Attribute attribute in activators.Values)
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

    class ReactionCatalyst : InstantCatalyst
    {
        Reaction reaction;

        Dictionary<Compound, Cell.Slot.Relation> slot_reactants= new Dictionary<Compound, Cell.Slot.Relation>(), 
                                                 slot_products= new Dictionary<Compound, Cell.Slot.Relation>();
        List<Compound> cytosol_reactants= new List<Compound>(), 
                       cytosol_products= new List<Compound>();
        ActivityFunction activity_function;

        float NRG_balance;

        public override Example Example
        {
            get
            {
                Organism organism = new Organism();
                Cell cell = organism.GetCells()[0];

                if (NRG_balance > 0)
                    organism.Cytosol.AddCompound(Molecule.DischargedNRG, NRG_balance * 10);
                else
                    organism.Cytosol.AddCompound(Molecule.ChargedNRG, -NRG_balance * 10);

                foreach (Compound compound in cytosol_reactants)
                    organism.Cytosol.AddCompound(compound.Molecule, compound.Quantity * 10);

                //Need to determine type of catalyst
                //Assuming ribozyme for now
                cell.Slots[0].AddCompound(new Ribozyme(this), 1);

                foreach(Compound compound in slot_reactants.Keys)
                    cell.Slots[0].GetAdjacentSlot(slot_reactants[compound]).AddCompound(compound);

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

        public ReactionCatalyst(string name, Reaction reaction_) : base(name, 1, reaction_.description)
        {
            reaction = reaction_;
            if (reaction == null)
                return;

            List<Cell.Slot.Relation> available_directions = new List<Cell.Slot.Relation>(reaction.direction_precedence.Keys);
            available_directions.Sort((a, b) => (reaction.direction_precedence[a].Value.CompareTo
                                                (reaction.direction_precedence[b].Value)));

            System.Predicate<Molecule> IsCytosolMolecule = 
                molecule => molecule.Equals(Molecule.Water) ||
                            molecule.Equals(Molecule.ChargedNRG) ||
                            molecule.Equals(Molecule.DischargedNRG);

            float enthalpy = 0;
            foreach (Compound compound in reaction.reactants)
            {
                if (IsCytosolMolecule(compound.Molecule))
                    cytosol_reactants.Add(compound);
                else
                {
                    Cell.Slot.Relation direction = Utility.RemoveElementAt(available_directions, 0);

                    slot_reactants[compound] = direction;
                    Attachments[direction] = new InputAttachment(compound.Molecule);
                }

                enthalpy += compound.Molecule.Enthalpy * compound.Quantity;
            }

            foreach (Compound compound in reaction.products)
            {
                if (IsCytosolMolecule(compound.Molecule))
                    cytosol_products.Add(compound);
                else
                {
                    Cell.Slot.Relation direction = Utility.RemoveElementAt(available_directions, 0);

                    slot_products[compound] = direction;
                    Attachments[direction] = new OutputAttachment(compound.Molecule);
                }

                enthalpy -= compound.Molecule.Enthalpy * compound.Quantity;
            }

            float efficiency = 0.7f;

            float kJ_lost = Mathf.Abs(enthalpy * (1 - efficiency)) + Molecule.ChargedNRG.kJPerMole * reaction.cost;
            enthalpy -= kJ_lost;

            NRG_balance = enthalpy / Molecule.ChargedNRG.kJPerMole;

            List<ActivityFunction> activity_functions = Utility.CreateList<ActivityFunction>(new ConstantActivityFunction(reaction.productivity.Value));

            activity_functions.Add(new NormalActivityFunction(reaction.optimal_temperature.Value,
                                                                (5.0f + Mathf.Abs(reaction.temperature_tolerance.Value)) * (reaction.is_ribozyme.IsTrue ? 0.7f : 1),
                                                                0.15f,
                                                                (solution) => (solution.Temperature)));
            activity_functions.Add(new NormalActivityFunction(reaction.optimal_pH.Value,
                                                                (0.25f + Mathf.Abs(reaction.pH_tolerance.Value)) * (reaction.is_ribozyme.IsTrue ? 0.7f : 1),
                                                                1,
                                                                (solution) => (solution.pH)));

            foreach (Molecule molecule in reaction.inhibitors.Keys)
                activity_functions.Add(new InhibitionFunction(molecule, reaction.inhibitors[molecule].Value));
            foreach (Molecule molecule in reaction.activators.Keys)
                activity_functions.Add(new CofactorActivityFunction(molecule, reaction.activators[molecule].Value));

            activity_function = new CompoundActivityFunction(activity_functions);
        }

        public ReactionCatalyst()
        {

        }

        float GetActivity(Cytosol cytosol)
        {
            return activity_function.Compute(cytosol);
        }

        protected override Action GetAction(Cell.Slot slot)
        {
            float activity = GetActivity(slot.Cell.Organism.Cytosol);
            if (activity == 0)
                return null;

            Dictionary<Cell.Slot, Compound> slot_reactants = new Dictionary<Cell.Slot, Compound>();
            List<Compound> locale_reactants = new List<Compound>();
            foreach (Compound compound in this.slot_reactants.Keys)
            {
                Cell.Slot reactant_slot = slot.GetAdjacentSlot(ApplyOrientation(this.slot_reactants[compound]));
                if (reactant_slot != null)
                    slot_reactants[reactant_slot] = new Compound(compound.Molecule, compound.Quantity * activity);
                else
                    locale_reactants.Add(compound * activity);
            }

            Dictionary<Cell.Slot, Compound> slot_products = new Dictionary<Cell.Slot, Compound>();
            List<Compound> locale_products = new List<Compound>();
            foreach (Compound compound in this.slot_products.Keys)
            {
                Cell.Slot product_slot = slot.GetAdjacentSlot(ApplyOrientation(this.slot_products[compound]));
                if (product_slot != null)
                    slot_products[product_slot] = new Compound(compound.Molecule, compound.Quantity * activity);
                else
                    locale_products.Add(compound * activity);
            }

            List<Compound> cytosol_reactants = new List<Compound>();
            foreach (Compound compound in this.cytosol_reactants)
                cytosol_reactants.Add(new Compound(compound.Molecule, compound.Quantity * activity));

            List<Compound> cytosol_products = new List<Compound>();
            foreach (Compound compound in this.cytosol_products)
                cytosol_products.Add(new Compound(compound.Molecule, compound.Quantity * activity));

            return new ReactionAction(slot,
                                      slot_reactants, slot_products,
                                      cytosol_reactants, cytosol_products,
                                      locale_reactants, locale_products,
                                      NRG_balance * activity);
        }

        public override Catalyst Mutate()
        {
            return reaction.Mutate().Catalyst;
        }

        public override bool IsSame(Catalyst other)
        {
            if (!base.IsSame(other))
                return false;

            return reaction == (other as ReactionCatalyst).reaction;
        }


        public override Catalyst Copy()
        {
            return new ReactionCatalyst(Name, reaction).CopyStateFrom(this);
        }

        public override JObject EncodeJson()
        {
            JObject json_reaction_object = base.EncodeJson();

            json_reaction_object["Name"] = reaction.Name;

            JObject inhibitors_json_object = new JObject();
            foreach (Molecule molecule in reaction.inhibitors.Keys)
                inhibitors_json_object[molecule.Name] = reaction.inhibitors[molecule].Percentile;
            json_reaction_object["Inhibitors"] = inhibitors_json_object;

            JObject activators_json_object = new JObject();
            foreach (Molecule molecule in reaction.activators.Keys)
                activators_json_object[molecule.Name] = reaction.activators[molecule].Percentile;
            json_reaction_object["Activators"] = activators_json_object;

            JObject directions_json_object = new JObject();
            foreach (Cell.Slot.Relation direction in reaction.direction_precedence.Keys)
                directions_json_object[direction.ToString()] = reaction.direction_precedence[direction].Percentile;
            json_reaction_object["Directions"] = directions_json_object;

            json_reaction_object["Is Ribozyme"] = reaction.is_ribozyme.Percentile;
            json_reaction_object["Optimal Temperature"] = reaction.optimal_temperature.Percentile;
            json_reaction_object["Temperature Tolerance"] = reaction.temperature_tolerance.Percentile;
            json_reaction_object["Optimal pH"] = reaction.optimal_pH.Percentile;
            json_reaction_object["pH Tolerance"] = reaction.pH_tolerance.Percentile;
            json_reaction_object["Productivity"] = reaction.productivity.Percentile;
            json_reaction_object["Potential"] = reaction.potential.Percentile;

            return json_reaction_object;
        }

        public override void DecodeJson(JObject json_object)
        {
            base.DecodeJson(json_object);

            reaction = Reaction.GetReactions(Utility.JTokenToString(json_object["Name"]))[0].Mutate();

            foreach (var molecule_pair in json_object["Inhibitors"] as JObject)
                reaction.inhibitors[Molecule.GetMolecule(molecule_pair.Key)].Percentile = Utility.JTokenToFloat(molecule_pair.Value);

            foreach (var molecule_pair in json_object["Activators"] as JObject)
                reaction.activators[Molecule.GetMolecule(molecule_pair.Key)].Percentile = Utility.JTokenToFloat(molecule_pair.Value);

            foreach (var direction_pair in json_object["Directions"] as JObject)
            {
                Cell.Slot.Relation direction;
                switch (direction_pair.Key)
                {
                    case "Right":  direction = Cell.Slot.Relation.Right; break;
                    case "Left": direction = Cell.Slot.Relation.Left; break;
                    case "Across": direction = Cell.Slot.Relation.Across; break;

                    default:
                        Debug.Assert(false);
                        direction = Cell.Slot.Relation.None;
                        break;
                }

                reaction.direction_precedence[direction].Percentile = Utility.JTokenToFloat(direction_pair.Value);
            }

            reaction.is_ribozyme.Percentile = Utility.JTokenToFloat(json_object["Is Ribozyme"]);
            reaction.optimal_temperature.Percentile = Utility.JTokenToFloat(json_object["Optimal Temperature"]);
            reaction.temperature_tolerance.Percentile = Utility.JTokenToFloat(json_object["Temperature Tolerance"]);
            reaction.optimal_pH.Percentile = Utility.JTokenToFloat(json_object["Optimal pH"]);
            reaction.pH_tolerance.Percentile = Utility.JTokenToFloat(json_object["pH Tolerance"]);
            reaction.productivity.Percentile = Utility.JTokenToFloat(json_object["Productivity"]);
            reaction.potential.Percentile = Utility.JTokenToFloat(json_object["Potential"]);


            ReactionCatalyst other = new ReactionCatalyst(reaction.catalyst_name, reaction);

            Initialize(other.Name, other.Price, other.Description);

            slot_reactants = new Dictionary<Compound, Cell.Slot.Relation>(other.slot_reactants);
            slot_products = new Dictionary<Compound, Cell.Slot.Relation>(other.slot_products);
            cytosol_reactants = new List<Compound>(other.cytosol_reactants);
            cytosol_products = new List<Compound>(other.cytosol_products);
            activity_function = other.activity_function;
            NRG_balance = other.NRG_balance;

            foreach (Cell.Slot.Relation direction in other.Attachments.Keys)
                Attachments[direction] = other.Attachments[direction];
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

        mutant.inhibitors = new Dictionary<Molecule, Attribute>(inhibitors);
        mutant.activators = new Dictionary<Molecule, Attribute>(activators);

        mutant.direction_precedence = new Dictionary<Cell.Slot.Relation, Attribute>(direction_precedence);

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
        genes.AddRange(new List<Attribute>(mutant.inhibitors.Values).Cast<object>());
        genes.AddRange(new List<Attribute>(mutant.activators.Values).Cast<object>());
        genes.Add(new List<Attribute>(mutant.direction_precedence.Values));

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

        reactions[Name].Add(mutant);
        return mutant;
    }

    Catalyst catalyst = null;
    public Catalyst Catalyst
    {
        get
        {
            if(catalyst == null)
                catalyst = new ReactionCatalyst(catalyst_name, this);

            return catalyst;
        }
    }
}
