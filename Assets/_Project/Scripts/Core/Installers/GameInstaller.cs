using _Project.GameFeatures.Database;
using _Project.GameFeatures.UI.Librarians;
using UnityEngine;
using Zenject;

namespace _Project.Core.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private LibrariansPopup _librariansPopup;
        
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
        }
    }
}