
public interface Versionable<T> : Copiable<T>
{
    void Checkout(T version);
    bool IsSameVersion(T version);
}


