﻿using System.Collections.Generic;
using UnityEngine;


//This needs an overhaul to reduce coupling with Action, or at least manage it better
public class ActionComponent : MonoBehaviour
{
    Action action;

    float time_elapsed = 0;
    float length = 1;

    OrganismComponent OrganismComponent
    {
        get { return GetComponent<OrganismComponent>(); }
    }

    CellComponent CellComponent
    {
        get { return OrganismComponent.GetCellComponent(action.Cell); }
    }

    SlotComponent SlotComponent
    {
        get { return OrganismComponent.GetSlotComponent(action.CatalystSlot); }
    }

    public void SetAction(Action action_, float length_)
    {
        action = action_;
        length = length_;

        action.Begin();
        if (!action.HasBegun)
            return;

        Queue<Action> actions = new Queue<Action>();
        actions.Enqueue(action);

        List<Transform> compound_positions = Utility.CreateList(SlotComponent.CompoundComponent.transform,
                                                              SlotComponent.LeftCorner,
                                                              SlotComponent.RightCorner,
                                                              SlotComponent.BottomCorner);
        Queue<Transform> incoming_compound_positions = new Queue<Transform>(compound_positions),
                         outgoing_compound_positions = new Queue<Transform>(compound_positions);

        Dictionary<Compound, float> compound_rotations = new Dictionary<Compound, float>();
        bool is_grabbing = false;
        bool is_releasing = false;

        Interpretase interpretase = action.Catalyst.GetFacet<Interpretase>();

        while (actions.Count > 0)
        {
            Action action = actions.Dequeue();
            if (action is CompositeAction)
                foreach (Action component_action in (action as CompositeAction).Actions)
                    actions.Enqueue(component_action);
            else if (action is Interpretase.ActionCommand)
                actions.Enqueue((action as Interpretase.ActionCommand).Action);

            if (action is ReactionAction)
            {
                ReactionAction reaction = action as ReactionAction;

                foreach (Cell.Slot reactant_slot in reaction.ReactantSlots)
                {
                    CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                    compound_component.SetCompound(reaction.GetReactant(reactant_slot));
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(OrganismComponent.GetSlotComponent(reactant_slot).CompoundComponent.gameObject, 
                                       CellComponent.GetSlotComponent(action.CatalystSlot).CompoundComponent.gameObject)
                        .SetLength(0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.2f * length, 0.4f * length);
                }

                foreach (Cell.Slot product_slot in reaction.ProductSlots)
                {
                    CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                    compound_component.SetCompound(reaction.GetProduct(product_slot));
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(CellComponent.GetSlotComponent(action.CatalystSlot).CompoundComponent.gameObject, OrganismComponent.GetSlotComponent(product_slot).CompoundComponent.gameObject)
                                                                .SetLength(0.5f * length, 0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(true, false)
                        .SetLength(0.25f * length, 0.75f * length);
                }

                foreach (Compound compound in reaction.GetCytosolReactants())
                {
                    CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                    compound_component.SetCompound(compound);
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.transform.localScale = new Vector3(0.75f, 0.75f);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    Transform target = incoming_compound_positions.Dequeue();

                    compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(CellComponent.gameObject, target != null ? target.gameObject : SlotComponent.CompoundComponent.gameObject)
                        .SetLength(0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.2f * length, 0.4f * length);
                }

                foreach (Compound compound in reaction.GetCytosolProducts())
                {
                    CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                    compound_component.SetCompound(compound);
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.transform.localScale = new Vector3(0.75f, 0.75f);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    Transform source = outgoing_compound_positions.Dequeue();

                    compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(source != null ? source.gameObject : SlotComponent.CompoundComponent.gameObject, CellComponent.gameObject)
                        .SetLength(0.5f * length, 0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.1f * length, 0.9f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(true, false)
                        .SetLength(0.1f * length, 0.5f * length);
                }

                foreach (Compound compound in reaction.GetLocaleReactants())
                {
                    CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                    compound_component.SetCompound(compound);
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.transform.localScale = new Vector3(0.75f, 0.75f);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    Transform target = incoming_compound_positions.Dequeue();

                    compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(SlotComponent.Outside.gameObject, target != null ? target.gameObject : SlotComponent.CompoundComponent.gameObject)
                        .SetLength(0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.2f * length, 0.4f * length);
                }

                foreach (Compound compound in reaction.GetLocaleProducts())
                {
                    CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                    compound_component.SetCompound(compound);
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.transform.localScale = new Vector3(0.75f, 0.75f);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    Transform source = outgoing_compound_positions.Dequeue();

                    compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(source != null ? source.gameObject : SlotComponent.CompoundComponent.gameObject, SlotComponent.Outside.gameObject)
                        .SetLength(0.5f * length, 0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.1f * length, 0.9f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(true, false)
                        .SetLength(0.1f * length, 0.5f * length);
                }


                SlotComponent.CompoundComponent.MoleculeComponent.gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(length);
            }

            if (action is Interpretase.LoadProgram)
            {
                Interpretase.LoadProgram load_program_action = action as Interpretase.LoadProgram;

                CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                compound_component.SetCompound(load_program_action.ProgramCompound);
                compound_component.transform.SetParent(CellComponent.transform);
                compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                Cell.Slot program_slot = load_program_action.ProgramSlot;
                SlotComponent program_slot_component = CellComponent.OrganismComponent.GetCellComponent(program_slot.Cell).GetSlotComponent(program_slot);

                compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(program_slot_component.CompoundComponent.gameObject, 
                                   CellComponent.GetSlotComponent(load_program_action.CatalystSlot).CompoundComponent.gameObject)
                    .SetLength(1.0f * length);

                compound_component.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(true, false)
                    .SetLength(0.5f);
            }

            if (action is Interpretase.CopyCommand)
            {
                Interpretase.CopyCommand copy_command = action as Interpretase.CopyCommand;

                CompoundComponent used_compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                used_compound_component.SetCompound(copy_command.UsedCompound);
                used_compound_component.transform.SetParent(CellComponent.transform);
                used_compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                SlotComponent input_slot_component = CellComponent.OrganismComponent.GetSlotComponent(copy_command.Source);

                used_compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(input_slot_component.CompoundComponent.gameObject, 
                                   CellComponent.GetSlotComponent(copy_command.CatalystSlot).CompoundComponent.gameObject)
                    .SetLength(0.5f * length);

                used_compound_component.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(true, true)
                    .SetLength(0.5f * length);


                CompoundComponent copied_compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                copied_compound_component.SetCompound(copy_command.CopiedCompound);
                copied_compound_component.transform.SetParent(CellComponent.transform);
                copied_compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                SlotComponent output_slot_component = CellComponent.OrganismComponent.GetSlotComponent(copy_command.Destination);

                copied_compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(CellComponent.GetSlotComponent(copy_command.CatalystSlot).CompoundComponent.gameObject, 
                                   output_slot_component.CompoundComponent.gameObject)
                    .SetLength(0.5f * length, 0.5f * length);

                copied_compound_component.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(true, false)
                    .SetLength(0.5f * length, 0.5f * length);
            }

            if (action is PumpAction)
            {
                PumpAction pump_action = action as PumpAction;

                SlotComponent.CompoundComponent.MoleculeComponent.gameObject.AddComponent<AnimatorAnimation>()
                    .SetParameters(3)
                    .SetLength(length);


                CompoundComponent source_compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                source_compound_component.SetCompound(pump_action.PumpedCompound);
                source_compound_component.transform.SetParent(CellComponent.transform);
                source_compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                if (pump_action.Source is Cell.Slot)
                {
                    

                    source_compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(OrganismComponent.GetSlotComponent(pump_action.Source as Cell.Slot).CompoundComponent.gameObject,
                                        CellComponent.GetSlotComponent(action.CatalystSlot).CompoundComponent.gameObject)
                        .SetLength(0.5f * length);

                    source_compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.2f * length, 0.4f * length);
                }
                else
                {
                    Transform target = incoming_compound_positions.Dequeue();

                    source_compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(SlotComponent.Outside.gameObject, target != null ? target.gameObject : SlotComponent.CompoundComponent.gameObject)
                        .SetLength(0.5f * length);

                    source_compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.2f * length, 0.4f * length);
                }


                CompoundComponent destination_compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                destination_compound_component.SetCompound(pump_action.PumpedCompound);
                destination_compound_component.transform.SetParent(CellComponent.transform);
                destination_compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                if (pump_action.Destination is Cell.Slot)
                {
                    destination_compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(CellComponent.GetSlotComponent(action.CatalystSlot).CompoundComponent.gameObject, 
                                       OrganismComponent.GetSlotComponent(pump_action.Destination as Cell.Slot).CompoundComponent.gameObject)
                        .SetLength(0.5f * length, 0.5f * length);

                    destination_compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(true, false)
                        .SetLength(0.25f * length, 0.75f * length);
                }
                else
                {
                    Transform source = outgoing_compound_positions.Dequeue();

                    destination_compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(source != null ? source.gameObject : SlotComponent.CompoundComponent.gameObject, SlotComponent.Outside.gameObject)
                        .SetLength(0.5f * length, 0.5f * length);

                    destination_compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.1f * length, 0.9f * length);

                    destination_compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(true, false)
                        .SetLength(0.1f * length, 0.5f * length);
                }
            }

            if (action is Interpretase.GrabCommand)
            {
                SlotComponent.CompoundComponent.GetComponentInChildren<GrabberComponent>().gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(0.5f * length)
                    .Smooth();

                is_grabbing = true;
            }

            if (action is Interpretase.ReleaseCommand)
            {
                SlotComponent.CompoundComponent.GetComponentInChildren<GrabberComponent>().gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(0.5f * length)
                    .Smooth()
                    .Reverse();

                is_releasing = true;
            }

            if (action is Interpretase.MoveCommand)
            {
                Interpretase.MoveCommand move_command = action as Interpretase.MoveCommand;

                float radians = MathUtility.Mod((move_command.MovingCompound.Molecule as Catalyst).Orientation - move_command.FinalOrientation, 3) * -2 * Mathf.PI / 3;

                compound_rotations[move_command.MovingCompound] = radians;
            }

            if (action is Interpretase.SpinCommand)
            {
                Interpretase.SpinCommand spin_command = action as Interpretase.SpinCommand;

                float radians = 2 * Mathf.PI / 3 * (spin_command.IsRightSpin ? 1 : -1);

                MoleculeComponent molecule_component = SlotComponent.CompoundComponent.MoleculeComponent;
                molecule_component.gameObject.AddComponent<RotationAnimation>()
                    .SetParameters(MathUtility.DegreesToRadians(molecule_component.transform.eulerAngles.z) + radians)
                    .SetLength(1.0f * length)
                    .Smooth();

                if(spin_command.Action != null)
                    foreach (Action component_action in (spin_command.Action as CompositeAction).Actions)
                    {
                        Compound compound;

                        if (component_action is MoveToSlotAction)
                            compound = (component_action as MoveToSlotAction).MovedCompound;
                        else
                            compound = (component_action as MoveToLocaleAction).MovedCompound;

                        compound_rotations[compound] = radians;
                    }
            }

            if (action is MoveToSlotAction ||
                action is MoveToCytosolAction ||
                action is MoveToLocaleAction)
            {
                CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                compound_component.transform.SetParent(CellComponent.transform);
                compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                GameObject source_game_object = null,
                           destination_game_object = null;

                Cell.Slot source_slot = null;

                float final_rotation = 0;
                float destination_rotation = 0;

                if (action is MoveToSlotAction)
                {
                    MoveToSlotAction move_to_slot_action = action as MoveToSlotAction;
                    Compound compound = move_to_slot_action.MovedCompound;

                    SlotComponent destination_slot_component = OrganismComponent.GetSlotComponent(move_to_slot_action.Destination);

                    compound_component.transform.rotation = OrganismComponent.GetSlotComponent(move_to_slot_action.Source)
                                                            .CompoundComponent.transform.rotation;

                    compound_component.SetCompound(move_to_slot_action.MovedCompound);

                    source_slot = move_to_slot_action.Source;
                    destination_game_object = destination_slot_component.CompoundComponent.gameObject;

                    destination_rotation = MathUtility.DegreesToRadians(destination_slot_component.transform.rotation.eulerAngles.z);
                    final_rotation = destination_rotation;

                    if (move_to_slot_action.MovedCompound.Molecule is Catalyst)
                        switch ((compound.Molecule as Catalyst).Orientation)
                        {
                            case Cell.Slot.Relation.Right: final_rotation += -2 * Mathf.PI / 3; break;
                            case Cell.Slot.Relation.Left: final_rotation += 2 * Mathf.PI / 3; break;
                        }
                }
                else if (action is MoveToCytosolAction)
                {
                    MoveToCytosolAction move_to_cytosol_action = action as MoveToCytosolAction;

                    compound_component.SetCompound(move_to_cytosol_action.MovedCompound);

                    source_slot = move_to_cytosol_action.Source;
                    destination_game_object = OrganismComponent.gameObject;
                }
                else if (action is MoveToLocaleAction)
                {
                    MoveToLocaleAction move_to_locale_action = action as MoveToLocaleAction;

                    compound_component.SetCompound(move_to_locale_action.MovedCompound);

                    source_slot = move_to_locale_action.Source;
                    destination_game_object = OrganismComponent.GetSlotComponent(source_slot).Outside.gameObject;
                }

                source_game_object = OrganismComponent.GetSlotComponent(source_slot).CompoundComponent.gameObject;

                float relative_move_length = is_grabbing ? 0.7f : 1.0f;
                float relative_move_delay = is_grabbing ? 0.3f : 0.0f;

                compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(source_game_object, destination_game_object)
                    .SetLength(relative_move_length * length, relative_move_delay * length)
                    .Smooth();

                if (compound_rotations.ContainsKey(compound_component.Compound))
                    final_rotation += compound_rotations[compound_component.Compound];
                compound_component.MoleculeComponent.gameObject.AddComponent<RotationAnimation>()
                    .SetParameters(final_rotation)
                    .SetLength(1.0f * length)
                    .Smooth();

                if(is_grabbing && compound_component.Compound.Molecule.Equals(action.Catalyst))
                    compound_component.GetComponentInChildren<GrabberComponent>().gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(0.2f * length)
                    .Smooth();

                if(compound_component.Compound.Molecule == action.Catalyst && interpretase != null)
                    compound_component.MoleculeComponent.gameObject.AddComponent<AnimatorAnimation>()
                        .SetLength(length);
            }

            if (action is Constructase.ConstructCell)
            {
                Animator construction_animator = Instantiate(Scene.Micro.Prefabs.ConstructionAnimator);
                construction_animator.transform.SetParent(SlotComponent.transform);
                construction_animator.transform.localRotation = Quaternion.Euler(0, 0, 60);
                construction_animator.transform.localPosition = new Vector3(0, 4.2f, 0);

                construction_animator.gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(1.0f * length)
                    .Smooth();

                construction_animator.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(false, true)
                    .SetLength(0.1f * length, 0.9f * length);

                SlotComponent.CompoundComponent.MoleculeComponent.gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(length);
            }

            if (action is Separatase.SeparateCell)
            {
                SlotComponent.CompoundComponent.GetComponentInChildren<SeparatorComponent>().gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(0.25f * length);


                Cell loyal_cell = CellComponent.Cell;
                Cell rebel_cell = SlotComponent.Slot.AdjacentCell;

                CellComponent rebel_cell_component = OrganismComponent.GetCellComponent(rebel_cell);

                List<Cell> rebel_cells = OrganismComponent.Organism.GetRebelCells(loyal_cell, rebel_cell);
                foreach (Cell cell in rebel_cells)
                {
                    CellComponent cell_component = OrganismComponent.GetCellComponent(cell);

                    cell_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.2f * length, 0.25f * length);
                    cell_component.gameObject.AddComponent<ScalingAnimation>()
                        .SetParameters(true)
                        .SetLength(0.2f * length, 0.25f * length);
                    cell_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(rebel_cell_component.gameObject)
                        .SetLength(0.2f * length, 0.25f * length);
                }


                Animator spore_animator = Instantiate(Scene.Micro.Prefabs.SporeAnimator);
                spore_animator.transform.SetParent(Scene.Micro.Visualization.transform);

                spore_animator.gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(0.4f * length, 0.25f * length)
                    .Smooth();

                Vector3 center = Vector3.zero;
                List<Cell> loyal_cells = OrganismComponent.Organism.GetLoyalCells(loyal_cell, rebel_cell);
                foreach (Cell cell in loyal_cells)
                    center += OrganismComponent.GetCellComponent(cell).transform.position / loyal_cells.Count;

                spore_animator.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(rebel_cell_component.transform.position, 
                                   center + (rebel_cell_component.transform.position - center).normalized * 20)
                    .SetLength(0.55f * length, 0.45f * length);

                spore_animator.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(false, true)
                    .SetLength(0.55f * length, 0.45f * length);
            }
        }


