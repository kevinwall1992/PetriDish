using UnityEngine;


public class Interpretase : ProgressiveCatalyst
{
    public override int Power { get { return 10; } }

    public Interpretase() : base("Interpretase", 3, "Interprets DNA programs")
    {

    }

    protected override Action GetAction(Cell.Slot catalyst_slot)
    {
        DNA dna = GetDNA(catalyst_slot);

        return Interpret(catalyst_slot, FindCommandCodon(dna));
    }

    public static Command Interpret(Cell.Slot slot, int command_codon_index)
    {
        if (command_codon_index < 0)
            return null;

        Cell.Slot dna_slot = GetDNASlot(slot);
        DNA dna = GetMoleculeInSlotAs<DNA>(dna_slot);

        string codon = dna.GetCodon(command_codon_index);
        if (codon[0] != 'C')
            return null;

        int step_codon_index = command_codon_index + GetCommandLength(dna, command_codon_index);

        int operand_index = command_codon_index + 1;

        Command null_command = new NullCommand(slot, dna_slot, step_codon_index);

        switch (codon)
        {
            case "CAA":
            case "CCC":
            case "CGG":
            case "CTT":
                object source_location = CodonToLocation(slot, operand_index, out operand_index);
                if (!(source_location is Cell.Slot))
                    return null_command;
                Cell.Slot source_slot = source_location as Cell.Slot;

                float quantity = 0;
                switch (codon)
                {
                    case "CAA": quantity = 1; break;
                    case "CCC": quantity = source_slot.Compound.Quantity / 2; break;
                    case "CGG": quantity = source_slot.Compound.Quantity; break;
                    case "CTT": quantity = slot.Compound.Quantity; break;
                }

                object destination_location = CodonToLocation(slot, operand_index, out operand_index);

                return new MoveCommand(slot, dna_slot, step_codon_index, source_slot, destination_location, quantity);

            case "CCA":
                object a_location = CodonToLocation(slot, operand_index, out operand_index);
                if (!(a_location is Cell.Slot))
                    return null_command;

                object b_location = CodonToLocation(slot, operand_index, out operand_index);
                if (!(b_location is Cell.Slot))
                    return null_command;

                return new SwapCommand(slot, dna_slot, step_codon_index, a_location as Cell.Slot, b_location as Cell.Slot);

            case "CAC":
                object activation_location = CodonToLocation(slot, operand_index, out operand_index);
                if (!(activation_location is Cell.Slot))
                    return null_command;

                int activation_count = ComputeFunction(slot, operand_index);

                return new ActivateCommand(slot, dna_slot, step_codon_index, activation_location as Cell.Slot, activation_count);

            case "CAG":
                object goto_location = CodonToLocation(slot, operand_index, out operand_index);
                if (!(goto_location is string))
                    return null_command;

                int condition_value = ComputeFunction(slot, operand_index);
                
                if (condition_value != 0)
                    return new GoToCommand(slot, dna_slot, step_codon_index, goto_location as string, condition_value);
                else
                    return null_command;

            case "CCG":
                object else_location = CodonToLocation(slot, operand_index, out operand_index);
                if (!(else_location is string))
                    return null_command;

                return new TryCommand(slot, 
                                      dna_slot, 
                                      step_codon_index, 
                                      Interpret(slot, FindCommandCodon(dna, command_codon_index + 1)), 
                                      else_location as string);

            case "CAT":
                object cut_location = CodonToLocation(slot, operand_index, out operand_index);
                if (!(cut_location is string))
                    return null_command;

                object paste_location = CodonToLocation(slot, operand_index, out operand_index);
                if (!(paste_location is Cell.Slot))
                    return null_command;

                return new CutCommand(slot,
                                      dna_slot,
                                      step_codon_index - GetSegmentLength(dna, cut_location as string, command_codon_index), 
                                      paste_location as Cell.Slot, 
                                      cut_location as string, 
                                      command_codon_index);
        }

        return null;
    }

    static Cell.Slot GetDNASlot(Cell.Slot catalyst_slot)
    {
        return catalyst_slot.PreviousSlot;
    }

