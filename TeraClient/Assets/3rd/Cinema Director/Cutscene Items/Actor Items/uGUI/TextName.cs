using UnityEngine;
using UnityEngine.UI;

namespace CinemaDirector
{
    public class TextName : MonoBehaviour
    {
        public string NameId;
        public Text Name;

        void Awake()
        {
            var dialogue = CGManager.Instance.GetDialogueById(NameId);
            if(dialogue != null)
                Name.text = dialogue.Name;
        }
    }
}