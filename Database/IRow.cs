using System.Threading.Tasks;

namespace Database
{
    public interface IRow
    {
        public bool New { get; set; }

        public Task SaveAsync();

        public Task DeleteAsync();
    }
}
