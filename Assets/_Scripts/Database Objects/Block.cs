namespace _Scripts.Database_Objects
{
    public class Block { //Object for database insertion
        public int BlockId; //Which user is being tested in what block; used as key
        public int UserId; //Id of trial, multiple per user
        public int AreaNumber;
        public int TechniqueNumber;
        public int NumberOfItems;
 
        public Block() {
        }

        public Block(int blockId, int userId, int areaNumber, int techniqueNumber, int numberOfItems) {
            this.BlockId = blockId;
            this.UserId = userId;
            this.AreaNumber = areaNumber;
            this.TechniqueNumber = techniqueNumber;
            this.NumberOfItems = numberOfItems;
        }
    }
}