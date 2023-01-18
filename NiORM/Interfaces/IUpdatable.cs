namespace NiORM.Interfaces
{
    public interface IUpdatable
    {
        DateTime CreatedDateTime { get; set; }
        DateTime UpdatedDateTime { get; set; }
    }
}
