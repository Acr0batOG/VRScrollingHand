namespace _Scripts.Database_Objects
{
    public class User { //Object for database insertion
        public int UserId;
        public string Name;
        public float UserHeight;
 
        public User() {
        }

        public User(int userId, string name, float userHeight) {
            this.UserId = userId;
            this.Name = name;
            this.UserHeight = userHeight;
        }
    }
}