        CatalystProgressIcon catalyst_progress_icon = SlotComponent.CompoundComponent.MoleculeComponent.CatalystProgressIcon;
        if (action is ProgressiveCatalyst.WorkAction || (catalyst_progress_icon.Moment > 0 && catalyst_progress_icon.Moment < 1))
        {
            ProgressiveCatalyst catalyst = action.Catalyst.GetFacet<ProgressiveCatalyst>();
            if (catalyst != null && !(catalyst is InstantCatalyst))
            {
                float start_progress = catalyst_progress_icon.Moment;
                if (start_progress >= 1)
                    start_progress = 0;

                float end_progress = 1;
                if (catalyst.PreviewAction != null)
                    end_progress = catalyst.NormalizedProgress;

                catalyst_progress_icon.gameObject.AddComponent<CatalystProgressAnimation>()
                    .SetParameters(start_progress, end_progress);
            }
        }

        if (action is ProgressiveCatalyst.WorkAction)
        {
            if (action.Catalyst.GetFacet<Separatase>() != null)
                SlotComponent.CompoundComponent.GetComponentInChildren<SeparatorComponent>().gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(length);
        }

        if (interpretase != null)
            SlotComponent.CompoundComponent.MoleculeComponent.gameObject.AddComponent<AnimatorAnimation>()
                .SetLength(length);
    }

    float GetMoment()
    {
        return time_elapsed / length;
    }

    private void Update()
    {
        time_elapsed += Time.deltaTime * Scene.Micro.Visualization.Speed;

        if (GetMoment() > 1)
        {
            if (action.HasBegun)
                action.End();

            GameObject.Destroy(this);
        }
    }
}


