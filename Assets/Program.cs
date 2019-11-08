using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

//Want to eventually move much of Interpretase's program queries into here
//(f.e. operand count, whether a value codon/token is a location or not, etc.)
//And ultimately, use this to remove a lot of reduntant switch statements
//over codon strings in favor of centralized switches over enums.
public class Program : Encodable
{
    public Sector MainSector { get; private set; }

    public Program(string dna_sequence = "")
    {
        MainSector = new Sector("Main Sector", 
                                "Describe this DNA strand here.", 
                                DNASequenceToTokens(dna_sequence).ConvertAll((token) => ((Code)token)));//****Replace these with Utility function
    }

    Code GetCodeById(int id)
    {
        Queue<Code> codes = new Queue<Code>();
        codes.Enqueue(MainSector);

        while (codes.Count > 0)
        {
            Code code = codes.Dequeue();
            if (code.Id == id)
                return code;

            if (code is Sector)
                foreach (Code child_code in (code as Sector).Codes)
                    codes.Enqueue(child_code);
        }

        return null;
    }

    public Sector GetSectorById(int id)
    {
        Code code = GetCodeById(id);
        if (code is Sector)
            return code as Sector;

        return null;
    }

    public Token GetTokenById(int id)
    {
        Code code = GetCodeById(id);
        if (code is Token)
            return code as Token;

        return null;
    }

    public string GenerateDNASequence()
    {
        return MainSector.GenerateDNASequence();
    }


    public static Code DecodeCode(JObject json_code_object)
    {
        Code code = null;

        switch(json_code_object["Code Type"].ToString())
        {
            case "Sector": code = new Sector(); break;
            case "Command Token": code = new CommandToken(); break;
            case "Locus Token": code = new LocusToken(); break;
            case "Value Token": code = new ValueToken(); break;
            case "Function Token": code = new FunctionToken(); break;
        }

        code.DecodeJson(json_code_object);

        return code;
    }

    public JObject EncodeJson()
    {
        JObject json_program_object = new JObject();

        json_program_object["Main Sector"] = MainSector.EncodeJson();

        return json_program_object;
    }

    public void DecodeJson(JObject json_object)
    {
        MainSector = DecodeCode(json_object["Main Sector"] as JObject) as Sector;
    }


    List<Code> registered_codes = new List<Code>();
    int next_id = -1;

    public int RegisterCode(Code code)
    {
        if (next_id < 0)
        {
            int max_id = 0;

            foreach (Code other_code in registered_codes)
                max_id = Mathf.Max(max_id, other_code.Id);

            next_id = max_id + 1;
        }

        registered_codes.Add(code);
        return next_id++;
    }


    public static string TokensToDNASequence(IEnumerable<Token> tokens)
    {
        string dna_sequence = "";

        foreach (Token token in tokens)
            dna_sequence += token.Codon;

        return dna_sequence;
    }

    public static string CodesToDNASequence(IEnumerable<Code> codes)
    {
        string dna_sequence = "";

        foreach (Code code in codes)
            dna_sequence += TokensToDNASequence(code.Tokens);

        return dna_sequence;
    }

    public static List<Token> DNASequenceToTokens(string dna_sequence)
    {
        List<Token> tokens = new List<Token>();

        DNA mock_dna_strand = new DNA(dna_sequence);

        for(int i = 0; i < mock_dna_strand.CodonCount; i++)
        {
            string codon = mock_dna_strand.GetCodon(i);

            switch (codon[0])
            {
                case 'V':
                    tokens.Add(new ValueToken(Interpretase.CodonToValue(codon)));
                    break;
                
                case 'C':
                    System.Func<string, CommandType> CodonToCommandType =
                        Utility.CreateInverseLookup(CommandToken.TypeToCodon, 
                                                    (CommandType[])System.Enum.GetValues(typeof(CommandType)));

                    tokens.Add(new CommandToken(CodonToCommandType(codon)));
                    break;

                case 'F':
                    System.Func<string, FunctionType> CodonToFunctionType =
                        Utility.CreateInverseLookup(FunctionToken.TypeToCodon,
                                                    (FunctionType[])System.Enum.GetValues(typeof(FunctionType)));

                    tokens.Add(new FunctionToken(CodonToFunctionType(codon)));
                    break;

                case 'L':
                    tokens.Add(new LocusToken(Interpretase.CodonToValue(codon)));
                    break;
            }
        }

        return tokens;
    }


