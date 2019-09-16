
//May need to come up with some bogus name for our units
public static class Measures
{
    public static decimal AvogadrosNumber { get { return 6.02m * (decimal)System.Math.Pow(10, 23); } }

    public static decimal GramsPerDalton { get { return 1.66m * (decimal)System.Math.Pow(10, -24); } }
    public static decimal SmolesPerMole { get { return (decimal)System.Math.Pow(10, 19); } }
    public static decimal MolesOfWaterPerLiter { get { return 55.56m; } }
    public static decimal SmolesOfWaterPerLiter { get { return MolesToSmoles(MolesOfWaterPerLiter); } }

    public static decimal SmolesToMoles(decimal smoles)
    {
        return smoles / SmolesPerMole;
    }

    public static float SmolesToMoles(float smoles)
    {
        return (float)SmolesToMoles((decimal)smoles);
    }

    public static decimal MolesToSmoles(decimal moles)
    {
        return moles * SmolesPerMole;
    }

    public static float MolesToSmoles(float moles)
    {
        return (float)MolesToSmoles((decimal)moles);
    }

    public static decimal GetMoles(Compound compound)
    {
        return SmolesToMoles((decimal)compound.Quantity);
    }

    public static decimal GramsToMoles(Molecule molecule, decimal grams)
    {
        return grams / (molecule.Mass * AvogadrosNumber * GramsPerDalton);
    }

    public static decimal MolesToGrams(Molecule molecule, decimal moles)
    {
        return molecule.Mass * moles * AvogadrosNumber * GramsPerDalton;
    }

    public static decimal GramsToSmoles(Molecule molecule, decimal grams)
    {
        return MolesToSmoles(GramsToMoles(molecule, grams));
    }

    public static decimal SmolesToGrams(Molecule molecule, decimal smoles)
    {
        return MolesToGrams(molecule, SmolesToMoles(smoles));
    }

    public static decimal GetGrams(Compound compound)
    {
        return SmolesToGrams(compound.Molecule, (decimal)compound.Quantity);
    }

    public static decimal PPMToMoles(decimal moles_of_medium, decimal ppm)
    {
        return (moles_of_medium / 1000000) * ppm;
    }

    public static decimal PPMToMolesPerLiter(decimal ppm)
    {
        return PPMToMoles(MolesOfWaterPerLiter, ppm);
    }
}