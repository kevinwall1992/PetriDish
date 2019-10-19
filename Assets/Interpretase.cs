using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpretase : ProgressiveCatalyst
{
    public override int Power { get { return 10; } }

    public Grabber Grabber { get; private set; }
    public OutputAttachment OutputAttachment { get; private set; }

    public DNA DNA { get { return GetGeneticCofactor(this); } }

    public Interpretase() : base("Interpretase", 3, "Interprets DNA programs")
    {
        Attachments[Cell.Slot.Relation.Across] = Grabber = new Grabber();
        Attachments[Cell.Slot.Relation.Right] = OutputAttachment = new OutputAttachment();
    }

    public override bool CanAddCofactor(Compound cofactor)
    {
        return cofactor.Molecule is DNA;
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (DNA == null)
        {
            Cell.Slot input_slot = Grabber.GetSlotPointedAt(slot);
            if (input_slot.Compound != null && input_slot.Compound.Molecule is DNA)
                return new LoadProgram(slot);

            return null;
        }

        return Interpret(slot, FindCommandCodon(DNA));
    }

    public Command Interpret(Cell.Slot slot, int command_codon_index)
    {
        if (command_codon_index < 0)
            return null;

        string codon = DNA.GetCodon(command_codon_index);
        if (codon[0] != 'C')
            return null;

        Cell.Slot grab_slot = slot.GetAdjacentSlot(Orientation);

        int step_codon_index = command_codon_index + GetOperandCount(DNA, command_codon_index) + 1;

        int operand_index = command_codon_index + 1;

        switch (codon)
        {
            //Self movement
            case "CVV":

            //Taking
            case "CVC":
                Cell.Slot.Relation direction = CodonToDirection(slot, DNA, operand_index, out operand_index);

                if (codon == "CVV")
                    return new MoveCommand(slot, command_codon_index, direction);
                else
                    return new MoveCommand(slot,
                                           command_codon_index,
                                           direction,
                                           ComputeFunction(slot, DNA, operand_index, out operand_index));

            //Grab
            case "CVF":
                return new GrabCommand(slot, command_codon_index);

            //Release
            case "CVL":
                return new ReleaseCommand(slot, command_codon_index);

            //Spin
            case "CCC":
                int value = ComputeFunction(slot, DNA, operand_index, out operand_index);

                return new SpinCommand(slot, command_codon_index, SpinCommand.ValueToDirection(value));

            //Copy
            case "CFF":
                string start_marker = DNA.GetCodon(operand_index++);
                if (start_marker[0] != 'L')
                    return null;

                string stop_marker = DNA.GetCodon(operand_index++);
                if (stop_marker[0] != 'L')
                    return null;

                return new CopyCommand(slot,
                                         command_codon_index,
                                         slot.GetAdjacentSlot(Orientation),
                                         start_marker as string,
                                         stop_marker as string);

            case "CLV":
                return new PassCommand(slot, command_codon_index);

            //Wait
            case "CLC":
                if(ComputeFunction(slot, DNA, operand_index, out operand_index)!= 0)
                {
                    DNA.ActiveCodonIndex = command_codon_index + GetOperandCount(DNA, command_codon_index) + 1;
                    return Interpret(slot, FindCommandCodon(DNA));
                }
                break;

            //If
            case "CLF":

            //Try
            case "CLL":
                string else_marker = DNA.GetCodon(operand_index++);
                if (else_marker[0] != 'L')
                    return null;

                if (codon == "CLF")
                {
                    int condition_value = ComputeFunction(slot, DNA, operand_index, out operand_index);

                    if (condition_value == 0)
                        return new GoToCommand(slot,
                                                command_codon_index,
                                                else_marker);
                    else
                        return new PassCommand(slot, command_codon_index);
                }
                else
                    return new TryCommand(slot,
                                          command_codon_index,
                                          Interpret(slot, FindCommandCodon(DNA, command_codon_index + 1)),
                                          else_marker);

        }

        return null;
    }

    //Gets the operand count of either a command or function
    public static int GetOperandCount(DNA dna, int codon_index)
    {
        string codon = dna.GetCodon(codon_index);
        if (codon[0] != 'C' && codon[0] != 'F')
            return 0;

        int length = 0;
        int operand_count = 0;

        do
        {
            string operand_codon = dna.GetCodon(codon_index + length);

            if (length > 0)
                operand_count--;
            length++;

            switch (operand_codon[0])
            {
                case 'A': break;

                case 'C':
                    switch (operand_codon)
                    {
                        case "CVF":
                        case "CVL":
                        case "CLV":
                            operand_count += 0;
                            break;

                        case "CVV":
                        case "CCC":
                        case "CLC":
                            operand_count += 1;
                            break;

                        case "CVC":
                        case "CLF":
                        case "CLL":
                        case "CFF":
                            operand_count += 2;
                            break;
                    }
                    break;

                case 'F':
                    switch (operand_codon)
                    {
                        case "FVV":
                            break;

                        case "FVC":
                        case "FVG":
                        case "FVF":
                        case "FVL":
                        case "FCV":
                        case "FCC":
                            operand_count += 2;
                            break;
                    }
                    break;

                case 'L': break;
            }
        } while (operand_count > 0);

        return length - 1;
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
            if (!is_first_codon || dna.GetCodon(codon_index) != marker)
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
                (command_codon_index + GetOperandCount(dna, command_codon_index)) < codon_index)
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

    public static string GetMarkedSequence(DNA dna, string start_marker, string stop_marker, int local_codon_index)
    {
        int start_codon_index = FindMarkerCodon(dna, start_marker, local_codon_index) + 1;
        int stop_codon_index = FindMarkerCodon(dna, stop_marker, start_codon_index) - 1;

        if (stop_codon_index > start_codon_index)
            return dna.GetSubsequence(start_codon_index, stop_codon_index - start_codon_index + 1);
        else
            return dna.GetSubsequence(start_codon_index, dna.CodonCount - start_codon_index) + 
                   dna.GetSubsequence(0, stop_codon_index + 1);
    }

    public static Catalyst GetCatalyst(DNA dna, int codon_index, out int length)
    {
        string catalyst_sequence = "";

        for (; codon_index < dna.CodonCount; codon_index++)
        {
            string codon = dna.GetCodon(codon_index);

            switch (codon[0])
            {
                case 'V':
                case 'F':
                    catalyst_sequence += codon;

                    Ribozyme ribozyme = Ribozyme.GetRibozyme(catalyst_sequence);
                    Protein protein = Protein.GetProtein(Protein.DNASequenceToAminoAcidSequence(catalyst_sequence));

                    if (ribozyme != null || protein != null)
                    {
                        length = catalyst_sequence.Length / 3;

                        if (ribozyme != null)
                            return ribozyme;
                        else
                            return protein;
                    }

                    break;

                case 'C':
                case 'L':
                    length = -1;
                    return null;

            }
        }

        length = -1;
        return null;
    }

    public static Catalyst GetCatalyst(DNA dna, int codon_index)
    {
        int length;
        return GetCatalyst(dna, codon_index, out length);
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
                case 'V': value = 0; break;
                case 'C': value = 1; break;
                case 'F': value = 2; break;
                case 'L': value = 3; break;
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
                case 0: codon += "V"; break;
                case 1: codon += "C"; break;
                case 2: codon += "F"; break;
                case 3: codon += "L"; break;
            }
        }

        return codon;
    }

    public static Cell.Slot.Relation ValueToDirection(int value)
    {
        return (Cell.Slot.Relation)MathUtility.Mod(value, 3);
    }

    public static Cell.Slot.Relation CodonToDirection(string codon)
    {
        return ValueToDirection(CodonToValue(codon));
    }

    public static Cell.Slot.Relation CodonToDirection(Cell.Slot slot, DNA dna, int codon_index, out int next_codon_index)
    {
        return ValueToDirection(ComputeFunction(slot, dna, codon_index, out next_codon_index));
    }

    public static Cell.Slot.Relation CodonToDirection(Cell.Slot slot, DNA dna, int codon_index)
    {
        int next_codon_index;

        return CodonToDirection(slot, dna, codon_index, out next_codon_index);
    }

    public static int ComputeFunction(Cell.Slot slot, DNA dna, int codon_index, out int next_codon_index)
    {
        string function_codon = dna.GetCodon(codon_index);

        next_codon_index = codon_index + 1;

        switch (function_codon)
        {
            case "FVV":
                Interpretase interpretase = (slot.Compound.Molecule as Catalyst).GetFacet<Interpretase>();
                Cell.Slot query_slot = interpretase.Grabber.GetSlotPointedAt(slot);
                return query_slot.Compound == null ? 0 : (int)query_slot.Compound.Quantity;

            case "FVC":
            case "FVL":
            case "FVF":
            case "FCV":
            case "FCC":
                int operand0 = ComputeFunction(slot, dna, next_codon_index, out next_codon_index);
                int operand1 = ComputeFunction(slot, dna, next_codon_index, out next_codon_index);

                switch (function_codon)
                {
                    case "FVC": return operand0 > operand1 ? 1 : 0;
                    case "FVL": return operand0 < operand1 ? 1 : 0;
                    case "FVF": return operand0 == operand1 ? 1 : 0;
                    case "FCV": return operand0 + operand1;
                    case "FCC": return operand0 - operand1;
                }
                break;
        }

        return CodonToValue(function_codon);
    }

    public static int ComputeFunction(Cell.Slot slot, DNA dna, int function_codon_index)
    {
        int next_codon_index;

        return ComputeFunction(slot, dna, function_codon_index, out next_codon_index);
    }

    public static DNA GetGeneticCofactor(Catalyst catalyst)
    {
        foreach (Compound compound in catalyst.Cofactors)
            if (compound.Molecule is DNA && !(compound.Molecule is Catalyst))
                return compound.Molecule as DNA;

        return null;
    }


    public override bool Equals(object obj)
    {
        if (!base.Equals(obj))
            return false;

        return Grabber.IsGrabbing == (obj as Interpretase).Grabber.IsGrabbing;
    }

    public override Catalyst Copy()
    {
        Interpretase other = new Interpretase();
        other.Grabber.IsGrabbing = Grabber.IsGrabbing;

        return other.CopyStateFrom(this);
    }


    public class LoadProgram : EnergeticAction
    {
        public override bool IsLegal
        {
            get
            {
                Compound compound = ProgramSlot.Compound;

                if (compound == null || !(compound.Molecule is DNA))
                    return false;

                return true;
            }
        }

        public Interpretase Interpretase { get { return Catalyst.GetFacet<Interpretase>(); } }

        public Cell.Slot ProgramSlot
        {
            get { return Interpretase.Grabber.GetSlotPointedAt(CatalystSlot); }
        }

        public Compound ProgramCompound { get; private set; }

        public LoadProgram(Cell.Slot catalyst_slot) : base(catalyst_slot, 1, -0.1f)
        {
            Cost = Interpretase.Grabber.GetSlotPointedAt(catalyst_slot).Compound.Quantity;
        }

        public override Dictionary<object, List<Compound>> GetResourceDemands()
        {
            Dictionary<object, List<Compound>> resource_demands = base.GetResourceDemands();
            resource_demands[ProgramSlot] = Utility.CreateList(ProgramSlot.Compound);

            return resource_demands;
        }

        public override void Begin()
        {
            base.Begin();

            ProgramCompound = ProgramSlot.RemoveCompound();
        }

        public override void End()
        {
            (ProgramCompound.Molecule as DNA).InsertSequence(0, "LLL");

            Interpretase.AddCofactor(ProgramCompound);

            base.End();
        }
    }


    public class Command : EnergeticAction
    {
        protected int CommandCodonIndex { get; set; }
        protected int NextCodonIndex { get { return CommandCodonIndex + GetOperandCount(Interpretase.DNA, CommandCodonIndex) + 1; } }

        public Interpretase Interpretase { get { return Catalyst.GetFacet<Interpretase>(); } }

        public Command(Cell.Slot catalyst_slot, int command_codon_index, float cost, float energy_balance) 
            : base(catalyst_slot, cost, energy_balance)
        {
            CommandCodonIndex = command_codon_index;
        }

        public override void End()
        {
            Interpretase.DNA.ActiveCodonIndex = NextCodonIndex;

            base.End();
        }
    }

    public class CopyCommand : Command
    {
        string start_marker, stop_marker;

        public Cell.Slot Destination { get; private set; }
        public Compound CopiedCompound { get; private set; }

        public string SequenceToBeCopied
        {
            get { return GetMarkedSequence(Interpretase.DNA, start_marker, stop_marker, CommandCodonIndex); }
        }

        public override float Scale
        {
            get
            {
                base.Scale = CatalystSlot.Compound.Quantity;

                return base.Scale;
            }
        }

        public override bool IsLegal
        {
            get
            {
                if (Destination.Compound != null)
                {
                    if (!(Destination.Compound.Molecule is DNA))
                        return false;
                    else if (SequenceToBeCopied != (Destination.Compound.Molecule as DNA).Sequence)
                        return false;
                }

                return base.IsLegal;
            }
        }

        public CopyCommand(Cell.Slot catalyst_slot, int command_codon_index, Cell.Slot output_slot, 
                           string start_marker_, string stop_marker_) 
            : base(catalyst_slot, command_codon_index, 1, -0.5f)
        {
            Destination = output_slot;
            start_marker = start_marker_;
            stop_marker = stop_marker_;

            Cost = SequenceToBeCopied.Length / 10.0f;
        }

        public override void Begin()
        {
            base.Begin();

            CopiedCompound = new Compound(new DNA(SequenceToBeCopied), 1);
        }

        public override void End()
        {
            Destination.AddCompound(CopiedCompound);

            base.End();
        }
    }

    public class GrabCommand : Command
    {
        public GrabCommand(Cell.Slot catalyst_slot, int command_codon_index)
            : base(catalyst_slot, command_codon_index, 0.1f, -0.1f)
        {

        }

        public override void End()
        {
            Interpretase.Grabber.Grab();

            base.End();
        }
    }

    public class ReleaseCommand : Command
    {
        public ReleaseCommand(Cell.Slot catalyst_slot, int command_codon_index)
            : base(catalyst_slot, command_codon_index, 0.1f, 0.1f)
        {

        }

        public override void End()
        {
            Interpretase.Grabber.Release();

            base.End();
        }
    }

    public class GoToCommand : Command
    {
        int seek_count = 1;

        public string Marker { get; private set; }

        public GoToCommand(Cell.Slot catalyst_slot, int command_codon_index, string marker) 
            : base(catalyst_slot, command_codon_index, 0, 0)
        {
            Marker = marker;
        }

        public override void End()
        {
            for (int i = 0; i < seek_count; i++)
                SeekToMarker(Interpretase.DNA, Marker, seek_count < 0);
        }
    }

    public class TryCommand : Command
    {
        string marker;
        bool command_has_failed = false;

        public Command Command { get; private set; }

        public override bool IsLegal
        {
            get
            {
                if (!base.IsLegal)
                    return false;

                if(Command == null || !Command.IsLegal)
                    command_has_failed = true;

                return true;
            }
        }

        public TryCommand(Cell.Slot catalyst_slot, int command_codon_index, Command command, string marker_) 
            : base(catalyst_slot, command_codon_index, command != null ? command.Cost : 0, 0)
        {
            Command = command;
            marker = marker_;
        }

        public override void Begin()
        {
            base.Begin();

            if(!command_has_failed)
                Command.Begin();
        }

        public override void End()
        {
            if (!command_has_failed)
            {
                Command.End();
                base.End();
            }
            else
                SeekToMarker(Interpretase.DNA, marker);
        }
    }

    public class ActionCommand : Command
    {
        public Action Action { get; private set; }

        public override bool IsLegal
        {
            get
            {
                if (Action != null && !Action.IsLegal)
                    return false;

                return base.IsLegal;
            }
        }

        public ActionCommand(Cell.Slot catalyst_slot, int command_codon_index, Action action, 
                             float command_cost, float energy_balance) 
            : base(catalyst_slot, command_codon_index, command_cost, energy_balance)
        {
            SetAction(action);
        }

        protected void SetAction(Action action)
        {
            Debug.Assert(Action == null, "ActionCommand : attempted to set action more than once.");

            Action = action;

            if (Action != null)
                BaseCost += Action.Cost;
        }

        public override Dictionary<object, List<Compound>> GetResourceDemands()
        {
            Dictionary<object, List<Compound>> resource_demands = base.GetResourceDemands();

            if (Action != null)
            {
                Dictionary<object, List<Compound>> action_resource_demands = Action.GetResourceDemands();
                foreach (object source in action_resource_demands.Keys)
                {
                    if (!resource_demands.ContainsKey(source))
                        resource_demands[source] = new List<Compound>();

                    resource_demands[source].AddRange(action_resource_demands[source]);
                }
            }

            return resource_demands;
        }

        public override void Begin()
        {
            base.Begin();

            if(HasBegun && Action != null)
                Action.Begin();
        }

        public override void End()
        {
            if(Action != null)
                Action.End();

            base.End();
        }
    }

    //This class is reused for "Taking"
    //Taking is removal of part of the stack in the grab direction
    //While also moving. This begins a grab
    public class MoveCommand : ActionCommand
    {
        PushAction push_action;

        public bool IsTake { get; private set; }
        public Cell.Slot.Relation Direction { get; private set; }

        public Compound MovingCompound { get { return push_action.PushingCompound; } }

        public Cell.Slot.Relation FinalOrientation
        {
            get
            {
                if (Direction == Catalyst.Orientation)
                    return Catalyst.Orientation;

                return CatalystSlot.GetAdjacentSlot(Direction).GetRelation(CatalystSlot);
            }
        }

        public override bool IsLegal
        {
            get
            {
                if (CatalystSlot.Compound == null ||
                    CatalystSlot.Compound.Molecule != Catalyst)
                    return false;

                if (Direction == Cell.Slot.Relation.Across && 
                    CatalystSlot.AcrossSlot == null)
                    return false;

                return base.IsLegal;
            }
        }

        public MoveCommand(Cell.Slot catalyst_slot, 
                           int command_codon_index, Cell.Slot.Relation direction, float quantity = -1) 
            : base(catalyst_slot, command_codon_index, null, 0, 0)
        {
            Direction = direction;

            IsTake = quantity > 0;

            //Grabbing if this is a "Take"
            GrabCommand grab_command = IsTake ? new GrabCommand(catalyst_slot, command_codon_index) : null;

            //Pushing Compounds out of the way
            push_action = new PushAction(catalyst_slot, catalyst_slot, direction);

            //Dragging grabbed compound behind us
            MoveToSlotAction move_action = null;
            Cell.Slot grab_slot = catalyst_slot.GetAdjacentSlot(Catalyst.Orientation);
            if ((Interpretase.Grabber.IsGrabbing || IsTake) &&
                grab_slot != null &&
                grab_slot.Compound != null &&
                Catalyst.Orientation != direction &&
                !push_action.IsFullPush)

                move_action = new MoveToSlotAction(catalyst_slot, catalyst_slot.GetAdjacentSlot(Catalyst.Orientation), catalyst_slot, quantity);

            SetAction(new CompositeAction(catalyst_slot, grab_command, push_action, move_action));
        }

        public override void End()
        {
            Catalyst.Orientation = FinalOrientation;

            base.End();
        }
    }

    public class SpinCommand : ActionCommand
    {
        //Right : Clockwise
        //Left : Counter clockwise
        public enum Direction { Right, Left }

        Direction direction;

        public bool IsRightSpin { get { return direction == Direction.Right; } }

        public override bool IsLegal
        {
            get
            {
                if (CatalystSlot.Compound == null ||
                    CatalystSlot.Compound.Molecule != Catalyst)
                    return false;

                return base.IsLegal;
            }
        }

        public SpinCommand(Cell.Slot catalyst_slot, int command_codon_index, Direction direction_)
            : base(catalyst_slot, command_codon_index, null, 1, 0.1f)
        {
            direction = direction_;

            Cell.Slot grab_slot = catalyst_slot.GetAdjacentSlot(Catalyst.Orientation);
            if (Interpretase.Grabber.IsGrabbing)
            {
                List<Action> actions = new List<Action>();

                Cell.Slot source = grab_slot;

                while (source != null && source.Compound!= null)
                {
                    Cell.Slot.Relation move_direction = Cell.Slot.RotateRelation(catalyst_slot.GetRelation(source), IsRightSpin);
                    Cell.Slot destination = catalyst_slot.GetAdjacentSlot(move_direction);

                    if (destination == null)
                        actions.Add(new MoveToLocaleAction(catalyst_slot, source));
                    else
                        actions.Add(new MoveToSlotAction(catalyst_slot, source, destination));

                    source = destination;
                }

                SetAction(new CompositeAction(catalyst_slot, actions.ToArray()));                
            }

            Cost = CatalystSlot.Compound.Quantity;
        }

        public override void End()
        {
            base.End();

            if(Interpretase.Grabber.IsGrabbing)
                for (int i = 0; i < 3; i++)
                {
                    Compound compound = CatalystSlot.GetAdjacentSlot((Cell.Slot.Relation)i).Compound;

                    if (compound == null || !(compound.Molecule is Catalyst))
                        continue;

                    Catalyst catalyst = (compound.Molecule as Catalyst);
                    catalyst.Orientation = Cell.Slot.RotateRelation(catalyst.Orientation, IsRightSpin);
                }

            Catalyst.Orientation = Cell.Slot.RotateRelation(Catalyst.Orientation, IsRightSpin);
        }

        public static Direction ValueToDirection(int value)
        {
            if (value % 2 == 0)
                return Direction.Right;
            else
                return Direction.Left;
        }

        public static Direction CodonToDirection(string codon)
        {
            return ValueToDirection(CodonToValue(codon));
        }
    }

    public class PassCommand : Command
    {
        public PassCommand(Cell.Slot catalyst_slot, int command_codon_index) 
            : base(catalyst_slot, command_codon_index, catalyst_slot.Compound.Quantity, 0)
        {
            
        }
    }
}


public class Grabber : Attachment
{
    public bool IsGrabbing { get; set; }

    public void Grab() { IsGrabbing = true; }
    public void Release() { IsGrabbing = false; }
}
