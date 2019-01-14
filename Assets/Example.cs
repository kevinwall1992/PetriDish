public class Example
{
    public Organism Organism { get; private set; }

    public int Length { get; private set; }

    public Example(Organism organism, int length)
    {
        Organism = organism;
        Length = length;
    }
}