    static DNA GetDNA(Cell.Slot catalyst_slot)
    {
        return GetMoleculeInSlotAs<DNA>(GetDNASlot(catalyst_slot));
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

    public static int FindMarkerCodon(DNA dna, string marker, int starting_codon_index, bool search_backwards = false, bool circular = true)
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
                    {
                        if (!circular)
                            return -1;

                        codon_index = dna.CodonCount - 1;
                    }
                    else if (codon_index >= dna.CodonCount)
                    {
                        if (!circular)
                            return -1;

                        codon_index = 0;
                    }

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

    public static int FindMarkerCodon(DNA dna, string marker, bool search_backwards = false, bool circular = true)
    {
        return FindMarkerCodon(dna, marker, dna.ActiveCodonIndex, search_backwards, circular);
    }

    public static void SeekToMarker(DNA dna, string marker, bool search_backwards = false, bool circular = true)
    {
        dna.ActiveCodonIndex = FindMarkerCodon(dna, marker, search_backwards, circular);
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

            total += MathUtility.Pow(4, exponent) * value;

            exponent--;
        }

        return total;
    }

    public static string ValueToCodon(int value)
    {
        string codon = "";

        for (int significance = 2; significance >= 0; significance--)
        {
            int digit = (value % MathUtility.Pow(4, significance + 1)) / MathUtility.Pow(4, significance);

            switch (digit)
            {
                case 0: codon += "A"; break;
                case 1: codon += "C"; break;
                case 2: codon += "G"; break;
                case 3: codon += "T"; break;
            }
        }

        return codon;
    }

    public static object CodonToLocation(Cell.Slot catalyst_slot, int codon_index, out int next_codon_index)
    {
        DNA dna = GetDNA(catalyst_slot);
        string codon = dna.GetCodon(codon_index);

        next_codon_index = codon_index + 1;

        if (codon[0] == 'T')
            return codon;
        else if (codon[0] == 'G')
            return catalyst_slot.Cell.Slots[catalyst_slot.Index + ComputeFunction(catalyst_slot, codon_index, out next_codon_index)];

        int value = CodonToValue(codon);

        if (value < 6)
            return catalyst_slot.Cell.Slots[catalyst_slot.Index + value];
        else if (value == 6)
        {
            if (catalyst_slot.IsExposed)
                return catalyst_slot.Cell.Organism.Locale;
            else
                return catalyst_slot.AcrossSlot;  
        }
        else if (value == 7)
            return catalyst_slot.Cell.Organism.Cytozol;

        return null;
    }

    public static object CodonToLocation(Cell.Slot catalyst_slot, int codon_index)
    {
        return CodonToLocation(catalyst_slot, codon_index, out codon_index);
    }

    public static int ComputeFunction(Cell.Slot catalyst_slot, int codon_index, out int next_codon_index)
    {
        DNA dna = GetDNA(catalyst_slot);
        string function_codon = dna.GetCodon(codon_index);

        next_codon_index = codon_index + 1;

        switch (function_codon)
        {
            case "GAA":
                object location = CodonToLocation(catalyst_slot, next_codon_index, out next_codon_index);
                if (!(location is Cell.Slot))
                    return 0;

                Cell.Slot query_slot =  location as Cell.Slot;
                return query_slot.Compound == null ? 0 : (int)query_slot.Compound.Quantity;

            case "GAC":
            case "GAT":
            case "GAG":
            case "GCA":
            case "GCC":
                int operand0 = ComputeFunction(catalyst_slot, next_codon_index, out next_codon_index);
                int operand1 = ComputeFunction(catalyst_slot, next_codon_index, out next_codon_index);

                switch (function_codon)
                {
                    case "GAC": return operand0 > operand1 ? 1 : 0;
                    case "GAT": return operand0 < operand1 ? 1 : 0;
                    case "GAG": return operand0 == operand1 ? 1 : 0;
                    case "GCA": return operand0 + operand1;
                    case "GCC": return operand0 - operand1;
                }
                break;
        }

        return CodonToValue(function_codon);
    }

    public static int ComputeFunction(Cell.Slot catalyst_slot, int function_codon_index)
    {
        int next_codon_index;

        return ComputeFunction(catalyst_slot, function_codon_index, out next_codon_index);
    }

    public override Catalyst Copy()
    {
        return new Interpretase().CopyStateFrom(this);
    }


    public class Command : Action
    {
        int step_codon_index;

        public Cell.Slot DNASlot { get; private set; }
        public DNA DNA
        {
            get
            {
                Debug.Assert(DNASlot.Compound.Molecule is DNA);

                return DNASlot.Compound.Molecule as DNA;
            }
        }

        public Command(Cell.Slot slot, Cell.Slot dna_slot, int step_codon_index_, float cost) : base(slot, cost)
        {
            DNASlot = dna_slot;
            step_codon_index = step_codon_index_;
        }

        public override bool Prepare() { return true; }

        public override void Begin() { }

