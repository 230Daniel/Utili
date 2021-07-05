using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class User : UserEntity
    {
        public string Email { get; set; }
        public string CustomerId { get; set; }

        public User(ulong userId) : base(userId) { }
    }
}
