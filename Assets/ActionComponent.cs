﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//This needs an overhaul to reduce coupling with Action, or at least manage it better
public class ActionComponent : MonoBehaviour
{
    Action action;

    List<ActionAnimation> animations = new List<ActionAnimation>();

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

    public void SetAction(Action action_, float length_)
    {
        action = action_;
        length = length_;

        action.Prepare();
        if (action.HasFailed)
            return;
        action.Begin();

        Queue<Action> actions = new Queue<Action>();
        actions.Enqueue(action);

        while (actions.Count > 0)
        {
            Action action = actions.Dequeue();
            if (action is CompositeAction)
                foreach (Action component_action in (action as CompositeAction).Actions)
                    actions.Enqueue(component_action);
            else if (action is Interpretase.ActionCommand)
                actions.Enqueue((action as Interpretase.ActionCommand).Action);

            if (action is Rotase.RotateAction)
                animations.Add(gameObject.AddComponent<RotationAnimation>().SetParameters(CellComponent, 1).SetLength(length));

            if (action is ReactionAction)
            {
                ReactionAction reaction = action as ReactionAction;

                List<Cell.Slot> reactant_slots = reaction.GetReactantSlots();
                foreach (Cell.Slot reactant_slot in reactant_slots)
                {
                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(reaction.GetReactant(reactant_slot));

                    animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                                .SetParameters(CellComponent.GetSlotComponent(reactant_slot).CompoundComponent.gameObject, CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject)
                                                                .SetLength(0.5f * length));

                    animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                                .SetParameters(false, true)
                                                                .SetLength(0.2f * length, 0.4f * length));
                }

                List<Cell.Slot> product_slots = reaction.GetProductSlots();
                foreach (Cell.Slot product_slot in product_slots)
                {
                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(reaction.GetProduct(product_slot));

                    animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                                .SetParameters(CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject, CellComponent.GetSlotComponent(product_slot).CompoundComponent.gameObject)
                                                                .SetLength(0.5f * length, 0.5f * length));

                    animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                                .SetParameters(true, false)
                                                                .SetLength(0.25f * length, 0.75f * length));
                }

                foreach (Compound compound in reaction.GetCytozolReactants())
                {
                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(compound);

                    animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                                .SetParameters(CellComponent.gameObject, CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject)
                                                                .SetLength(0.5f * length));

                    animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                                .SetParameters(false, true)
                                                                .SetLength(0.2f * length, 0.4f * length));
                }

                foreach (Compound compound in reaction.GetCytozolProducts())
                {
                    GameObject source = CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject;
                    if (reaction.GetCytozolProducts().Count == 2)
                    {
                        if (reaction.GetCytozolProducts().IndexOf(compound) == 0)
                            source = CellComponent.GetSlotComponent(action.Slot).LeftCorner;
                        else
                            source = CellComponent.GetSlotComponent(action.Slot).RightCorner;
                    }

                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(compound);

                    animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                                .SetParameters(source, CellComponent.gameObject)
                                                                .SetLength(0.5f * length, 0.5f * length));

                    animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                                .SetParameters(false, true)
                                                                .SetLength(0.1f * length, 0.9f * length));

                    animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                                .SetParameters(true, false)
                                                                .SetLength(0.1f * length, 0.5f * length));
                }
            }

            if (action is Interpretase.ActivateCommand)
            {
                Interpretase.ActivateCommand activate_command = action as Interpretase.ActivateCommand;

                CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.SetCompound(activate_command.OutputtedCompound);

                SlotComponent output_slot_component = CellComponent.OrganismComponent.GetCellComponent(activate_command.OutputSlot.Cell).GetSlotComponent(activate_command.OutputSlot);

                animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                            .SetParameters(CellComponent.gameObject, output_slot_component.CompoundComponent.gameObject)
                                                            .SetLength(1.0f * length));

                animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                            .SetParameters(true, false)
                                                            .SetLength(0.5f));
            }

            if (action is Interpretase.CutCommand)
            {
                Interpretase.CutCommand cut_command = action as Interpretase.CutCommand;

                CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.SetCompound(new Compound(new DNA(), 1));

                SlotComponent output_slot_component = CellComponent.OrganismComponent.GetCellComponent(cut_command.OutputSlot.Cell).GetSlotComponent(cut_command.OutputSlot);

                animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                            .SetParameters(CellComponent.GetSlotComponent(cut_command.Slot).CompoundComponent.gameObject, output_slot_component.CompoundComponent.gameObject)
                                                            .SetLength(1.0f * length));

                animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                            .SetParameters(true, false)
                                                            .SetLength(0.5f));
            }

            if (action is MoveToSlotAction || 
                action is MoveToCytozolAction ||
                action is MoveToLocaleAction)
            {
                CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                GameObject source_game_object = null, 
                           destination_game_object = null;

                Cell.Slot source_slot = null;

                if (action is MoveToSlotAction)
                {
                    MoveToSlotAction move_to_slot_action = action as MoveToSlotAction;

                    compound_component.SetCompound(move_to_slot_action.MovedCompound);

                    source_slot = move_to_slot_action.Source;
                    destination_game_object = OrganismComponent.GetSlotComponent(move_to_slot_action.Destination).CompoundComponent.gameObject;
                }
                else if (action is MoveToCytozolAction)
                {
                    MoveToCytozolAction move_to_cytozol_action = action as MoveToCytozolAction;

                    compound_component.SetCompound(move_to_cytozol_action.MovedCompound);

                    source_slot = move_to_cytozol_action.Source;
                    destination_game_object = OrganismComponent.gameObject;
                }
                else if (action is MoveToLocaleAction)
                {
                    MoveToLocaleAction move_to_locale_action = action as MoveToLocaleAction;

                    compound_component.SetCompound(move_to_locale_action.MovedCompound);

                    source_slot = move_to_locale_action.Source;
                    destination_game_object = OrganismComponent.GetSlotComponent(source_slot).Outside;
                }

                source_game_object = OrganismComponent.GetSlotComponent(source_slot).CompoundComponent.gameObject;

                animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                            .SetParameters(source_game_object, destination_game_object)
                                                            .SetLength(1.0f * length));
            }

            if (action is Interpretase.SwapCommand)
            {
                Interpretase.SwapCommand swap_command = action as Interpretase.SwapCommand;

                CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.SetCompound(swap_command.CompoundA);

                animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                            .SetParameters(CellComponent.GetSlotComponent(swap_command.SlotA).CompoundComponent.gameObject, CellComponent.GetSlotComponent(swap_command.SlotB).CompoundComponent.gameObject)
                                                            .SetLength(1.0f * length));

                compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.SetCompound(swap_command.CompoundB);

                animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                            .SetParameters(CellComponent.GetSlotComponent(swap_command.SlotB).CompoundComponent.gameObject, CellComponent.GetSlotComponent(swap_command.SlotA).CompoundComponent.gameObject)
                                                            .SetLength(1.0f * length));
            }
        }

        foreach (ActionAnimation animation in animations)
            animation.transform.parent = transform;
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
            foreach (ActionAnimation animation in animations)
                GameObject.Destroy(animation);

            if (!action.HasFailed)
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

    protected virtual void Update()
    {
        elapsed_time += Time.deltaTime * Scene.Micro.Visualization.Speed;
    }

    protected float GetMoment()
    {
        return Mathf.Max((elapsed_time - delay), 0) / length;
    }

    public ActionAnimation SetLength(float length_, float delay_ = 0)
    {
        length = length_;
        elapsed_time = 0;
        delay = delay_;

        return this;
    }
}

