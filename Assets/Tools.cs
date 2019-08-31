using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;

public static class Tools
{
    public static class ReactionBrainstormer
    {
        struct Component
        {
            public Molecule Molecule { get; private set; }
            public int Quantity { get; private set; }
            public bool IsInput { get; private set; }
            public bool IsAdjustable { get; private set; }

            public Component(Molecule molecule, int quantity, bool is_input, bool is_adjustable)
            {
                Molecule = molecule;
                Quantity = quantity;
                IsInput = is_input;
                IsAdjustable = is_adjustable;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Component))
                    return false;

                Component other = (Component)obj;

                return other.Molecule == this.Molecule &&
                       other.Quantity == this.Quantity &&
                       other.IsInput == this.IsInput &&
                       other.IsAdjustable == this.IsAdjustable;
            }

            //https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            public override int GetHashCode()
            {
                int hash = 17;

                hash = hash * 23 + Molecule.GetHashCode();
                hash = hash * 23 + Quantity.GetHashCode();
                hash = hash * 23 + IsInput.GetHashCode();
                hash = hash * 23 + IsAdjustable.GetHashCode();

                return hash;
            }
        }

        class Reaction : List<Component>
        {
            public Reaction()
            {

            }

            public Reaction(Reaction reaction) : base(reaction as List<Component>)
            {

            }

            public Reaction(List<Component> reaction) : base(reaction)
            {

            }
        }

        static List<Reaction> GetReactions(Reaction incomplete_reaction, List<Molecule> byproducts)
        {
            Dictionary<Element, int> elements = new Dictionary<Element, int>();
            foreach (Component component in incomplete_reaction)
            {
                foreach (Element element in component.Molecule.Elements.Keys)
                {
                    if (!elements.ContainsKey(element))
                        elements[element] = 0;

                    elements[element] += (component.IsInput ? 1 : -1) * component.Molecule.Elements[element] * component.Quantity;
                }
            }

            foreach (Element element in elements.Keys)
                if (elements[element] < 0)
                    return new List<Reaction>();

            List<Reaction> reactions = new List<Reaction>();

            List<List<Molecule>> byproduct_permutations = MathUtility.Permute(byproducts);

            foreach (List<Molecule> permutation in byproduct_permutations)
            {
                Dictionary<Molecule, int> chosen_byproducts = new Dictionary<Molecule, int>();

                Dictionary<Element, int> remaining_elements = new Dictionary<Element, int>(elements);

                foreach (Molecule molecule in permutation)
                {
                    int maximum_quantity = int.MaxValue;

                    foreach (Element element in molecule.Elements.Keys)
                    {
                        if (!remaining_elements.ContainsKey(element))
                            remaining_elements[element] = 0;

                        maximum_quantity = Mathf.Min(maximum_quantity, remaining_elements[element] / molecule.Elements[element]);
                    }

                    if (maximum_quantity == 0)
                        continue;

                    chosen_byproducts[molecule] = maximum_quantity;
                    foreach (Element element in molecule.Elements.Keys)
                        remaining_elements[element] -= molecule.Elements[element] * maximum_quantity;
                }

                bool there_are_leftovers = false;
                foreach (Element element in remaining_elements.Keys)
                    if (remaining_elements[element] > 0)
                        there_are_leftovers = true;

                if (there_are_leftovers)
                    continue;

                Reaction reaction = new Reaction(incomplete_reaction);
                foreach (Molecule molecule in chosen_byproducts.Keys)
                    reaction.Add(new Component(molecule, chosen_byproducts[molecule], false, false));
                reactions.Add(reaction);
            }

            return reactions;
        }

        static List<string> GetReactionStrings(List<Reaction> reactions, int multiple)
        {
            Dictionary<string, float> scored_reaction_strings = new Dictionary<string, float>();

            foreach (Reaction reaction in reactions)
            {
                string reaction_string = "";

                Utility.Sorted(reaction, (component) => (component.Molecule.Name));

                foreach (Component component in reaction)
                    if (component.IsInput)
                        reaction_string += (component.Quantity / (float)multiple).ToString("n1") + " " + component.Molecule.Name + " + ";
                reaction_string = Utility.Trim(reaction_string, 2);

                reaction_string += "-> ";

                foreach (Component component in reaction)
                    if (!component.IsInput)
                        reaction_string += (component.Quantity / (float)multiple).ToString("n1") + " " + component.Molecule.Name + " + ";
                reaction_string = Utility.Trim(reaction_string, 2);

                //Add in heat calculation
                //score based on heat?

                scored_reaction_strings[reaction_string] = reaction.Count * 1000000 +
                                                    MathUtility.Sum(reaction, (component) => (component.Molecule.AtomCount * component.Quantity / multiple));
            }

            return Utility.Sorted(scored_reaction_strings.Keys, (reaction_string) => (scored_reaction_strings[reaction_string]));
        }

        static List<string> Brainstorm(Reaction incomplete_reaction, List<Molecule> byproducts)
        {
            List<int> factors = new List<int>();

            foreach (Component component in incomplete_reaction)
            {
                foreach (Element element in component.Molecule.Elements.Keys)
                {
                    List<int> new_factors = MathUtility.GetPrimeFactors(component.Molecule.Elements[element]);
                    foreach (int factor in Utility.RemoveDuplicates(new_factors))
                    {
                        int difference = Utility.CountDuplicates(new_factors, factor) - Utility.CountDuplicates(factors, factor);

                        if (difference > 0)
                            for (int i = 0; i < difference; i++)
                                factors.Add(factor);
                    }
                }
            }

            int lowest_common_multiple = 1;
            foreach (int factor in factors)
                lowest_common_multiple *= factor;


            List<List<Component>> options = new List<List<Component>>();
            foreach (Component component in incomplete_reaction)
            {
                List<Component> option = new List<Component> { new Component(component.Molecule, component.Quantity * lowest_common_multiple, component.IsInput, false) };

                if (component.IsAdjustable)
                    foreach (Element element in component.Molecule.Elements.Keys)
                    {
                        int count = component.Molecule.Elements[element];

                        List<Component> new_choices = new List<Component>();

                        for (int i = 0; i < 5; i++)
                        {
                            int new_count = count - 2 + i;
                            if (new_count < 0)
                                continue;

                            new_choices.Add(new Component(component.Molecule, lowest_common_multiple * new_count / count, component.IsInput, false));
                        }

                        option = MathUtility.Union(option, new_choices);
                    }

                options.Add(option);
            }

            List<Reaction> reactions = new List<Reaction>();
            Utility.ForEach(MathUtility.Choose(options), delegate (List<Component> fixed_incomplete_reaction) 
                { reactions.AddRange(GetReactions(new Reaction(fixed_incomplete_reaction), byproducts)); });

            return GetReactionStrings(reactions, lowest_common_multiple);
        }

        public static void Run()
        {
            Dictionary<string, List<string>> brainstormed_reaction_strings = new Dictionary<string, List<string>>();

            JObject reaction_prototypes_file = JObject.Parse(ReadToolFile("reaction_prototypes"));

            JObject reaction_prototypes = reaction_prototypes_file["Reaction Prototypes"] as JObject;
            foreach (var reaction_pair in reaction_prototypes)
            {
                bool failed = false;

                string reaction_name = reaction_pair.Key;
                JObject reaction_prototype_json = reaction_pair.Value as JObject;

                Reaction reaction_prototype = new Reaction();

                if (reaction_prototype_json["Reactants"] != null)
                {
                    foreach (var reactant in reaction_prototype_json["Reactants"] as JObject)
                        if (Molecule.DoesMoleculeExist(reactant.Key))
                            reaction_prototype.Add(new Component(Molecule.GetMolecule(reactant.Key),
                                                                 Utility.JTokenToInt(reactant.Value["Quantity"]),
                                                                 true,
                                                                 Utility.JTokenToBool(reactant.Value["Adjustable"])));
                        else
                            failed = true;
                }
                else
                    failed = true;

                if (reaction_prototype_json["Products"] != null)
                {
                    foreach (var product in reaction_prototype_json["Products"] as JObject)
                        if (Molecule.DoesMoleculeExist(product.Key))
                            reaction_prototype.Add(new Component(Molecule.GetMolecule(product.Key),
                                                                 Utility.JTokenToInt(product.Value["Quantity"]),
                                                                 false,
                                                                 Utility.JTokenToBool(product.Value["Adjustable"])));
                        else
                            failed = true;
                }
                else
                    failed = true;

                List<Molecule> byproducts = new List<Molecule>();
                if (reaction_prototype_json["Byproducts"] != null)
                    foreach (var byproduct in reaction_prototype_json["Byproducts"])
                    {
                        string byproduct_name = Utility.JTokenToString(byproduct);

                        if (Molecule.DoesMoleculeExist(byproduct_name))
                            byproducts.Add(Molecule.GetMolecule(byproduct_name));
                        else
                            failed = true;
                    }
                else
                    failed = true;

                if (failed)
                    continue;

                brainstormed_reaction_strings[reaction_name] = Brainstorm(reaction_prototype, byproducts);
            }


            string reaction_brainstorms_text = "";

            foreach (string reaction_name in brainstormed_reaction_strings.Keys)
            {
                reaction_brainstorms_text += reaction_name + "\n\n";

                foreach (string reaction_string in brainstormed_reaction_strings[reaction_name])
                    reaction_brainstorms_text += reaction_string + "\n";

                reaction_brainstorms_text += "\n\n";
            }

            FileUtility.OutputText(reaction_brainstorms_text, "reaction_brainstorms");
        }
    }

    public static void Run()
    {
        ReactionBrainstormer.Run();
    }

    static string ReadToolFile(string name)
    {
        return FileUtility.ReadTextFile("Tools/" + name + ".json");
    }
}