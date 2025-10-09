using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.GameFeatures.UI.HomeForm
{
    public class HomeFormView : MonoBehaviour
    {
        [SerializeField] private Button _libratiansButton;
        [SerializeField] private Button _bookLendingButton;
        [SerializeField] private Button _genresButton;
        [SerializeField] private Button _bookButton;
        [SerializeField] private Button _bookArrivalsButton;
        [SerializeField] private Button _readersButton;
        
        public event UnityAction OnLibratiansClick
        {
            add => _libratiansButton.onClick.AddListener(value);
            remove => _libratiansButton.onClick.RemoveListener(value);
        }
        
        public event UnityAction OnBookLendingClick
        {
            add => _bookLendingButton.onClick.AddListener(value);
            remove => _bookLendingButton.onClick.RemoveListener(value);
        }
        
        public event UnityAction OnGenresClick
        {
            add => _genresButton.onClick.AddListener(value);
            remove => _genresButton.onClick.RemoveListener(value);
        }
        
        public event UnityAction OnBookClick
        {
            add => _bookButton.onClick.AddListener(value);
            remove => _bookButton.onClick.RemoveListener(value);
        }
        
        public event UnityAction OnBookArrivalsClick
        {
            add => _bookArrivalsButton.onClick.AddListener(value);
            remove => _bookArrivalsButton.onClick.RemoveListener(value);
        }
        
        public event UnityAction OnReadersClick
        {
            add => _readersButton.onClick.AddListener(value);
            remove => _readersButton.onClick.RemoveListener(value);
        }
    }
}