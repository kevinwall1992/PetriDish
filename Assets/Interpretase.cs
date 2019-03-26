using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Interpretase : ProgressiveCatalyst
{
    public bool IsGrabbing { get; private set; }
    public void Grab() { IsGrabbing = true; }
    public void Release() { IsGrabbing = false; }

    public override int Power { get { return 10; } }

    public DNA DNA { get { return GetGeneticCofactor(this); } }

    public Interpretase() : base("Interpretase", 3, "Interprets DNA programs")
    {

    }

    public override bool CanAddCofactor(Compound cofactor)
    {
        return cofactor.Molecule is DNA;
    }

    protected override Action GetAction(Cell.Slot slot)
    {
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

        int step_codon_index = command_codon_index + GetCommandLength(DNA, command_codon_index);

        int operand_index = command_codon_index + 1;

        switch (codon)
        {
            //Self movement
            case "CAA":

            //Taking
            case "CAC":
                Cell.Slot.Relation direction = CodonToDirection(slot, DNA, operand_index, out operand_index);

                if (codon == "CAA")
                    return new MoveCommand(slot, command_codon_index, direction);
                else
                    return new MoveCommand(slot,
                                           command_codon_index,
                                           direction,
                                           ComputeFunction(slot, DNA, operand_index, out operand_index));

            //Grab
            case "CAG":
                return new GrabCommand(slot, command_codon_index);

            //Release
            case "CAT":
                return new ReleaseCommand(slot, command_codon_index);

            //Spin
            case "CCC":
                int value = ComputeFunction(slot, DNA, operand_index, out operand_index);

                return new SpinCommand(slot, command_codon_index, SpinCommand.ValueToDirection(value));

            //Excise
            case "CGG":
                string excise_marker = DNA.GetCodon(operand_index++);
                if (excise_marker[0] != 'T')
                    return null;
                
                return new ExciseCommand(slot,
                                         command_codon_index,
                                         slot.GetAdjacentSlot(Orientation),
                                         excise_marker as string);


            //If
            case "CTG":

            //Try
            case "CTT":
                string else_marker = DNA.GetCodon(operand_index++);
                if (else_marker[0] != 'T')
                    return null;

                if (codon == "CTG")
                {
                    int condition_value = ComputeFunction(slot, DNA, operand_index, out operand_index);

                    if (condition_value == 0)
                        return new GoToCommand(slot,
                                                command_codon_index,
                                                else_marker);
                    else
                        return new Command(slot, command_codon_index, 0);
                }
                else
                    return new TryCommand(slot,
                                          command_codon_index,
                                          Interpret(slot, FindCommandCodon(DNA, command_codon_index + 1)),
                                          else_marker);

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
                    switch (codon)
                    {
                        case "CAG":
                        case "CAT":
                            operand_count += 0;
                            break;

                        case "CAA":
                        case "CCC":
                        case "CGG":
                            operand_count += 1;
                            break;

                        case "CAC":
                        case "CTG":
                        case "CTT":
                            operand_count += 2;
                            break;
                    }
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

    public static string GetBlockSequence(DNA dna, string marker, int local_codon_index)
    {
        string segment = "";

        int codon_index = FindMarkerCodon(dna, marker, local_codon_index) + 1;
        string codon = dna.GetCodon(codon_index);

        while (codon_index < dna.CodonCount &&
               (codon = dna.GetCodon(codon_index++)) != "TTT")
            segment += codon;

        return segment;
    }

    public static int GetBlockLength(DNA dna, string marker, int local_codon_index)
    {
        return GetBlockSequence(dna, marker, local_codon_index).Length / 3;
    }

    public static DNA Excise(DNA dna, string marker, int local_codon_index)
    {
        return dna.RemoveStrand(FindMarkerCodon(dna, marker, local_codon_index) + 1, GetBlockLength(dna, marker, local_codon_index));
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
            case "GAA":
                Cell.Slot.Relation direction = CodonToDirection(slot, dna, next_codon_index, out next_codon_index);
                if (direction == Cell.Slot.Relation.None)
                    return 0;

                Cell.Slot query_slot = slot.GetAdjacentSlot(direction);
                return query_slot.Compound == null ? 0 : (int)query_slot.Compound.Quantity;

            case "GAC":
            case "GAT":
            case "GAG":
            case "GCA":
            case "GCC":
                int operand0 = ComputeFunction(slot, dna, next_codon_index, out next_codon_index);
                int operand1 = ComputeFunction(slot, dna, next_codon_index, out next_codon_index);

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


    public override Catalyst Copy()
    {
        Interpretase other = new Interpretase();
        other.IsGrabbing = IsGrabbing;

        return other.CopyStateFrom(this);
    }


    public class Command : Action
    {
        protected int CommandCodonIndex { get; set; }
        protected int NextCodonIndex { get { return CommandCodonIndex + GetCommandLength(DNA, CommandCodonIndex); } }

        public DNA DNA { get { return GetGeneticCofactor(Catalyst); } }

        public Command(Cell.Slot catalyst_slot, int command_codon_index, float cost) 
            : base(catalyst_slot, cost)
        {
            CommandCodonIndex = command_codon_index;
        }

        public override void End()
        {
            DNA.ActiveCodonIndex = NextCodonIndex;
        }
    }

    public class ExciseCommand : Command
    {
        string marker;

        public Cell.Slot Destination { get; private set; }
        public Compound RemovedCompound { get; private set; }

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
                    else if (GetBlockSequence(DNA, marker, CommandCodonIndex) != (Destination.Compound.Molecule as DNA).Sequence)
                        return false;
                }

                return base.IsLegal;
            }
        }

        public ExciseCommand(Cell.Slot catalyst_slot, int command_codon_index, Cell.Slot output_slot, string marker_) 
            : base(catalyst_slot, command_codon_index, 1)
        {
            Destination = output_slot;
            marker = marker_;
        }

        public override void Begin()
        {
            base.Begin();

            bool command_codon_index_shifted = FindMarkerCodon(DNA, marker, CommandCodonIndex) < CommandCodonIndex;
            DNA dna = Excise(DNA, marker, CommandCodonIndex);
            if(command_codon_index_shifted)
                CommandCodonIndex -= dna.CodonCount;
                
            Polymer polymer = Ribozyme.GetRibozyme(dna.Sequence);
            if (polymer != null)
                dna = polymer as DNA;

            RemovedCompound = new Compound(dna, 1);
        }

        public override void End()
        {
            Destination.AddCompound(RemovedCompound);

            base.End();
        }
    }

    public class GrabCommand : Command
    {
        public GrabCommand(Cell.Slot catalyst_slot, int command_codon_index)
            : base(catalyst_slot, command_codon_index, 1)
        {

        }

        public override void End()
        {
            Catalyst.GetFacet<Interpretase>().Grab();

            base.End();
        }
    }

    public class ReleaseCommand : Command
    {
        public ReleaseCommand(Cell.Slot catalyst_slot, int command_codon_index)
            : base(catalyst_slot, command_codon_index, 1)
        {

        }

        public override void End()
        {
            Catalyst.GetFacet<Interpretase>().Release();

            base.End();
        }
    }

    public class GoToCommand : Command
    {
        int seek_count = 1;

        public string Marker { get; private set; }

        public GoToCommand(Cell.Slot catalyst_slot, int command_codon_index, string marker) 
            : base(catalyst_slot, command_codon_index, 0)
        {
            Marker = marker;
        }

        public override void End()
        {
            for (int i = 0; i < seek_count; i++)
                SeekToMarker(DNA, Marker, seek_count < 0);
        }
    }

    public class TryCommand : Command
    {
        string marker;
        bool command_has_failed = false;

        public Command Command { get; private set; }

        public TryCommand(Cell.Slot catalyst_slot, int command_codon_index, Command command, string marker_) 
            : base(catalyst_slot, command_codon_index, command != null ? command.Cost : 0)
        {
            Command = command;
            marker = marker_;
        }

        public override void Begin()
        {
            base.Begin();

            if (Command == null || !Command.IsLegal)
                command_has_failed = true;
            else
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
                SeekToMarker(DNA, marker);
        }
    }

    public class ActionCommand : Command
    {
        public Action Action { get; private set; }

        public override bool IsLegal
        {
            get
            {
                if (!Action.IsLegal)
                    return false;

                return base.IsLegal;
            }
        }

        public ActionCommand(Cell.Slot catalyst_slot, int command_codon_index, Action action = null, float command_cost = 0) 
            : base(catalyst_slot, command_codon_index, command_cost)
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

        public override void Begin()
        {
            base.Begin();

            Action.Begin();
        }

        public override void End()
        {
            Action.End();

            base.End();
        }
    }

    //This class is reused for "Taking"
    //Taking is removal of part of the stack in the grab direction
    //While also moving. This begins a grab
    public class MoveCommand : ActionCommand
    {
        public bool IsTake { get; private set; }
        public Cell.Slot.Relation Direction { get; private set; }

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
            : base(catalyst_slot, command_codon_index)
        {
            Direction = direction;

            IsTake = quantity > 0;

            //Grabbing if this is a "Take"
            GrabCommand grab_command = IsTake ? new GrabCommand(catalyst_slot, command_codon_index) : null;

            //Pushing Compounds out of the way
            PushAction push_action = new PushAction(catalyst_slot, catalyst_slot, direction);

            //Dragging grabbed compound behind us
            MoveToSlotAction move_action = null;
            Cell.Slot grab_slot = catalyst_slot.GetAdjacentSlot(Catalyst.Orientation);
            if ((Catalyst.GetFacet<Interpretase>().IsGrabbing || IsTake) &&
                grab_slot != null &&
                grab_slot.Compound != null &&
                Catalyst.Orientation != direction &&
                !push_action.IsFullPush)

                move_action = new MoveToSlotAction(catalyst_slot, catalyst_slot.GetAdjacentSlot(Catalyst.Orientation), catalyst_slot, quantity);

            SetAction(new CompositeAction(catalyst_slot, grab_command, push_action, move_action));
        }

        public override void End()
        {
            if (Direction != Catalyst.Orientation)
                Catalyst.Orientation = CatalystSlot.GetAdjacentSlot(Direction).GetRelation(CatalystSlot);

            base.End();
        }
    }

    public class SpinCommand : ActionCommand
    {
        //Right : Clockwise
        //Left : Counter clockwise
        public enum Direction { Right, Left }

        Direction direction;

        bool IsRightSpin { get { return direction == Direction.Right; } }

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
            : base(catalyst_slot, command_codon_index)
        {
            direction = direction_;

            Cell.Slot grab_slot = catalyst_slot.GetAdjacentSlot(Catalyst.Orientation);
            if (Catalyst.GetFacet<Interpretase>().IsGrabbing)
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
        }

        public override void End()
        {
            base.End();

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
}
