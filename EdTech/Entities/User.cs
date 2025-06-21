
namespace EdTech.Entities
{
    public class User
    {
        public long UserId {  get; set; }
        public string? UserName { get; set; } = string.Empty;
        public string? UserEmail { get; set; } = string.Empty;
        public string? UserPassword { get; set;} = string.Empty;
        public int? UserType { get; set;} 
        public string? UserToken {  get; set; } = string.Empty;

        public int? UserState { get; set; }

    }
}
