using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scrolling_Interface
{
    public interface IScrollable
    {
        // Coroutine for the dwell selection
        public void Setup();

        public void Scroll(Collider collider);

    }
}