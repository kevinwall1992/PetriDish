﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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
                gameObject.AddComponent<RotationAnimation>().SetParameters(CellComponent, 1).SetLength(length);

            if (action is ReactionAction)
            {
                ReactionAction reaction = action as ReactionAction;

                List<Cell.Slot> reactant_slots = reaction.GetReactantSlots();
                foreach (Cell.Slot reactant_slot in reactant_slots)
                {
                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(reaction.GetReactant(reactant_slot));
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(CellComponent.GetSlotComponent(reactant_slot).CompoundComponent.gameObject, 
                                       CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject)
                        .SetLength(0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.2f * length, 0.4f * length);
                }

                List<Cell.Slot> product_slots = reaction.GetProductSlots();
                foreach (Cell.Slot product_slot in product_slots)
                {
                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(reaction.GetProduct(product_slot));
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject, CellComponent.GetSlotComponent(product_slot).CompoundComponent.gameObject)
                                                                .SetLength(0.5f * length, 0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(true, false)
                        .SetLength(0.25f * length, 0.75f * length);
                }

                foreach (Compound compound in reaction.GetCytozolReactants())
                {
                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(compound);
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(CellComponent.gameObject, CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject)
                        .SetLength(0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.2f * length, 0.4f * length);
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
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(source, CellComponent.gameObject)
                        .SetLength(0.5f * length, 0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.1f * length, 0.9f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(true, false)
                        .SetLength(0.1f * length, 0.5f * length);
                }
            }

            if (action is Interpretase.ActivateCommand)
            {
                Interpretase.ActivateCommand activate_command = action as Interpretase.ActivateCommand;

                CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.SetCompound(activate_command.OutputtedCompound);
                compound_component.transform.SetParent(CellComponent.transform);
                compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                SlotComponent output_slot_component = CellComponent.OrganismComponent.GetCellComponent(activate_command.OutputSlot.Cell).GetSlotComponent(activate_command.OutputSlot);

                compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(CellComponent.gameObject, output_slot_component.CompoundComponent.gameObject)
                    .SetLength(1.0f * length);

                compound_component.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(true, false)
                    .SetLength(0.5f);
            }

            if (action is Interpretase.CutCommand)
            {
                Interpretase.CutCommand cut_command = action as Interpretase.CutCommand;

                CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.SetCompound(new Compound(new DNA(), 1));
                compound_component.transform.SetParent(CellComponent.transform);
                compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                SlotComponent output_slot_component = CellComponent.OrganismComponent.GetCellComponent(cut_command.OutputSlot.Cell).GetSlotComponent(cut_command.OutputSlot);

                compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(CellComponent.GetSlotComponent(cut_command.Slot).CompoundComponent.gameObject, 
                                   output_slot_component.CompoundComponent.gameObject)
                    .SetLength(1.0f * length);

                compound_component.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(true, false)
                    .SetLength(0.5f);
            }

            if (action is MoveToSlotAction ||
                action is MoveToCytozolAction ||
                action is MoveToLocaleAction)
            {
                CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.transform.SetParent(CellComponent.transform);
                compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

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

                compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(source_game_object, destination_game_object)
                    .SetLength(1.0f * length);
            }

            if (action is Interpretase.SwapCommand)
            {
                Interpretase.SwapCommand swap_command = action as Interpretase.SwapCommand;

                CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.SetCompound(swap_command.CompoundA);
                compound_component.transform.SetParent(CellComponent.transform);
                compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(CellComponent.GetSlotComponent(swap_command.SlotA).CompoundComponent.gameObject, 
                                   CellComponent.GetSlotComponent(swap_command.SlotB).CompoundComponent.gameObject)
                    .SetLength(1.0f * length);

                compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.SetCompound(swap_command.CompoundB);

                compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(CellComponent.GetSlotComponent(swap_command.SlotB).CompoundComponent.gameObject, 
                                   CellComponent.GetSlotComponent(swap_command.SlotA).CompoundComponent.gameObject)
                    .SetLength(1.0f * length);
            }

            if (action is Sporulase.SporulateAction)
            {
                Animator spore = Instantiate(Scene.Micro.Prefabs.Spore);
                spore.transform.SetParent(Scene.Micro.Visualization.transform);

                spore.gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(0.2f * length)
                    .Smooth();

                CellComponent.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(false, true)
                    .SetLength(0.2f * length);
                CellComponent.gameObject.AddComponent<ScalingAnimation>()
                    .SetParameters(true)
                    .SetLength(0.2f * length);

                spore.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(CellComponent.gameObject, OrganismComponent.North)
                    .SetLength(25, 0.3f * length);

                spore.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(false, true)
                    .SetLength(0.7f * length, 0.3f * length);
            }
        }
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

    bool is_smooth = false;
    float smooth_moment = 0;
    float velocity;

    protected virtual void Update()
    {
        elapsed_time += Time.deltaTime * Scene.Micro.Visualization.Speed;

        if (GetMoment() >= 1)
            Destroy(this);
    }

    protected float GetMoment()
    {
        if (!is_smooth)
            return Mathf.Max((elapsed_time - delay), 0) / length;
        else
            return smooth_moment = Mathf.SmoothDamp(smooth_moment, 1, ref velocity, length);
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

    public class GarbageCollector : MonoBehaviour
    {
        private void Update()
        {
            if (GetComponents<ActionAnimation>().Length == 0)
                Destroy(gameObject);
        }
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

        if (source == null || target == null)
            return;

        transform.position = Vector2.Lerp(source.transform.position, target.transform.position, GetMoment());
    }

    public MoveAnimation SetParameters(GameObject source_, GameObject target_)
    {
        source = source_;
        target = target_;

        transform.position = source.transform.position;

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

public class AnimatorAnimation : ActionAnimation
{
    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        animator.SetFloat("moment", GetMoment());
    }
}

public class CytozolAnimation : ActionAnimation
{

}
