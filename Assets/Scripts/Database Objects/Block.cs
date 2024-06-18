namespace Database_Objects
{
    public class Block { //Object for database insertion
        public int BlockId; //Which user is being tested in what block; used as key
        public int UserId; //Id of trial, multiple per user
        public int AreaNumber;
        public int TechniqueNumber;
        public bool BodyVisibility;
 
        public Block() {
        }

        public Block(int blockId, int userId, int areaNumber, int techniqueNumber, bool bodyVisibility) {
            this.BlockId = blockId;
            this.UserId = userId;
            this.AreaNumber = areaNumber;
            this.TechniqueNumber = techniqueNumber;
            this.BodyVisibility = bodyVisibility;
        }
    }
}