using _Project.GameFeatures.Database;
using _Project.GameFeatures.UI.BookLending;
using _Project.GameFeatures.UI.BookReceipts;
using _Project.GameFeatures.UI.Books;
using _Project.GameFeatures.UI.Genres;
using _Project.GameFeatures.UI.HomeForm;
using _Project.GameFeatures.UI.Librarians;
using _Project.GameFeatures.UI.Notifications;
using _Project.GameFeatures.UI.Readers;
using UnityEngine;
using Zenject;

namespace _Project.Core.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private MonoBehaviorComponent _monoBehaviorComponent;
        [SerializeField] private LibrariansPopup _librariansPopup;
        [SerializeField] private GenresPopup _genresPopup;
        [SerializeField] private ReadersPopup _readersPopup;
        [SerializeField] private NotificationPopup _notificationPopup;
        [SerializeField] private NotificationList _notificationList;
        [SerializeField] private BooksPopup _booksPopup;
        [SerializeField] private BookReceiptsPopup _bookReceiptsPopup;
        [SerializeField] private BookLendingPopup _bookLendingPopup;
        [SerializeField] private HomeFormView homeFormView;

        public override void InstallBindings()
        {
            BindNotification();
            
            Container
                .BindInterfacesAndSelfTo<DatabaseController>()
                .AsSingle();

            Container
                .Bind<LibrariansPopup>()
                .FromInstance(_librariansPopup)
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<LibrariansPresenter>()
                .AsSingle();

            Container
                .Bind<GenresPopup>()
                .FromInstance(_genresPopup)
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<GenresPresenter>()
                .AsSingle();

            Container
                .Bind<ReadersPopup>()
                .FromInstance(_readersPopup)
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<ReadersPresenter>()
                .AsSingle();

            Container
                .Bind<BooksPopup>()
                .FromInstance(_booksPopup)
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BooksPresenter>()
                .AsSingle();
            
            Container
                .Bind<BookReceiptsPopup>()
                .FromInstance(_bookReceiptsPopup)
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BookReceiptsPresenter>()
                .AsSingle();
            
            Container
                .Bind<BookLendingPopup>()
                .FromInstance(_bookLendingPopup)
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BookLendingPresenter>()
                .AsSingle();
            
            Container
                .Bind<HomeFormView>()
                .FromInstance(homeFormView)
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<HomeFormPresenter>()
                .AsSingle();
        }

        private void BindNotification()
        {
            Container
                .Bind<NotificationPopup>()
                .FromInstance(_notificationPopup)
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<NotificationList>()
                .FromInstance(_notificationList)
                .AsSingle();

            Container
                .BindInterfacesTo<NotificationListPresenter>()
                .AsSingle()
                .WithArguments(_monoBehaviorComponent);
        }
    }
}