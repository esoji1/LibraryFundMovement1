using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.GameFeatures.UI.Books
{
    public class BooksPopup : MonoBehaviour
    {
        [field: SerializeField] public TMP_InputField ISBNInput { get; private set; }
        [field: SerializeField] public TMP_InputField NameInput { get; private set; }
        [field: SerializeField] public TMP_InputField AuthorInput { get; private set; }
        [field: SerializeField] public TMP_InputField PublishingHouseInput { get; private set; }
        [field: SerializeField] public TMP_InputField YearPublicationInput { get; private set; }
        [field: SerializeField] public TMP_Dropdown GenreList { get; private set; }
        [field: SerializeField] public TMP_InputField NumberCopiesInput { get; private set; }
        [field: SerializeField] public TMP_InputField CopiesAvailableInput { get; private set; }
        [field: SerializeField] public TMP_InputField StorageLocationInput { get; private set; }
        
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

        public void ClearFields()
        {
            ISBNInput.text = "";
            NameInput.text = "";
            AuthorInput.text = "";
            PublishingHouseInput.text = "";
            YearPublicationInput.text = "";
            NumberCopiesInput.text = "";
            CopiesAvailableInput.text = "";
            StorageLocationInput.text = "";
        }
    }
}