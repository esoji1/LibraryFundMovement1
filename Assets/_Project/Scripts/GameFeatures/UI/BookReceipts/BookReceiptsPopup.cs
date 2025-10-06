using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.GameFeatures.UI.BookReceipts
{
    public class BookReceiptsPopup : MonoBehaviour
    {
        [field: SerializeField] public TMP_Dropdown BookInput { get; private set; }
        [field: SerializeField] public TMP_InputField InvoiceNumberInput { get; private set; }
        [field: SerializeField] public TMP_InputField DateReceiptInput { get; private set; }
        [field: SerializeField] public TMP_InputField SupplierInput { get; private set; }
        [field: SerializeField] public TMP_InputField QuantityInput { get; private set; }
        [field: SerializeField] public TMP_InputField PricePerUnitInput { get; private set; }
        [field: SerializeField] public TMP_Dropdown LibrarianInput { get; private set; }
        
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
            InvoiceNumberInput.text = "";
            DateReceiptInput.text = "";
            SupplierInput.text = "";
            QuantityInput.text = "";
            PricePerUnitInput.text = "";
        }
    }
}