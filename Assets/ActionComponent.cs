using UnityEngine;
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

    SlotComponent SlotComponent
    {
        get { return OrganismComponent.GetSlotComponent(action.CatalystSlot); }
    }

    public void SetAction(Action action_, float length_)
    {
        action = action_;
        length = length_;

        if (!action.Prepare())
            return;

        action.Begin();

        Queue<Action> actions = new Queue<Action>();
        actions.Enqueue(action);

        List<Transform> compound_positions = Utility.CreateList(SlotComponent.CompoundComponent.transform,
                                                              SlotComponent.LeftCorner,
                                                              SlotComponent.RightCorner,
                                                              SlotComponent.BottomCorner);
        Queue<Transform> incoming_compound_positions = new Queue<Transform>(compound_positions),
                         outgoing_compound_positions = new Queue<Transform>(compound_positions);

        while (actions.Count > 0)
        {
            Action action = actions.Dequeue();
            if (action is CompositeAction)
                foreach (Action component_action in (action as CompositeAction).Actions)
                    actions.Enqueue(component_action);
            else if (action is Interpretase.ActionCommand)
                actions.Enqueue((action as Interpretase.ActionCommand).Action);

            if (action is Rotase.RotateAction)
                gameObject.AddComponent<RotationAnimation>()
                    .SetParameters(CellComponent, 1)
                    .SetLength(length)
                    .Smooth();

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
                        .SetParameters(CellComponent.GetSlotComponent(reactant_slot).CompoundComponent.gameObject, 
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
                        .SetParameters(CellComponent.GetSlotComponent(action.CatalystSlot).CompoundComponent.gameObject, CellComponent.GetSlotComponent(product_slot).CompoundComponent.gameObject)
                                                                .SetLength(0.5f * length, 0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(true, false)
                        .SetLength(0.25f * length, 0.75f * length);
                }

                foreach (Compound compound in reaction.GetCytozolReactants())
                {
                    CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                    compound_component.SetCompound(compound);
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.transform.localScale = new Vector3(0.75f, 0.75f);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    Transform target = outgoing_compound_positions.Dequeue();

                    compound_component.gameObject.AddComponent<MoveAnimation>()
                        .SetParameters(CellComponent.gameObject, target != null ? target.gameObject : SlotComponent.CompoundComponent.gameObject)
                        .SetLength(0.5f * length);

                    compound_component.gameObject.AddComponent<FadeAnimation>()
                        .SetParameters(false, true)
                        .SetLength(0.2f * length, 0.4f * length);
                }

                foreach (Compound compound in reaction.GetCytozolProducts())
                {
                    CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                    compound_component.SetCompound(compound);
                    compound_component.transform.SetParent(CellComponent.transform);
                    compound_component.transform.localScale = new Vector3(0.75f, 0.75f);
                    compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                    Transform source = incoming_compound_positions.Dequeue();

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
            }

            if (action is Interpretase.ActivateCommand)
            {
                Interpretase.ActivateCommand activate_command = action as Interpretase.ActivateCommand;

                CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
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

                CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                compound_component.SetCompound(new Compound(new DNA(), 1));
                compound_component.transform.SetParent(CellComponent.transform);
                compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                SlotComponent output_slot_component = CellComponent.OrganismComponent.GetCellComponent(cut_command.OutputSlot.Cell).GetSlotComponent(cut_command.OutputSlot);

                compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(CellComponent.GetSlotComponent(cut_command.CatalystSlot).CompoundComponent.gameObject, 
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
                CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
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
                    MoveToCytozolAction move_to_cytosol_action = action as MoveToCytozolAction;

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

                compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(source_game_object, destination_game_object)
                    .SetLength(1.0f * length);

            }

            if (action is Interpretase.SwapCommand)
            {
                Interpretase.SwapCommand swap_command = action as Interpretase.SwapCommand;

                CompoundComponent compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                compound_component.SetCompound(swap_command.CompoundA);
                compound_component.transform.SetParent(CellComponent.transform);
                compound_component.gameObject.AddComponent<ActionAnimation.GarbageCollector>();

                compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(CellComponent.GetSlotComponent(swap_command.SlotA).CompoundComponent.gameObject, 
                                   CellComponent.GetSlotComponent(swap_command.SlotB).CompoundComponent.gameObject)
                    .SetLength(1.0f * length);

                compound_component = Instantiate(Scene.Micro.Prefabs.CompoundComponent);
                compound_component.SetCompound(swap_command.CompoundB);

                compound_component.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(CellComponent.GetSlotComponent(swap_command.SlotB).CompoundComponent.gameObject, 
                                   CellComponent.GetSlotComponent(swap_command.SlotA).CompoundComponent.gameObject)
                    .SetLength(1.0f * length);
            }

            if (action is Sporulase.SporulateAction)
            {
                Animator spore_animator = Instantiate(Scene.Micro.Prefabs.SporeAnimator);
                spore_animator.transform.SetParent(Scene.Micro.Visualization.transform);

                spore_animator.gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(0.2f * length)
                    .Smooth();

                CellComponent.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(false, true)
                    .SetLength(0.2f * length);
                CellComponent.gameObject.AddComponent<ScalingAnimation>()
                    .SetParameters(true)
                    .SetLength(0.2f * length);

                spore_animator.gameObject.AddComponent<MoveAnimation>()
                    .SetParameters(CellComponent.gameObject, OrganismComponent.North)
                    .SetLength(25, 0.3f * length);

                spore_animator.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(false, true)
                    .SetLength(0.7f * length, 0.3f * length);
            }

            if (action is Constructase.ConstructCell)
            {
                Transform adjustment = new GameObject("adjustment").GetComponent<Transform>();
                adjustment.SetParent(Scene.Micro.Visualization.transform);

                Animator construction_animator = Instantiate(Scene.Micro.Prefabs.ConstructionAnimator);
                construction_animator.transform.SetParent(adjustment);
                construction_animator.transform.Translate(-0.150f, -0.04f, 0);

                Organism.HexagonalDirection direction = SlotComponent.Slot.Direction;
                adjustment.Rotate(0, 0, (((int)direction + 5) % 6) * -60);

                Vector2Int cell_position = OrganismComponent.Organism.GetNeighborPosition(CellComponent.Cell, direction);
                adjustment.position = OrganismComponent.CellPositionToWorldPosition(cell_position);

                construction_animator.gameObject.AddComponent<AnimatorAnimation>()
                    .SetLength(1.0f * length)
                    .Smooth();

                construction_animator.gameObject.AddComponent<FadeAnimation>()
                    .SetParameters(false, true)
                    .SetLength(0.1f * length, 0.9f * length);
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
        float linear_moment = Mathf.Max((elapsed_time - delay), 0) / length;

        if (!is_smooth)
            return linear_moment;
        else
        {
            if (elapsed_time >= delay)
                smooth_moment = Mathf.SmoothDamp(smooth_moment, 1, ref velocity, length);

            if (linear_moment >= 1)
                return linear_moment;
            else
                return Mathf.Min(smooth_moment / 0.975f, 1);
        }
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

    float starting_rotation;

    protected override void Update()
    {
        base.Update();

        if (cell_component == null)
            return;

        if (GetMoment() < 1)
            cell_component.transform.localEulerAngles = 
                new Vector3(0, 0, starting_rotation + rotation_count * -60 * GetMoment());
    }

    public RotationAnimation SetParameters(CellComponent cell_component_, int rotation_count_)
    {
        cell_component = cell_component_;
        rotation_count = rotation_count_;

        starting_rotation = cell_component.transform.eulerAngles.z;

        return this;
    }
}

public class TransformAnimation : ActionAnimation
{

}

//switch to Transforms instead of GameObjects
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