public class RotationAnimation : ActionAnimation
{
    CellComponent cell_component;
    int rotation_count;

    protected override void Update()
    {
        base.Update();

        if (cell_component == null)
            return;

        cell_component.transform.rotation = Quaternion.identity;
        if (GetMoment() < 1)
            cell_component.transform.Rotate(new Vector3(0, 0, rotation_count * -60 * GetMoment()));
    }

    public RotationAnimation SetParameters(CellComponent cell_component_, int rotation_count_)
    {
        cell_component = cell_component_;
        rotation_count = rotation_count_;

        return this;
    }

    private void OnDestroy()
    {

    }
}

public class TransformAnimation : ActionAnimation
{

}

public class MoveAnimation : ActionAnimation
{
    GameObject source, target;

    protected override void Update()
    {
        base.Update();

        transform.position = Vector2.Lerp(source.transform.position, target.transform.position, GetMoment());
    }

    public MoveAnimation SetParameters(GameObject source_, GameObject target_)
    {
        source = source_;
        target = target_;

        transform.position = source.transform.position;

        return this;
    }

    private void OnDestroy()
    {
        if (gameObject != null)
            GameObject.Destroy(gameObject);
    }
}

public class FadeAnimation : ActionAnimation
{
    bool fade_in, fade_out;

    protected override void Update()
    {
        base.Update();

        if (GetMoment() < 0.5f && fade_in)
            GetComponent<SpriteRenderer>().color = Color.Lerp(Color.clear, Color.white, GetMoment() / 0.5f);
        else if (fade_out)
            GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.clear, (GetMoment() - 0.5f) / 0.5f);

    }

    public FadeAnimation SetParameters(bool fade_in_, bool fade_out_)
    {
        fade_in = fade_in_;
        fade_out = fade_out_;

        if (fade_in)
            GetComponent<SpriteRenderer>().color = Color.clear;
        else if (fade_out)
            GetComponent<SpriteRenderer>().color = Color.white;

        return this;
    }

    private void OnDestroy()
    {
        if (gameObject != null)
            GameObject.Destroy(gameObject);
    }
}

public class CytozolAnimation : ActionAnimation
{

}