public class ActionAnimation : MonoBehaviour
{
    float length = 1;
    float elapsed_time = 0;
    float delay = 0;

    bool is_smooth = false;
    float smooth_moment = 0;
    float velocity = 0;

    bool reverse = false;

    protected virtual void Update()
    {
        elapsed_time += Time.deltaTime * Scene.Micro.Visualization.Speed;

        if (is_smooth && elapsed_time >= delay)
            //This function is GARBAGE, Unity!
            smooth_moment = Mathf.SmoothDamp(smooth_moment, 1, ref velocity, 0.5f * length / Scene.Micro.Visualization.Speed);

        if ((elapsed_time - delay) >= length)
            Destroy(this);
    }

    protected float GetMoment()
    {
        float moment = Mathf.Max((elapsed_time - delay), 0) / length;

        if (is_smooth && moment < 1)
            moment = Mathf.Min(smooth_moment / 0.915f, 1);

        return reverse ? 1 - moment : moment;
    }

    public ActionAnimation SetLength(float length_, float delay_ = 0)
    {
        length = length_;
        elapsed_time = 0;
        delay = delay_;

        return this;
    }

    public ActionAnimation Smooth()
    {
        if (elapsed_time > 0)
            throw new System.NotSupportedException();

        is_smooth = true;

        return this;
    }

