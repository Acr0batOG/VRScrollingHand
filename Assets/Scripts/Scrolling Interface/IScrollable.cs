using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scrolling_Interface
{
    public interface IScrollable
    {
        public static Transform startPoint;
        public static Transform endPoint;
        public static ScrollRect scrollableList;
        public static CapsuleCollider capsuleCollider;
        public static TextMeshPro distText;
        public static TextMeshProUGUI menuText;
        public static GameManager gameManager;
        public static Slider selectionBar;
        public static TextMeshPro selectText;
        public static float userHeight; 
        public static int areaNum;
        public static int selectedItem;
        public static Coroutine dwellCoroutine; // Coroutine for the dwell selection
        public void Setup()
        {
            gameManager = GameManager.instance;
            scrollableList = gameManager.ScrollRect;
            selectedItem = gameManager.SelectedItem;
            
        }
        public void Scroll()
        {
            
        }

    }
}