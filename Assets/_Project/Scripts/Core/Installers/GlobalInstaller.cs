using _Project.Core.Services;
using Zenject;

namespace _Project.Core.Installers
{
    public class GlobalInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindNotificationService();
        }

        private void BindNotificationService()
        {
            Container
                .Bind<NotificationService>()
                .AsSingle();
        }
    }
}