    public interface Code : Encodable
    {
        int Id { get; }
        IEnumerable<Token> Tokens { get; }
    }

    public class Sector : Code
    {
        List<Code> codes = new List<Code>();

        public int Id { get; private set; }

        public IEnumerable<Token> Tokens
        {
            get
            {
                List<Token> tokens = new List<Token>();

                foreach (Code code in codes)
                    tokens.AddRange(code.Tokens);

                return tokens;
            }
        }

        public string Name { get; set; }
        public string Description { get; set; }

        public List<Code> Codes { get { return new List<Code>(codes); } }

        public Sector(string name, string description, IEnumerable<Code> codes_ = null)
        {
            Name = name;
            Description = description;

            all_sectors.Add(this);
            for (int i = 0; true; i++)
            {
                bool id_already_in_use = false;

                foreach (Sector sector in all_sectors)
                    if (sector.Id == i)
                        id_already_in_use = true;

                if (!id_already_in_use)
                {
                    Id = i;
                    break;
                }
            }

            if (codes_ != null)
                codes.AddRange(codes_);
        }

        public Sector()
        {

        }

        public Code GetPreviousCode(Code code)
        {
            int previous_index = codes.IndexOf(code) - 1;

            if (previous_index < 0)
                return null;

            return codes[previous_index];
        }

        public Code GetNextCode(Code code)
        {
            int next_index = codes.IndexOf(code) + 1;

            if (codes.Count <= next_index)
                return null;

            return codes[next_index];
        }

        public void InsertBefore(Code reference_code, IEnumerable<Code> codes_to_insert)
        {
            if (reference_code == null)
                codes.AddRange(codes_to_insert);
            else
                codes.InsertRange(codes.IndexOf(reference_code), codes_to_insert);
        }

        public void InsertBefore(Code reference_code, Code code_to_insert)
        {
            if (reference_code == null)
                codes.Add(code_to_insert);
            else
                codes.Insert(codes.IndexOf(reference_code), code_to_insert);
        }

        public void Remove(Code first, Code last)
        {
            int first_index = codes.IndexOf(first);
            int last_index = codes.IndexOf(last);

            codes.RemoveRange(first_index, last_index - first_index + 1);
        }

        public void Remove(IEnumerable<Code> codes_to_remove)
        {
            foreach (Code code in codes_to_remove)
                codes.Remove(code);
        }

        public void Remove(Code code_to_remove)
        {
            codes.Remove(code_to_remove);
        }

        public string GenerateDNASequence()
        {
            return CodesToDNASequence(codes);
        }


        static List<Sector> all_sectors = new List<Sector>();
        static int next_id = -1;

        void Register()
        {
            if (next_id < 0)
            {
                int max_id = 0;

                foreach (Sector sector in all_sectors)
                    max_id = Mathf.Max(max_id, sector.Id);

                next_id = max_id + 1;
            }

            all_sectors.Add(this);
            Id = next_id++;
        }

        public JObject EncodeJson()
        {
            JObject json_sector_object = new JObject();
            json_sector_object["Code Type"] = "Sector";

            json_sector_object["Name"] = Name;
            json_sector_object["Description"] = Description;

            JArray json_code_array = new JArray();
            foreach (Code code in codes)
                json_code_array.Add(code.EncodeJson());
            json_sector_object["Codes"] = json_code_array;

            return json_sector_object;
        }

        public void DecodeJson(JObject json_object)
        {
            Name = json_object["Name"].ToString();
            Description = json_object["Description"].ToString();

            foreach (JToken json_code_token in json_object["Codes"])
                codes.Add(DecodeCode(json_code_token as JObject));
        }
    }

    public abstract class Token : Code
    {
        public int Id { get; private set; }

        public IEnumerable<Token> Tokens { get { return Utility.CreateList(this); } }

        public abstract string Codon { get; }

        public Token()
        {

        }

        public abstract JObject EncodeJson();

        public abstract void DecodeJson(JObject json_object);
    }

    public class CommandToken : Token
    {
        public CommandType Type { get; set; }

        public override string Codon
        {
            get { return TypeToCodon(Type); }
        }

        public CommandToken(CommandType type = (CommandType)(-1))
        {
            Type = type;
        }

