using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.GameFeatures.UI.Genres
{
    public class GenresPopup : MonoBehaviour
    {
        [field: SerializeField] public TMP_InputField GenreInput { get; private set; }
        
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _deleteButton;

        public event UnityAction OnPreviousClick
        {
            add => _previousButton.onClick.AddListener(value);
            remove => _previousButton.onClick.RemoveListener(value);
        }

        public event UnityAction OnNextClick
        {
            add => _nextButton.onClick.AddListener(value);
            remove => _nextButton.onClick.RemoveListener(value);
        }

        public event UnityAction OnDeleteClick
        {
            add => _deleteButton.onClick.AddListener(value);
            remove => _deleteButton.onClick.RemoveListener(value);
        }
        
        public void ClearField() => GenreInput.text = "";
    }
}