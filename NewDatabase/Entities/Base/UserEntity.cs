namespace NewDatabase.Entities.Base
{
    public class UserEntity
    {
        public ulong UserId { get; internal set; }

        protected UserEntity(ulong userId)
        {
            UserId = userId;
        }

        internal UserEntity() { }
    }
}