        public static string TypeToCodon(CommandType type)
        {
            switch (type)
            {
                case CommandType.Move: return "CVV";
                case CommandType.Spin: return "CCC";
                case CommandType.Grab: return "CVF";
                case CommandType.Take: return "CVC";
                case CommandType.Release: return "CVL";
                case CommandType.Copy: return "CFF";
                case CommandType.If: return "CLF";
                case CommandType.Try: return "CLL";
                case CommandType.Pass: return "CLV";
                case CommandType.Wait: return "CLC";
                default: Debug.Assert(false); return "";
            }
        }

        public override JObject EncodeJson()
        {
            JObject json_command_token_object = new JObject();
            json_command_token_object["Code Type"] = "Command Token";
            json_command_token_object["Type"] = Type.ToString();

            return json_command_token_object;
        }

        public override void DecodeJson(JObject json_object)
        {
            Type = Utility.CreateInverseLookup((type) => (type.ToString()), Utility.GetEnumValues<CommandType>())(json_object["Type"].ToString());
        }
    }
    //May eventually move this enum out and integrate it into 
    //other implementations; f.e. Interpretase, SectorNode
    public enum CommandType { Move, Spin, Grab, Take, Release, Copy, If, Try, Pass, Wait }
                              //Unassigned0, Unassigned1, Unassigned2, Unassigned3, Unassigned4, Unassigned5 }

    public class LocusToken : Token
    {
        int location;
        public int Location
        {
            get { return location; }
            set { location = MathUtility.Mod(value, 16); }
        }


        public override string Codon
        {
            get { return Interpretase.ValueToCodon(Location + 48); }
        }

        public LocusToken(int location = -1)
        {
            Location = location;
        }

        public override JObject EncodeJson()
        {
            JObject json_locus_token_object = new JObject();
            json_locus_token_object["Code Type"] = "Locus Token";
            json_locus_token_object["Location"] = location;

            return json_locus_token_object;
        }

        public override void DecodeJson(JObject json_object)
        {
            location = Utility.JTokenToInt(json_object["Location"]);
        }
    }

    public class ValueToken : Token
    {
        int value;
        public int Value
        {
            get { return value; }
            set { this.value = MathUtility.Mod(value, 16); }
        }

        public override string Codon
        {
            get { return Interpretase.ValueToCodon(Value); }
        }

        public ValueToken(int value = -1)
        {
            Value = value;
        }

        public override JObject EncodeJson()
        {
            JObject json_value_token_object = new JObject();
            json_value_token_object["Code Type"] = "Value Token";
            json_value_token_object["Value"] = value;

            return json_value_token_object;
        }

        public override void DecodeJson(JObject json_object)
        {
            value = Utility.JTokenToInt(json_object["Value"]);
        }
    }

    public class FunctionToken : Token
    {
        public FunctionType Type { get; set; }

        public override string Codon
        {
            get { return TypeToCodon(Type); }
        }

        public FunctionToken(FunctionType type = (FunctionType)(-1))
        {
            Type = type;
        }

        public static string TypeToCodon(FunctionType type)
        {
            switch (type)
            {
                case FunctionType.Measure: return "FVV";
                case FunctionType.GreaterThan: return "FVC";
                case FunctionType.LessThan: return "FVL";
                case FunctionType.EqualTo: return "FVF";
                case FunctionType.Add: return "FCV";
                case FunctionType.Subtract: return "FCC";
                default: return Interpretase.ValueToCodon((int)type + 32);
            }
        }

        public override JObject EncodeJson()
        {
            JObject json_function_token_object = new JObject();
            json_function_token_object["Code Type"] = "Function Token";
            json_function_token_object["Type"] = Type.ToString();

            return json_function_token_object;
        }

        public override void DecodeJson(JObject json_object)
        {
            Type = Utility.CreateInverseLookup((type) => (type.ToString()), Utility.GetEnumValues<FunctionType>())(json_object["Type"].ToString());
        }
    }
    public enum FunctionType { Measure, GreaterThan, LessThan, EqualTo, Add, Subtract,
                               Unassigned0, Unassigned1, Unassigned2, Unassigned3, Unassigned4,
                               Unassigned5, Unassigned6, Unassigned7, Unassigned8, Unassigned9 }
}
