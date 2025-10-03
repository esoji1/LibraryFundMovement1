using _Project.GameFeatures.Database;
using _Project.GameFeatures.UI.Genres;
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

        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<DatabaseController>()
                .AsSingle();

            Container
                .Bind<LibrariansPopup>()
                .FromInstance(_librariansPopup)
                .AsSingle();

            Container
                .BindInterfacesTo<LibrariansPresenter>()
                .AsSingle();

            Container
                .Bind<GenresPopup>()
                .FromInstance(_genresPopup)
                .AsSingle();

            Container
                .BindInterfacesTo<GenresPresenter>()
                .AsSingle();

            Container
                .Bind<ReadersPopup>()
                .FromInstance(_readersPopup)
                .AsSingle();

            Container
                .BindInterfacesTo<ReadersPresenter>()
                .AsSingle();

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