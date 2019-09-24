using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class ChargeableMolecule : Molecule
{
    Molecule molecule;

    public override float Enthalpy
    {
        get { return molecule.Enthalpy + (IsCharged ? kJPerUnit : 0); }
    }

    public override Dictionary<Element, int> Elements { get { return molecule.Elements; } }

    public bool IsCharged { get; private set; }

    public float kJPerMole { get; private set; }
    public float kJPerUnit { get { return (float)((decimal)kJPerMole / Measures.SmolesPerMole); } }

    public ChargeableMolecule(Molecule molecule_, float kJ_per_mole)
    {
        molecule = molecule_;
        kJPerMole = kJ_per_mole;
    }

    public float Charge()
    {
        if (IsCharged)
            return 0;

        IsCharged = true;

        return kJPerUnit;
    }

    public float Discharge()
    {
        if (!IsCharged)
            return 0;

        IsCharged = false;

        return -kJPerUnit;
    }

    public ChargeableMolecule Charged()
    {
        ChargeableMolecule copy = Copy() as ChargeableMolecule;
        copy.Charge();

        return copy;
    }

    public ChargeableMolecule Discharged()
    {
        ChargeableMolecule copy = Copy() as ChargeableMolecule;
        copy.Discharge();

        return copy;
    }

    public override bool IsStackable(object obj)
    {
        if (!base.IsStackable(obj))
            return false;

        ChargeableMolecule other = obj as ChargeableMolecule;
        if (other == null)
            return false;

        return (kJPerMole == other.kJPerMole) && 
               (IsCharged == other.IsCharged);
    }

    public override Molecule Copy()
    {
        ChargeableMolecule copy = new ChargeableMolecule(molecule, kJPerMole);
        copy.IsCharged = IsCharged;

        return copy;
    }

    public override JObject EncodeJson()
    {
        return JObject.FromObject(Utility.CreateDictionary<string, object>("Type", "Chargeable Molecule", 
                                                                           "Molecule", molecule.EncodeJson(), 
                                                                           "kJ Per Mole", kJPerMole, 
                                                                           "Is Charged", IsCharged));
    }

    public override void DecodeJson(JObject json_object)
    {
        molecule = Molecule.DecodeMolecule(json_object["Molecule"] as JObject);
        kJPerMole = Utility.JTokenToFloat(json_object["kJ Per Mole"]);
        IsCharged = Utility.JTokenToBool(json_object["Is Charged"]);
    }
}