    public ActionAnimation Reverse()
    {
        reverse = !reverse;

        return this;
    }

    public class GarbageCollector : MonoBehaviour
    {
        private void Update()
        {
            if (GetComponents<ActionAnimation>().Length == 0)
                Destroy(gameObject);
        }
    }
}

public class TransformAnimation : ActionAnimation
{

}

//switch to Transforms instead of GameObjects
public class MoveAnimation : ActionAnimation
{
    GameObject source, target;
    Vector3 fixed_source_position = Vector3.zero, 
            fixed_target_position = Vector3.zero;

    protected override void Update()
    {
        base.Update();

        transform.position = Vector2.Lerp(source == null ? fixed_source_position : source.transform.position, 
                                          target == null ? fixed_target_position : target.transform.position, 
                                          GetMoment());
    }

    public MoveAnimation SetParameters(GameObject source_, GameObject target_)
    {
        source = source_;
        target = target_;

        transform.position = source.transform.position;

        return this;
    }

    public MoveAnimation SetParameters(GameObject target_)
    {
        fixed_source_position = transform.position;
        target = target_;

        return this;
    }

    public MoveAnimation SetParameters(GameObject source_, Vector3 target_position)
    {
        source = source_;
        fixed_target_position = target_position;

        return this;
    }

    public MoveAnimation SetParameters(Vector3 target_position)
    {
        fixed_source_position = transform.position;
        fixed_target_position = target_position;

        return this;
    }

