using UnityEngine;


public class Interpretase : Ribozyme
{
    public Interpretase() : base("Interpretase", 6)
    {

    }

    public override Action Catalyze(Cell.Slot slot)
    {
        if (slot.Compound == null || !(slot.Compound.Molecule is DNA))
            return null;

        DNA dna = slot.Compound.Molecule as DNA;

        SeekToCommand(dna);
        if (dna.ActiveCodonIndex >= dna.GetCodonCount())
            return null;

        string codon = dna.ActiveCodon;
        string subcodon = codon.Substring(1);

        switch (codon[0])
        {
            case 'A':
                break;

            case 'C':
                switch (subcodon)
                {
                    case "AA":
                    case "CC":
                        object source_location = CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 1));
                        if (!(source_location is Cell.Slot))
                            return new NullCommand(slot);

                        object destination_location = CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 2));
                        if (!(destination_location is Cell.Slot))
                            return new NullCommand(slot);

                        return new MoveCommand(slot, destination_location as Cell.Slot, source_location as Cell.Slot, subcodon == "AA");

                    case "AC":
                        object activation_location = CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 1));
                        if (!(activation_location is Cell.Slot))
                            return new NullCommand(slot);

                        int activation_count = CodonToValue(dna.GetCodon(dna.ActiveCodonIndex + 2));

                        return new ActivateCommand(slot, activation_location as Cell.Slot, activation_count);

                    case "AG":
                        object goto_location = CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 1));
                        if (!(goto_location is string))
                            return new NullCommand(slot);

                        int condition_value = 1;
                        if ((dna.ActiveCodonIndex + 2) < dna.GetCodonCount())
                        {
                            string condition_codon = dna.GetCodon(dna.ActiveCodonIndex + 2);
                            if (condition_codon[0] == 'A')
                                condition_value = CodonToValue(condition_codon);
                            else if (condition_codon[0] == 'G')
                                condition_value = ComputeFunction(slot, dna.ActiveCodonIndex + 2);
                        }

                        if (condition_value != 0)
                            return new GoToCommand(slot, goto_location as string, condition_value);
                        else
                            return new NullCommand(slot);

                    case "AT":
                        object cut_location = CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 1));
                        if (!(cut_location is string))
                            return new NullCommand(slot);

                        object paste_location = CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 2));
                        if (!(paste_location is Cell.Slot))
                            return new NullCommand(slot);

                        return new CutCommand(slot, paste_location as Cell.Slot, cut_location as string);

                    case "CA":
                        break;


                }
                break;

            case 'G':
                break;

            case 'T':
                break;

            default: throw new System.InvalidOperationException();
        }

        return null;
    }

    public void SeekToCommand(DNA dna, bool seek_backwards = false)
    {
        while (dna.ActiveCodonIndex < dna.GetCodonCount() &&
                dna.ActiveCodonIndex >= 0 &&
                dna.ActiveCodon[0] != 'C')

            if (seek_backwards)
                dna.ActiveCodonIndex--;
            else
                dna.ActiveCodonIndex++;

        dna.ActiveCodonIndex = Mathf.Clamp(dna.ActiveCodonIndex, 0, dna.GetCodonCount() - 1);
    }

    public void SeekToMarker(DNA dna, string marker, bool seek_backwards = false)
    {
        int original_codon_index = dna.ActiveCodonIndex;

        while (true)
        {
            do
            {
                if (seek_backwards)
                    dna.ActiveCodonIndex--;
                else
                    dna.ActiveCodonIndex++;

                if (dna.ActiveCodonIndex < 0)
                    dna.ActiveCodonIndex = dna.GetCodonCount() - 1;
                else if (dna.ActiveCodonIndex >= dna.GetCodonCount())
                    dna.ActiveCodonIndex = 0;

                if (dna.ActiveCodonIndex == original_codon_index)
                    return;
            }
            while (dna.ActiveCodon != marker);

            int t_codon_index = dna.ActiveCodonIndex;
            SeekToCommand(dna, true);
            int operand_count = 0;
            if (dna.ActiveCodon[0] == 'C')
                switch (dna.ActiveCodon.Substring(1))
                {
                    case "AA":
                    case "CC":
                    case "AC":
                    case "AT":
                        operand_count = 2;
                        break;

                    case "AG":
                    case "CA":
                        operand_count = 1;
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            else
            {
                dna.ActiveCodonIndex = t_codon_index;
                return;
            }

            if ((dna.ActiveCodonIndex + operand_count) < t_codon_index)
            {
                dna.ActiveCodonIndex = t_codon_index;
                return;
            }
            else if (!seek_backwards)
                dna.ActiveCodonIndex += operand_count;
        }
    }

    DNA Cut(DNA dna, string marker)
    {
        DNA dna_segment = new DNA();

        int current_codon_index = dna.ActiveCodonIndex;
        SeekToMarker(dna, marker);
        int marker_codon_index = dna.ActiveCodonIndex + 1;
        dna.ActiveCodonIndex = current_codon_index;

        int monomer_index = marker_codon_index * 3;
        while (marker_codon_index < dna.GetCodonCount() && dna.GetCodon(marker_codon_index) != "TTT")
        {
            dna_segment.AddMonomer(dna.RemoveMonomer(monomer_index));
            dna_segment.AddMonomer(dna.RemoveMonomer(monomer_index));
            dna_segment.AddMonomer(dna.RemoveMonomer(monomer_index));
        }

        return dna_segment;
    }

    public static int CodonToValue(string codon)
    {
        int total = 0;
        int exponent = 2;

        foreach (char character in codon)
        {
            int value = 0;

            switch (character)
            {
                case 'A': value = 0; break;
                case 'C': value = 1; break;
                case 'G': value = 2; break;
                case 'T': value = 3; break;
            }

            int base_ = 4;
            int power = 1;
            for (int i = 0; i < exponent; i++)
                power *= base_;

            total += power * value;

            exponent--;
        }

        return total;
    }

    object CodonToLocation(Cell.Slot dna_slot, string codon)
    {
        int value = CodonToValue(codon);

        if (value < 48)
            return null;

        if (value < 54)
            return dna_slot.Cell.GetSlot(dna_slot.Index + value - 48);
        else if (value == 54)
            return dna_slot.Cell.Organism;
        else
            return codon;
    }

    public int ComputeFunction(Cell.Slot dna_slot, int function_codon_index, out int next_codon_index)
    {
        DNA dna = dna_slot.Compound.Molecule as DNA;

        string function_codon = dna.GetCodon(function_codon_index);

        switch (function_codon)
        {
            case "GAA":
                next_codon_index = function_codon_index + 2;
                Cell.Slot query_slot = CodonToLocation(dna_slot, dna.GetCodon(function_codon_index + 1)) as Cell.Slot;

                return query_slot.Compound == null ? 0 : (int)query_slot.Compound.Quantity;

            case "GAC":
            case "GAT":
            case "GAG":
                int operand0 = ComputeFunction(dna_slot, function_codon_index + 1, out next_codon_index);
                int operand1 = ComputeFunction(dna_slot, next_codon_index, out next_codon_index);

                switch (function_codon)
                {
                    case "GAC": return operand0 > operand1 ? 1 : 0;
                    case "GAT": return operand0 < operand1 ? 1 : 0;
                    case "GAG": return operand0 == operand1 ? 1 : 0;
                }
                break;

            default:
                next_codon_index = function_codon_index + 1;
                return CodonToValue(dna.GetCodon(function_codon_index));
        }

        next_codon_index = 0;
        return 0;
    }

    int ComputeFunction(Cell.Slot dna_slot, int function_codon_index)
    {
        int next_codon_index;

        return ComputeFunction(dna_slot, function_codon_index, out next_codon_index);
    }


    public class Command : Action
    {
        public Command(Cell.Slot slot) : base(slot)
        {

        }

        public Interpretase GetInterpretase()
        {
            return Slot.CatalystCompound.Molecule as Interpretase;
        }

        public override void End()
        {
            base.End();

            GetDNA().ActiveCodonIndex++;
        }

        protected DNA GetDNA()
        {
            Debug.Assert(Slot.Compound.Molecule is DNA);

            return Slot.Compound.Molecule as DNA;
        }
    }

    public class OutputCommand : Command
    {
        Cell.Slot output_slot;
        Compound outputted_compound;

        public Cell.Slot OutputSlot
        {
            get { return output_slot; }
        }

        public Compound OutputtedCompound
        {
            get { return outputted_compound; }
            protected set { outputted_compound = value; }
        }

        public OutputCommand(Cell.Slot slot, Cell.Slot output_slot_) : base(slot)
        {
            output_slot = output_slot_;
        }

        public override void End()
        {
            base.End();

            output_slot.AddCompound(outputted_compound);
        }

        protected bool IsMoleculeValidForOutput(Molecule molecule)
        {
            if (output_slot.Compound == null)
                return true;

            return molecule.CompareMolecule(output_slot.Compound.Molecule);
        }
    }

    public class CutCommand : OutputCommand
    {
        string marker;

        public CutCommand(Cell.Slot slot, Cell.Slot output_slot, string marker_) : base(slot, output_slot)
        {
            marker = marker_;
        }

        public override void Beginning()
        {
            base.Beginning();

            if (OutputSlot.Compound != null && !(OutputSlot.Compound.Molecule is DNA))
                Fail();
            else
            {
                DNA dna = GetInterpretase().Cut(GetDNA(), marker);
                if (dna == null)
                    Fail();
                else
                {
                    Polymer polymer = GetRibozyme(dna.Sequence);
                    if (polymer != null)
                        dna = polymer as DNA;

                    OutputtedCompound = new Compound(dna, 1);
                }
            }
        }
    }

    public class ActivateCommand : OutputCommand
    {
        int activation_count = 1;

        public int ActivationCount
        {
            get { return activation_count; }
        }

        public ActivateCommand(Cell.Slot slot, Cell.Slot output_slot, int activation_count_) : base(slot, output_slot)
        {
            activation_count = activation_count_;
        }

        public override void Beginning()
        {
            base.Beginning();

            if (!IsMoleculeValidForOutput(Molecule.ATP))
                Fail();
            else
                OutputtedCompound = Cell.Organism.Cytozol.RemoveCompound(Molecule.ATP, activation_count);
        }
    }

    public class MoveCommand : OutputCommand
    {
        Cell.Slot input_slot;
        bool move_entire_stack;

        public Cell.Slot InputSlot
        {
            get { return input_slot; }
        }

        public MoveCommand(Cell.Slot slot, Cell.Slot output_slot, Cell.Slot input_slot_, bool move_entire_stack_) : base(slot, output_slot)
        {
            input_slot = input_slot_;
            move_entire_stack = move_entire_stack_;
        }

        public override void Beginning()
        {
            base.Beginning();

            if (!IsMoleculeValidForOutput(input_slot.Compound.Molecule))
                Fail();
            else
                OutputtedCompound = move_entire_stack ? input_slot.RemoveCompound() : input_slot.Compound.Split(1);
        }
    }

    public class GoToCommand : Command
    {
        string marker;
        int seek_count;

        public GoToCommand(Cell.Slot slot, string marker_, int seek_count_) : base(slot)
        {
            marker = marker_;
            seek_count = seek_count_;
        }

        public override void End()
        {
            for (int i = 0; i < seek_count; i++)
                GetInterpretase().SeekToMarker(GetDNA(), marker, seek_count < 0);
        }
    }

    class NullCommand : Command
    {
        public NullCommand(Cell.Slot slot) : base(slot)
        {

        }
    }
}
