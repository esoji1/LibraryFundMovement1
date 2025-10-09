using _Project.GameFeatures.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.GameFeatures.UI.Readers
{
    public class ReadersPopup : MonoBehaviour, IPopup
    {
        [field: SerializeField] public TMP_InputField LastNameInput { get; private set; }
        [field: SerializeField] public TMP_InputField FirstNameInput { get; private set; }
        [field: SerializeField] public TMP_InputField SurnameInput { get; private set; }
        [field: SerializeField] public TMP_InputField PassportDetailsInput { get; private set; }
        [field: SerializeField] public TMP_InputField TelephoneInput { get; private set; }
        [field: SerializeField] public TMP_InputField EmailInput { get; private set; }
        [field: SerializeField] public TMP_InputField DateRegistrationInput { get; private set; }
        
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

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
        
        public void ClearFields()
        {
            LastNameInput.text = "";
            FirstNameInput.text = "";
            SurnameInput.text = "";
            PassportDetailsInput.text = "";
            TelephoneInput.text = "";
            EmailInput.text = "";
            DateRegistrationInput.text = "";
        }
    }
}