    public MoveAnimation SetParameters(Vector3 source_position, Vector3 target_position)
    {
        fixed_source_position = source_position;
        fixed_target_position = target_position;

        return this;
    }
}

public class FadeAnimation : ActionAnimation
{
    bool fade_in, fade_out;

    float max_alpha = 1;

    float Alpha
    {
        set
        {
            foreach (SpriteRenderer sprite_renderer in GetComponentsInChildren<SpriteRenderer>())
                sprite_renderer.color = new Color(sprite_renderer.color.r, 
                                                  sprite_renderer.color.g, 
                                                  sprite_renderer.color.b, 
                                                  value * max_alpha);

            foreach (FadeAnimation fade_animation in GetComponentsInChildren<FadeAnimation>())
                if(fade_animation != this)
                    fade_animation.max_alpha = value * max_alpha;

        }
    }

    bool IsFadingIn { get { return fade_in && (fade_out ? GetMoment() < 0.5f : true) && FadeInMoment > 0; } }
    float FadeInMoment { get { return GetMoment() / (fade_out ? 0.5f : 1); } }

    bool IsFadingOut { get { return fade_out && (fade_in ? GetMoment() > 0.5f : true) && FadeOutMoment > 0; } }
    float FadeOutMoment { get { return fade_in ? (GetMoment() - 0.5f) / 0.5f : GetMoment(); } }

