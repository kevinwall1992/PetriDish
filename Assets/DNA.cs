using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

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

    public string Sequence
    {
        get
        {
            string sequence = "";

            for (int i = 0; i < CodonCount; i++)
                sequence += GetCodon(i);

            return sequence;
        }
    }

    public int CodonCount { get { return Monomers.Count / 3; } }

    public DNA(string sequence)
    {
        AppendSequence(sequence);
        AddMainSector();
    }

    public DNA()
    {
        AddMainSector();
    }



    public override void InsertMonomer(Monomer monomer, int index)
    {
        if (monomer.Equals(Nucleotide.Valanine) ||
            monomer.Equals(Nucleotide.Comine) ||
            monomer.Equals(Nucleotide.Funcosine) ||
            monomer.Equals(Nucleotide.Locomine))
            base.InsertMonomer(monomer, index);
    }

    public override Monomer RemoveMonomer(int index)
    {
        if (index < ActiveCodonIndex)
            ActiveCodonIndex--;

        return base.RemoveMonomer(index);
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
       
        int length = sequence.Length / 3;

        foreach (Sector sector in Sectors)
        {
            if (sector == MainSector)
                continue;

            if (sector.FirstCodonIndex > index)
                sector.FirstCodonIndex += length;
            else if (sector.LastCodonIndex >= index)
                sector.LastCodonIndex += length;
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

        foreach (Sector sector in Sectors)
            if (sector.FirstCodonIndex > starting_index)
                sector.FirstCodonIndex -= length;
            else if (sector.LastCodonIndex >= starting_index)
                sector.LastCodonIndex = Mathf.Max(sector.LastCodonIndex - length, starting_index);

        return removed_dna;
    }


    public override bool Equals(object obj)
    {
        if (!base.Equals(obj))
            return false;

        return obj is DNA && (obj as DNA).ActiveCodonIndex == ActiveCodonIndex;
    }

    public override Molecule Copy()
    {
        if (this is Ribozyme)
            return this;

        DNA copy = new DNA(Sequence);
        copy.ActiveCodonIndex = ActiveCodonIndex;

        copy.sectors.Clear();
        foreach (Sector sector in Sectors)
            copy.AddSector(sector.Name, sector.Description, sector.FirstCodonIndex, sector.LastCodonIndex);

        return copy;
    }

    public override JObject EncodeJson()
    {
        JObject json_dna_object = new JObject();

        json_dna_object["Type"] = "DNA";
        json_dna_object["Sequence"] = Sequence;

        if (sectors.Count > 1 || 
            MainSector.Name != default_main_sector_name || 
            MainSector.Description != default_main_sector_description)
        {
            JArray json_sector_array = new JArray();
            foreach (Sector sector in Sectors)
            {
                JObject json_sector_object = new JObject();
                json_sector_object["Name"] = sector.Name;
                json_sector_object["Description"] = sector.Description;
                json_sector_object["First Codon Index"] = sector.FirstCodonIndex;
                json_sector_object["Last Codon Index"] = sector.LastCodonIndex;

                json_sector_array.Add(json_sector_object);
            }
            json_dna_object["Sectors"] = json_sector_array;
        }

        return json_dna_object;
    }

    public override void DecodeJson(JObject json_dna_object)
    {
        Monomers.Clear();
        AppendSequence(Utility.JTokenToString(json_dna_object["Sequence"]));

        if (json_dna_object.ContainsKey("Sectors"))
        {
            sectors.Clear();

            foreach (JToken json_token in json_dna_object["Sectors"])
            {
                JObject json_sector_object = json_token as JObject;

                AddSector(Utility.JTokenToString(json_sector_object["Name"]),
                            Utility.JTokenToString(json_sector_object["Description"]),
                            Utility.JTokenToInt(json_sector_object["First Codon Index"]),
                            Utility.JTokenToInt(json_sector_object["Last Codon Index"]));
            }
        }
    }


    List<Sector> sectors = new List<Sector>();
    public IEnumerable<Sector> Sectors
    {
        get { return sectors; }
    }

    public Sector MainSector
    {
        get
        {
            Sector main_sector = null;

            foreach (Sector sector in Sectors)
                if (sector.FirstCodonIndex == 0 && (main_sector == null || sector.LastCodonIndex > main_sector.LastCodonIndex))
                    main_sector = sector;

            main_sector.LastCodonIndex = CodonCount - 1;
            return main_sector;
        }
    }
    const string default_main_sector_name = "Main Sector";
    const string default_main_sector_description = "Describe this DNA strand here";
    void AddMainSector() { AddSector(default_main_sector_name, default_main_sector_description, 0, CodonCount - 1); }

    public Sector AddSector(string name , string description, int first_codon_index, int last_codon_index)
    {
        Sector sector = new Sector(this, name, description, first_codon_index, last_codon_index);
        sectors.Add(sector);

        return sector;
    }

    public void RemoveSector(Sector sector)
    {
        if (sector == MainSector)
            return;

        sectors.Remove(sector);
    }

    public Sector GetSector(int codon_index)
    {
        Sector deepest_sector = null;

        foreach (Sector sector in Sectors)
            if (sector.FirstCodonIndex <= codon_index &&
                sector.LastCodonIndex >= codon_index)
                if (deepest_sector == null || (deepest_sector.FirstCodonIndex <= sector.FirstCodonIndex &&
                                               deepest_sector.LastCodonIndex >= sector.LastCodonIndex))
                    deepest_sector = sector;

        return deepest_sector;
    }

    public class Sector
    {
        public DNA DNA { get; private set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int FirstCodonIndex { get; set; }
        public int LastCodonIndex { get; set; }

        public int Identity { get; private set; }

        public int Length { get { return LastCodonIndex - FirstCodonIndex + 1; } }

        public string Sequence { get { return DNA.GetSubsequence(FirstCodonIndex, Length); } }

        public Sector(DNA dna, string name, string description, int first_codon_index, int last_codon_index)
        {
            DNA = dna;

            Name = name;
            Description = description;
            FirstCodonIndex = first_codon_index;
            LastCodonIndex = last_codon_index;

            Identity = 0;

            while (true)
            {
                foreach (Sector sector in DNA.Sectors)
                    if (Identity == sector.Identity)
                    {
                        Identity++;
                        continue;
                    }

                break;
            }
        }
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
        if (!base.Equals(obj))
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
        switch(Utility.JTokenToString(json_object["Nucleotide Type"]))
        {
            case "Valanine": type = Type.Valanine; break;
            case "Comine": type = Type.Comine; break;
            case "Funcosine": type = Type.Funcosine; break;
            case "Locomine": type = Type.Locomine; break;

            default: type = Type.None; break;
        }
    }
}