        public override void End()
        {
            if (!HasFailed)
                DNA.ActiveCodonIndex = step_codon_index;
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

        public OutputCommand(Cell.Slot slot, Cell.Slot dna_slot, int step_codon_index, Cell.Slot output_slot_) 
            : base(slot, dna_slot, step_codon_index, 1)
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

        public CutCommand(Cell.Slot slot, Cell.Slot dna_slot, int step_codon_index, Cell.Slot output_slot, string marker_, int local_codon_index_) 
            : base(slot, dna_slot, step_codon_index, output_slot)
        {
            marker = marker_;
            local_codon_index = local_codon_index_;
        }

        public override bool Prepare()
        {
            if (OutputSlot.Compound != null && !(OutputSlot.Compound.Molecule is DNA))
                Fail();
            else if(Interpretase.GetSegmentLength(DNA, marker, local_codon_index)== 0)
                Fail();

            return !HasFailed;
        }

        public override void Begin()
        {
            base.Begin();

            DNA dna = Interpretase.Cut(DNA, marker, local_codon_index);
                
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

        public ActivateCommand(Cell.Slot slot, Cell.Slot dna_slot, int step_codon_index, Cell.Slot output_slot, int activation_count_) 
            : base(slot, dna_slot, step_codon_index, output_slot)
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

    public class MoveCommand : ActionCommand
    {
        public MoveCommand(Cell.Slot slot, Cell.Slot dna_slot, int step_codon_index, Cell.Slot source, object destination, float quantity)
            : base(slot, dna_slot, step_codon_index, CreateMoveAction(slot, source, destination, quantity))
        {

        }

        static Action CreateMoveAction(Cell.Slot slot, Cell.Slot source, object destination, float quantity)
        {
            if (destination is Cell.Slot)
                return new MoveToSlotAction(slot, source, destination as Cell.Slot, quantity);
            else if (destination is Cytozol)
                return new MoveToCytozolAction(slot, source, destination as Cytozol, quantity);
            else if (destination is Locale)
                return new MoveToLocaleAction(slot, source, destination as Locale, quantity);

            Debug.Assert(false, "Invalid destination.");
            return null;
        }
    }

    public class SwapCommand : Command
    {
        public Cell.Slot SlotA { get; private set; }
        public Cell.Slot SlotB { get; private set; }

        public Compound CompoundA { get; private set; }
        public Compound CompoundB { get; private set; }

        public SwapCommand(Cell.Slot slot, Cell.Slot dna_slot, int step_codon_index, Cell.Slot slot_a, Cell.Slot slot_b) 
            : base(slot, dna_slot, step_codon_index, 1)
        {
            SlotA = slot_a;
            SlotB = slot_b;

            Cost = GetTotalQuantityMoved();
        }

        float GetTotalQuantityMoved()
        {
            return (SlotA.Compound != null ? SlotA.Compound.Quantity : 0) +
                   (SlotB.Compound != null ? SlotB.Compound.Quantity : 0);
        }

        public override bool Prepare()
        {
            if (Cost < GetTotalQuantityMoved())
                Fail();

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

        public GoToCommand(Cell.Slot slot, Cell.Slot dna_slot, int step_codon_index, string marker_, int seek_count_) 
            : base(slot, dna_slot, step_codon_index, 0)
        {
            marker = marker_;
            seek_count = seek_count_;
        }

        public override void End()
        {
            for (int i = 0; i < seek_count; i++)
                Interpretase.SeekToMarker(DNA, marker, seek_count < 0);
        }
    }

    public class TryCommand : Command
    {
        Command command;
        string marker;

        public TryCommand(Cell.Slot slot, Cell.Slot dna_slot, int step_codon_index, Command command_, string marker_) 
            : base(slot, dna_slot, step_codon_index, command_.Cost)
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
                Interpretase.SeekToMarker(DNA, marker);
        }
    }

    public class ActionCommand : Command
    {
        public Action Action { get; private set; }

        public ActionCommand(Cell.Slot slot, Cell.Slot dna_slot, int step_codon_index, Action action) 
            : base(slot, dna_slot, step_codon_index, action.Cost)
        {
            Action = action;
        }

        public override bool Prepare()
        {
            if (!Action.Prepare())
                Fail();

            return base.Prepare();
        }

        public override void Begin()
        {
            if (!HasFailed)
                Action.Begin();

            base.Begin();
        }

        public override void End()
        {
            if (!HasFailed)
                Action.End();

            base.End();
        }
    }

    public class NullCommand : Command
    {
        public NullCommand(Cell.Slot slot, Cell.Slot dna_slot, int step_codon_index) 
            : base(slot, dna_slot, step_codon_index, 0)
        {

        }
    }
}
