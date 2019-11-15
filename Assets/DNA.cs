using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DNA : Polymer
{
    int active_codon_index = 0;

    public int ActiveCodonIndex
    {
        get { return active_codon_index; }
        set { active_codon_index = value; }
    }

    public string ActiveCodon
    {
        get { return GetCodon(ActiveCodonIndex); }
    }

    string baked_sequence = null;
    public string Sequence
    {
        get
        {
            if (baked_sequence == null)
            {

                baked_sequence = "";

                for (int i = 0; i < CodonCount; i++)
                    baked_sequence += GetCodon(i);

                return baked_sequence;
            }

            return baked_sequence;
        }
    }

    public int CodonCount { get { return Monomers.Count / 3; } }

    public DNA(string sequence)
    {
        AppendSequence(sequence);
    }

    public DNA()
    {

    }



    public override void InsertMonomer(Monomer monomer, int index)
    {
        if (monomer is Nucleotide)
            base.InsertMonomer(monomer, index);

        baked_sequence = null;
    }

    public override Monomer RemoveMonomer(int index)
    {
        if (index < ActiveCodonIndex)
            ActiveCodonIndex--;

        Monomer monomer = base.RemoveMonomer(index);
        baked_sequence = null;

        return monomer;
    }

    public string GetCodon(int codon_index)
    {
        string codon = "";

        for (int i = 0; i < 3; i++)
        {
            Monomer monomer = Monomers[(codon_index * 3 + i)];

            if (monomer.Equals(Nucleotide.Valanine))
                codon += "V";
            else if (monomer.Equals(Nucleotide.Comine))
                codon += "C";
            else if (monomer.Equals(Nucleotide.Funcosine))
                codon += "F";
            else if (monomer.Equals(Nucleotide.Locomine))
                codon += "L";
        }

        return codon;
    }

    public string GetSubsequence(int starting_index, int length)
    {
        string subsequence = "";

        for (int i = 0; i < length && (i + starting_index) < CodonCount; i++)
            subsequence += GetCodon(starting_index + i);

        return subsequence;
    }


    public void InsertSequence(int index, string sequence)
    {
        int monomer_index = index * 3;

        foreach (char character in sequence)
            switch (character)
            {
                case 'V': InsertMonomer(Nucleotide.Valanine, monomer_index++); break;
                case 'C': InsertMonomer(Nucleotide.Comine, monomer_index++); break;
                case 'F': InsertMonomer(Nucleotide.Funcosine, monomer_index++); break;
                case 'L': InsertMonomer(Nucleotide.Locomine, monomer_index++); break;
                default: break;
            }
    }

    public void AppendSequence(string sequence)
    {
        InsertSequence(Monomers.Count, sequence);
    }

    public DNA RemoveSequence(int starting_index, int length)
    {
        DNA removed_dna = new DNA();

        for (int i = 0; i < length; i++)
        {
            removed_dna.AppendMonomer(RemoveMonomer(starting_index * 3));
            removed_dna.AppendMonomer(RemoveMonomer(starting_index * 3));
            removed_dna.AppendMonomer(RemoveMonomer(starting_index * 3));
        }

        return removed_dna;
    }

    public override bool IsStackable(object obj)
    {
        if (!(obj is DNA))
            return false;

        if ((obj as DNA).Sequence != Sequence)
            return false;

        return true;
    }

    public override bool Equals(object obj)
    {
        if (!IsStackable(obj))
            return false;

        if ((obj as DNA).ActiveCodonIndex != ActiveCodonIndex)
            return false;

        return true;
    }

    public override Molecule Copy()
    {
        DNA copy = new DNA(Sequence);
        copy.ActiveCodonIndex = ActiveCodonIndex;

        return copy;
    }

    public override JObject EncodeJson()
    {
        JObject json_dna_object = new JObject();

        json_dna_object["Type"] = "DNA";
        json_dna_object["Sequence"] = Sequence;
        if (ActiveCodonIndex != 0)
            json_dna_object["Active Codon Index"] = ActiveCodonIndex;

        return json_dna_object;
    }

    public override void DecodeJson(JObject json_dna_object)
    {
        Monomers.Clear();
        AppendSequence(Utility.JTokenToString(json_dna_object["Sequence"]));
        if (json_dna_object.ContainsKey("Active Codon Index"))
            ActiveCodonIndex = Utility.JTokenToInt(json_dna_object["Active Codon Index"]);
    }
}


public class Nucleotide : Polymer.Monomer
{
    public static Molecule genes;

    public static Nucleotide Valanine { get { return new Nucleotide(Type.Valanine); } }
    public static Nucleotide Comine { get { return new Nucleotide(Type.Comine); } }
    public static Nucleotide Funcosine { get { return new Nucleotide(Type.Funcosine); } }
    public static Nucleotide Locomine { get { return new Nucleotide(Type.Locomine); } }

    static Nucleotide()
    {
        genes = GetMolecule("Genes");
        RegisterNamedMolecule("Genes", new Nucleotide());
    }


    enum Type { None, Valanine, Comine, Funcosine, Locomine }
    Type type;

    public override float Enthalpy { get { return genes.Enthalpy; } }
    public override Dictionary<Element, int> Elements { get { return genes.Elements; } }

    Nucleotide(Type type_ = Type.None) : base(Water)
    {
        type = type_;
    }

    public Nucleotide() : base(Water)
    {
        type = Type.None;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Nucleotide))
            return false;

        return (obj as Nucleotide).type == type;
    }

    public override JObject EncodeJson()
    {
        return JObject.FromObject(Utility.CreateDictionary<string, string>("Type", "Nucleotide",
                                                                           "Nucleotide Type", type.ToString()));
    }

    public override void DecodeJson(JObject json_object)
    {
        switch (Utility.JTokenToString(json_object["Nucleotide Type"]))
        {
            case "Valanine": type = Type.Valanine; break;
            case "Comine": type = Type.Comine; break;
            case "Funcosine": type = Type.Funcosine; break;
            case "Locomine": type = Type.Locomine; break;

            default: type = Type.None; break;
        }
    }
}
