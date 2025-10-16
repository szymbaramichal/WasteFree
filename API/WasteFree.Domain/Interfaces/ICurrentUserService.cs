namespace WasteFree.Domain.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string Username { get; }
    }
}

