namespace WebApiQandA.DTO
{
    public class UserForPublic
    {
        public string Login { get; set; } = null!;

        public override bool Equals(object? obj)
        {
            return obj is UserForPublic temp && temp.Login == Login;
        }

        public override int GetHashCode()
        {
            return Login != null ? Login.GetHashCode() : 0;
        }
    }
}
