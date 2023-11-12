namespace TinyAssembler;

internal interface IFreezable
{
    public bool IsFrozen { get; }
    public void Freeze();
}