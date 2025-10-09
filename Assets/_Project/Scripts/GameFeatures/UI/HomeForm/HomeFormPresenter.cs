using System;
using _Project.GameFeatures.UI.BookLending;
using _Project.GameFeatures.UI.BookReceipts;
using _Project.GameFeatures.UI.Books;
using _Project.GameFeatures.UI.Common;
using _Project.GameFeatures.UI.Genres;
using _Project.GameFeatures.UI.Librarians;
using _Project.GameFeatures.UI.Readers;
using Zenject;

namespace _Project.GameFeatures.UI.HomeForm
{
    public class HomeFormPresenter : IInitializable, IDisposable
    {
        private readonly HomeFormView _homeFormView;
        private readonly LibrariansPopup _librariansPopup;
        private readonly BookLendingPopup _bookLendingPopup;
        private readonly GenresPopup _genresPopup;
        private readonly BooksPopup _booksPopup;
        private readonly BookReceiptsPopup _bookReceiptsPopup;
        private readonly ReadersPopup _readersPopup;

        private IPopup _currentPopup;

        public HomeFormPresenter(HomeFormView homeFormView, LibrariansPopup librariansPopup,
            BookLendingPopup bookLendingPopup, GenresPopup genresPopup,
            BooksPopup booksPopup, BookReceiptsPopup bookReceiptsPopup, ReadersPopup readersPopup)
        {
            _homeFormView = homeFormView;
            _librariansPopup = librariansPopup;
            _bookLendingPopup = bookLendingPopup;
            _genresPopup = genresPopup;
            _booksPopup = booksPopup;
            _bookReceiptsPopup = bookReceiptsPopup;
            _readersPopup = readersPopup;
        }

        public void Initialize()
        {
            _homeFormView.OnLibratiansClick += OnLibratiansClick;
            _homeFormView.OnBookLendingClick += OnBookLendingClick;
            _homeFormView.OnGenresClick += OnGenresClick;
            _homeFormView.OnBookClick += OnBookClick;
            _homeFormView.OnBookArrivalsClick += OnBookArrivalsClick;
            _homeFormView.OnReadersClick += OnReadersClick;
        }

        public void Dispose()
        {
            _homeFormView.OnLibratiansClick -= OnLibratiansClick;
            _homeFormView.OnBookLendingClick -= OnBookLendingClick;
            _homeFormView.OnGenresClick -= OnGenresClick;
            _homeFormView.OnBookClick -= OnBookClick;
            _homeFormView.OnBookArrivalsClick -= OnBookArrivalsClick;
            _homeFormView.OnReadersClick -= OnReadersClick;
        }

        private void OnLibratiansClick()
        {
            if (CheckCurrentPopup())
                _currentPopup.Hide();

            _librariansPopup.Show();
            _currentPopup = _librariansPopup;
        }

        private void OnBookLendingClick()
        {
            if (CheckCurrentPopup())
                _currentPopup.Hide();

            _bookLendingPopup.Show();
            _currentPopup = _bookLendingPopup;
        }

        private void OnGenresClick()
        {
            if (CheckCurrentPopup())
                _currentPopup.Hide();
            
            _genresPopup.Show();
            _currentPopup = _genresPopup;
        }

        private void OnBookClick()
        {
            if (CheckCurrentPopup())
                _currentPopup.Hide();
            
            _booksPopup.Show();
            _currentPopup = _booksPopup;
        }

        private void OnBookArrivalsClick()
        {
            if (CheckCurrentPopup())
                _currentPopup.Hide();
            
            _bookReceiptsPopup.Show();
            _currentPopup = _bookReceiptsPopup;
        }

        private void OnReadersClick()
        {
            if (CheckCurrentPopup())
                _currentPopup.Hide();
            
            _readersPopup.Show();
            _currentPopup = _readersPopup;
        }

        private bool CheckCurrentPopup() => _currentPopup != null;
    }
}