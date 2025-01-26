using System.Collections.Generic;

namespace _Scripts.Database_Objects
{
    public class Trial { //Object for database insertion
        public int BlockId; //Which user is being tested in what block; used as key
        public int TrialId; //Id of trial, multiple per user
        public float TimeToComplete; //During trial time to take and proper selection
        public bool CorrectSelection;
        public int UserId;
        public int AreaNumber;
        public int TechniqueNumber;
        public int SelectedItem;
        public int CorrectItem;
        public int NumberOfItems;
        public float ItemLocation;
        public float DistanceToItem;
        public float DistanceTravelled;
        public float StartingPoint;
        public float InitialNormalisedLandingPoint;
        public float OvershootErrorDistance;
        public float SizeOfList;
        public float ExactLandingPoint;
        public float AmplitudeOfFlick;
        public float AverageSpeedOfFlicks;
        public int NumberOfFlicks;
        public List<float> TimeBetweenFlicksArray;
        public Trial() {
        }

        public Trial(int userId, int blockId, int trialId, float timeToComplete, bool correctSelection, int areaNumber, int techniqueNumber, int selectedItem, int correctItem, float itemLocation, float distanceToItem, float distanceTravelled, int numberOfItems, float startingPoint, float initialNormalisedLandingPoint, float overshootErrorDistance, float sizeOfList, float exactLandingPoint, float amplitudeOfFlick, float averageSpeedOfFlicks, int numberOfFlicks, List<float> timeBetweenFlicksArray) {
            this.UserId = userId;
            this.BlockId = blockId;
            this.TrialId = trialId;
            this.TimeToComplete = timeToComplete;
            this.CorrectSelection = correctSelection;  
            this.AreaNumber = areaNumber;
            this.TechniqueNumber = techniqueNumber;
            this.SelectedItem = selectedItem;
            this.CorrectItem = correctItem;
            this.ItemLocation = itemLocation;
            this.DistanceToItem = distanceToItem;
            this.DistanceTravelled = distanceTravelled;
            this.NumberOfItems = numberOfItems;
            this.StartingPoint = startingPoint;
            this.InitialNormalisedLandingPoint = initialNormalisedLandingPoint;
            this.OvershootErrorDistance = overshootErrorDistance;
            this.SizeOfList = sizeOfList;
            this.ExactLandingPoint = exactLandingPoint;
            this.AmplitudeOfFlick = amplitudeOfFlick;
            this.AverageSpeedOfFlicks = averageSpeedOfFlicks;
            this.NumberOfFlicks = numberOfFlicks;
            this.TimeBetweenFlicksArray = timeBetweenFlicksArray;
            

        }
    }
}

