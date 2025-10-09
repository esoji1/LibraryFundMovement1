using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.GameFeatures.UI.BookLending
{
    public class BookLendingPopup : MonoBehaviour
    {
        [field: SerializeField] public TMP_Dropdown BookInput { get; private set; }
        [field: SerializeField] public TMP_Dropdown ReaderInput { get; private set; }
        [field: SerializeField] public TMP_Dropdown LibrarianInput { get; private set; }
        [field: SerializeField] public TMP_InputField DataIssueInput { get; private set; }
        [field: SerializeField] public TMP_InputField ReturnPeriodInput { get; private set; }
        [field: SerializeField] public TMP_InputField ReturnDateInput { get; private set; }
        [field: SerializeField] public TMP_InputField StatusInput { get; private set; }
        
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _saveButton;

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
        
        public event UnityAction OnSaveClick
        {
            add => _saveButton.onClick.AddListener(value);
            remove => _saveButton.onClick.RemoveListener(value);
        }

        public void ClearFields()
        {
            DataIssueInput.text = "";
            ReturnPeriodInput.text = "";
            ReturnDateInput.text = "";
            StatusInput.text = "";
        }
    }
}