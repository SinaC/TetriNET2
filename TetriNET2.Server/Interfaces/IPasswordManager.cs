namespace TetriNET2.Server.Interfaces
{
    public enum DomainTypes
    {
        Admin,
        Player,
        Game,
    }

    public interface IPasswordManager
    {
        bool CheckSucceedIfNotFound { get; set; }

        bool Add(DomainTypes domainType, string name, string cryptedPassword);
        bool Remove(DomainTypes domainType, string name);
        bool Check(DomainTypes domainType, string name, string cryptedPassword);
    }
}
