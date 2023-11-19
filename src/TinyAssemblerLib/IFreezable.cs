namespace TinyAssemblerLib;

internal interface IFreezable
{
    public bool IsFrozen { get; }
    public void Freeze();
}