    protected override void Update()
    {
        base.Update();

        if (IsFadingIn)
            Alpha = Mathf.Lerp(0, 1, FadeInMoment);
        else if (IsFadingOut)
            Alpha = Mathf.Lerp(1, 0, FadeOutMoment);

    }

    public FadeAnimation SetParameters(bool fade_in_, bool fade_out_)
    {
        fade_in = fade_in_;
        fade_out = fade_out_;

        if (fade_in)
            Alpha = 0;
        else if (fade_out)
            Alpha = 1;

        return this;
    }
}

public class ScalingAnimation : ActionAnimation
{
    GameObject game_object;
    bool shrink;

    protected override void Update()
    {
        base.Update();

        float scale;

        if (shrink)
            scale = Mathf.Lerp(1, 0, GetMoment());
        else
            scale = Mathf.Lerp(0, 1, GetMoment());

        transform.localScale = new Vector3(scale, scale, scale);
    }

    public ScalingAnimation SetParameters(bool shrink_)
    {
        shrink = shrink_;

        return this;
    }
}

public class RotationAnimation : ActionAnimation
{
    float start_radians = 0;
    float end_radians = 0;

    protected override void Update()
    {
        base.Update();

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(MathUtility.RadiansToDegrees(start_radians), 
                                                               MathUtility.RadiansToDegrees(end_radians), 
                                                               GetMoment()));
    }

    public RotationAnimation SetParameters(float radians_)
    {
        start_radians = MathUtility.DegreesToRadians(transform.rotation.eulerAngles.z);
        end_radians = radians_;

        float distance = Mathf.Abs(end_radians - start_radians);

        if (Mathf.Abs(end_radians + 2 * Mathf.PI - start_radians) + 0.001f < distance)
            end_radians += 2 * Mathf.PI;
        else if (Mathf.Abs(end_radians - 2 * Mathf.PI - start_radians) + 0.001f < distance)
            end_radians -= 2 * Mathf.PI;

        return this;
    }
}

public class AnimatorAnimation : ActionAnimation
{
    Animator animator;

    int cycles = 1;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        animator.SetFloat("moment", GetMoment() * cycles);
    }

    public AnimatorAnimation SetParameters(int cycles_)
    {
        cycles = cycles_;

        return this;
    }
}

public class CytosolAnimation : ActionAnimation
{

}

public class CatalystProgressAnimation : ActionAnimation
{
    CatalystProgressIcon catalyst_progress_icon;

    float start_progress, end_progress;

    private void Start()
    {
        catalyst_progress_icon = GetComponentInChildren<CatalystProgressIcon>();
    }

    protected override void Update()
    {
        base.Update();

        catalyst_progress_icon.Moment = Mathf.Lerp(start_progress, end_progress, GetMoment());
    }

    public CatalystProgressAnimation SetParameters(float start_progress_, float end_progress_)
    {
        start_progress = start_progress_;
        end_progress = end_progress_;

        return this;
    }
}
