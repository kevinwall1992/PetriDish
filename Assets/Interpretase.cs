﻿using UnityEngine;


public class Interpretase : ProgressiveCatalyst
{
    public Interpretase() : base("Interpretase")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (slot.Compound == null || !(slot.Compound.Molecule is DNA))
            return null;

        return Interpret(slot, FindCommandCodon(slot.Compound.Molecule as DNA));
    }

    public static Command Interpret(Cell.Slot slot, int command_codon_index)
    {
        if (command_codon_index < 0)
            return null;

        if (slot.Compound == null || !(slot.Compound.Molecule is DNA))
            return null;

        DNA dna = slot.Compound.Molecule as DNA;

        string codon = dna.GetCodon(command_codon_index);
        if (codon[0] != 'C')
            return null;

        int step_codon_index = command_codon_index + GetCommandLength(dna, command_codon_index);

        switch (codon)
        {
            case "CAA":
            case "CCC":
            case "CGG":
            case "CTT":
                object source_location = CodonToLocation(slot, dna.GetCodon(command_codon_index + 1));
                if (!(source_location is Cell.Slot))
                    return new NullCommand(slot, step_codon_index);

                object destination_location = CodonToLocation(slot, dna.GetCodon(command_codon_index + 2));
                if (!(destination_location is Cell.Slot))
                    return new NullCommand(slot, step_codon_index);

                MoveCommand.Type move_type = MoveCommand.Type.Max;
                switch(codon)
                {
                    case "CAA": move_type = MoveCommand.Type.Single; break;
                    case "CCC": move_type = MoveCommand.Type.Half; break;
                    case "CGG": move_type = MoveCommand.Type.Full; break;
                    case "CTT": move_type = MoveCommand.Type.Max; break;
                }

                return new MoveCommand(slot, step_codon_index, 
                                        destination_location as Cell.Slot, 
                                        source_location as Cell.Slot,
                                        move_type);

            case "CCA":
                object a_location = CodonToLocation(slot, dna.GetCodon(command_codon_index + 1));
                if (!(a_location is Cell.Slot))
                    return new NullCommand(slot, step_codon_index);

                object b_location = CodonToLocation(slot, dna.GetCodon(command_codon_index + 2));
                if (!(b_location is Cell.Slot))
                    return new NullCommand(slot, step_codon_index);

                return new SwapCommand(slot, step_codon_index, a_location as Cell.Slot, b_location as Cell.Slot);

            case "CAC":
                object activation_location = CodonToLocation(slot, dna.GetCodon(command_codon_index + 1));
                if (!(activation_location is Cell.Slot))
                    return new NullCommand(slot, step_codon_index);

                int activation_count = CodonToValue(dna.GetCodon(command_codon_index + 2));

                return new ActivateCommand(slot, step_codon_index, activation_location as Cell.Slot, activation_count);

            case "CAG":
                object goto_location = CodonToLocation(slot, dna.GetCodon(command_codon_index + 1));
                if (!(goto_location is string))
                    return new NullCommand(slot, step_codon_index);

                int condition_value = 1;
                if ((command_codon_index + 2) < dna.CodonCount)
                {
                    string condition_codon = dna.GetCodon(command_codon_index + 2);
                    if (condition_codon[0] == 'A')
                        condition_value = CodonToValue(condition_codon);
                    else if (condition_codon[0] == 'G')
                        condition_value = ComputeFunction(slot, command_codon_index + 2);
                }

                if (condition_value != 0)
                    return new GoToCommand(slot, step_codon_index, goto_location as string, condition_value);
                else
                    return new NullCommand(slot, step_codon_index);

            case "CCG":
                object else_location = CodonToLocation(slot, dna.GetCodon(command_codon_index + 1));
                if (!(else_location is string))
                    return new NullCommand(slot, step_codon_index);

                return new TryCommand(slot, step_codon_index, Interpret(slot, FindCommandCodon(dna, command_codon_index + 1)), else_location as string);

            case "CAT":
                object cut_location = CodonToLocation(slot, dna.GetCodon(command_codon_index + 1));
                if (!(cut_location is string))
                    return new NullCommand(slot, step_codon_index);

                object paste_location = CodonToLocation(slot, dna.GetCodon(command_codon_index + 2));
                if (!(paste_location is Cell.Slot))
                    return new NullCommand(slot, step_codon_index);

                return new CutCommand(slot, 
                                      step_codon_index - GetSegmentLength(dna, cut_location as string, command_codon_index), 
                                      paste_location as Cell.Slot, 
                                      cut_location as string, 
                                      command_codon_index);
        }

        return null;
    }

    public static int GetCommandLength(DNA dna, int command_codon_index)
    {
        string command_codon = dna.GetCodon(command_codon_index);
        if (command_codon[0] != 'C')
            return 0;

        int length = 0;
        int operand_count = 0;

        do
        {
            string codon = dna.GetCodon(command_codon_index + length);

            if (length > 0)
                operand_count--;
            length++;

            switch (codon[0])
            {
                case 'A': break;

                case 'C':
                    operand_count += 2;
                    break;

                case 'G':
                    switch (codon)
                    {
                        case "GAA":
                            operand_count++;
                            break;

                        case "GAC":
                        case "GAG":
                        case "GAT":
                            operand_count += 2;
                            break;
                    }
                    break;

                case 'T': break;
            }
        } while (operand_count > 0);

        return length;
    }

    public static int FindCommandCodon(DNA dna, int starting_codon_index, bool search_backwards = false)
    {
        int codon_index = starting_codon_index;

        while (codon_index < dna.CodonCount &&
                codon_index >= 0 &&
                dna.GetCodon(codon_index)[0] != 'C')

            if (search_backwards)
                codon_index--;
            else
                codon_index++;

        if (codon_index >= dna.CodonCount || codon_index < 0)
            return -1;

        return codon_index;
    }

    public static int FindCommandCodon(DNA dna, bool search_backwards = false)
    {
        return FindCommandCodon(dna, dna.ActiveCodonIndex, search_backwards);
    }

    public static void SeekToCommand(DNA dna, bool search_backwards = false)
    {
        dna.ActiveCodonIndex = FindCommandCodon(dna, search_backwards);
    }

    public static int FindMarkerCodon(DNA dna, string marker, int starting_codon_index, bool search_backwards = false)
    {
        int codon_index = starting_codon_index;

        bool is_first_codon = true;

        while (true)
        {
            if(!is_first_codon || dna.GetCodon(codon_index) != marker)
                do
                {
                    if (search_backwards)
                        codon_index--;
                    else
                        codon_index++;

                    if (codon_index < 0)
                        codon_index = dna.CodonCount - 1;
                    else if (codon_index >= dna.CodonCount)
                        codon_index = 0;

                    if (codon_index == starting_codon_index)
                        return -1;
                } while (dna.GetCodon(codon_index) != marker);

            int command_codon_index = FindCommandCodon(dna, codon_index, true);
            if (command_codon_index < 0 ||
                (command_codon_index + GetCommandLength(dna, command_codon_index) - 1) < codon_index)
                return codon_index;

            is_first_codon = false;
        }
    }

    public static int FindMarkerCodon(DNA dna, string marker, bool search_backwards = false)
    {
        return FindMarkerCodon(dna, marker, dna.ActiveCodonIndex, search_backwards);
    }

    public static void SeekToMarker(DNA dna, string marker, bool search_backwards = false)
    {
        dna.ActiveCodonIndex = FindMarkerCodon(dna, marker, search_backwards);
    }

    public static int GetSegmentLength(DNA dna, string marker, int local_codon_index)
    {
        int length = 0;

        int marker_contents_codon_index = FindMarkerCodon(dna, marker, local_codon_index) + 1;

        while ((marker_contents_codon_index + length) < dna.CodonCount &&
               dna.GetCodon(marker_contents_codon_index + length) != "TTT")
            length++;

        return length;
    }

    public static DNA Cut(DNA dna, string marker, int local_codon_index)
    {
        return dna.RemoveStrand(FindMarkerCodon(dna, marker) + 1, GetSegmentLength(dna, marker, local_codon_index));
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

    public static object CodonToLocation(Cell.Slot dna_slot, string codon)
    {
        int value = CodonToValue(codon);

        if (value < 48)
            return null;

        if (value < 54)
            return dna_slot.Cell.Slots[dna_slot.Index + value - 48];
        else if (value == 54)
            return dna_slot.Cell.Organism;
        else
            return codon;
    }

    public static int ComputeFunction(Cell.Slot dna_slot, int function_codon_index, out int next_codon_index)
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

    public static int ComputeFunction(Cell.Slot dna_slot, int function_codon_index)
    {
        int next_codon_index;

        return ComputeFunction(dna_slot, function_codon_index, out next_codon_index);
    }

    public class Command : Action
    {
        int step_codon_index;

        public Command(Cell.Slot slot, int step_codon_index_, float cost) : base(slot, cost)
        {
            step_codon_index = step_codon_index_;
        }

        public override bool Prepare() { return true; }

        public override void Begin() { }

        public override void End()
        {
            if (!HasFailed)
                GetDNA().ActiveCodonIndex = step_codon_index;
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

        public OutputCommand(Cell.Slot slot, int step_codon_index, Cell.Slot output_slot_) : base(slot, step_codon_index, 1)
        {
            output_slot = output_slot_;
        }

        public override void End()
        {
            output_slot.AddCompound(outputted_compound);

            base.End();
        }

        protected bool IsMoleculeValidForOutput(Molecule molecule)
        {
            if (output_slot.Compound == null)
                return true;

            return molecule == output_slot.Compound.Molecule;
        }
    }

    public class CutCommand : OutputCommand
    {
        string marker;
        int local_codon_index;

        public override float Scale
        {
            get
            {
                base.Scale = Slot.Compound.Quantity;

                return base.Scale;
            }
        }

        public CutCommand(Cell.Slot slot, int step_codon_index, Cell.Slot output_slot, string marker_, int local_codon_index_) : base(slot, step_codon_index, output_slot)
        {
            marker = marker_;
            local_codon_index = local_codon_index_;
        }

        public override bool Prepare()
        {
            if (OutputSlot.Compound != null && !(OutputSlot.Compound.Molecule is DNA))
                Fail();
            else if(Interpretase.GetSegmentLength(GetDNA(), marker, local_codon_index)== 0)
                Fail();

            return !HasFailed;
        }

        public override void Begin()
        {
            base.Begin();

            DNA dna = Interpretase.Cut(GetDNA(), marker, local_codon_index);
                
            Polymer polymer = Ribozyme.GetRibozyme(dna.Sequence);
            if (polymer != null)
                dna = polymer as DNA;

            OutputtedCompound = new Compound(dna, 1);
        }
    }

    public class ActivateCommand : OutputCommand
    {
        int activation_count = 1;

        public int ActivationCount
        {
            get { return activation_count; }
        }

        public ActivateCommand(Cell.Slot slot, int step_codon_index, Cell.Slot output_slot, int activation_count_) : base(slot, step_codon_index, output_slot)
        {
            activation_count = activation_count_;

            Cost = activation_count;
        }

        public override bool Prepare()
        {
            if (!IsMoleculeValidForOutput(Molecule.ATP))
                Fail();

            return !HasFailed;
        }

        public override void Begin()
        {
            base.Begin();

            OutputtedCompound = Cell.Organism.Cytozol.RemoveCompound(Molecule.ATP, activation_count);
        }
    }

    public class MoveCommand : OutputCommand
    {
        Cell.Slot input_slot;
        Type type;

        Compound MovedCompound
        {
            get
            {
                if (input_slot.Compound != null)
                    return input_slot.Compound;

                if (input_slot.CatalystCompound != null)
                    return input_slot.CatalystCompound;

                return null;
            }
        }

        public Cell.Slot InputSlot
        {
            get { return input_slot; }
        }

        public MoveCommand(Cell.Slot slot, int step_codon_index, Cell.Slot output_slot, Cell.Slot input_slot_, Type type_) 
            : base(slot, step_codon_index, output_slot)
        {
            input_slot = input_slot_;

            type = type_;

            if (MovedCompound == null)
                Cost = 0;
            else
                switch (type)
                {
                    case Type.Single:
                        Cost = 1;
                        break;

                    case Type.Half:
                        Cost = MovedCompound.Quantity / 2;
                        break;

                    case Type.Full:
                        Cost = MovedCompound.Quantity;
                        break;

                    case Type.Max:
                        Cost = MovedCompound.Quantity;
                        break;
                }
        }

        public override bool Prepare()
        {
            if (MovedCompound == null || !IsMoleculeValidForOutput(MovedCompound.Molecule))
                Fail();
            else if (type == Type.Single && MovedCompound.Quantity < 1)
                Fail();

            return base.Prepare();
        }

        public override void Begin()
        {
            base.Begin();

            OutputtedCompound = MovedCompound.Split(Scale);
        }

        public enum Type { Single, Half, Full, Max }
    }

    public class SwapCommand : Command
    {
        public Cell.Slot SlotA { get; private set; }
        public Cell.Slot SlotB { get; private set; }

        public Compound CompoundA { get; private set; }
        public Compound CompoundB { get; private set; }

        public SwapCommand(Cell.Slot slot, int step_codon_index, Cell.Slot slot_a, Cell.Slot slot_b) : base(slot, step_codon_index, 1)
        {
            SlotA = slot_a;
            SlotB = slot_b;

            Cost = (SlotA.Compound != null ? SlotA.Compound.Quantity : 0) + 
                   (SlotB.Compound != null ? SlotB.Compound.Quantity : 0);
        }

        public override bool Prepare()
        {
            return base.Prepare();
        }

        public override void Begin()
        {
            CompoundA = SlotA.RemoveCompound();
            CompoundB = SlotB.RemoveCompound();

            base.Begin();
        }

        public override void End()
        {
            if (CompoundA != null)
                SlotB.AddCompound(CompoundA);

            if (CompoundB != null)
                SlotA.AddCompound(CompoundB);

            base.End();
        }
    }

    public class GoToCommand : Command
    {
        string marker;
        int seek_count;

        public GoToCommand(Cell.Slot slot, int step_codon_index, string marker_, int seek_count_) : base(slot, step_codon_index, 0)
        {
            marker = marker_;
            seek_count = seek_count_;
        }

        public override void End()
        {
            for (int i = 0; i < seek_count; i++)
                Interpretase.SeekToMarker(GetDNA(), marker, seek_count < 0);
        }
    }

    public class TryCommand : Command
    {
        Command command;
        string marker;

        public TryCommand(Cell.Slot slot, int step_codon_index, Command command_, string marker_) : base(slot, step_codon_index, command_.Cost)
        {
            command = command_;
            marker = marker_;
        }

        public override bool Prepare()
        {
            command.Prepare();

            return base.Prepare();
        }

        public override void Begin()
        {
            if(!command.HasFailed)
                command.Begin();

            base.Begin();
        }

        public override void End()
        {
            if (!command.HasFailed)
            {
                command.End();
                base.End();
            }
            else
                Interpretase.SeekToMarker(GetDNA(), marker);
        }
    }

    class NullCommand : Command
    {
        public NullCommand(Cell.Slot slot, int step_codon_index) : base(slot, step_codon_index, 0)
        {

        }
    }
}
