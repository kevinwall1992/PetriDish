//Linguistically, this is ambiguous.
//Here, a Choice is a collection of Options.

public interface Option<T>
{
    Choice<T> Choice { get; }

    T Value { get; set; }
}

public interface Choice<T>
{
    Option<T> Selection { get; set; }
}
