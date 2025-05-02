using Configs;
using Cysharp.Threading.Tasks;
using Itic.Scopes;
using Monobehaviours.Animations;
using Providers;
using R3;
using UnityEngine.Windows;
using VContainer.Unity;
using Views;
using UnityEngine;
using Input = UnityEngine.Input;

namespace Initializers
{
    public class IslandInitializer : IInitializable
    {
        private readonly IslandProvider _islandProvider;

        // private readonly IslandMenuView _islandMenuView;
        private readonly SceneLoader _sceneLoader;
        private readonly CameraProvider _cameraProvider;

        public IslandInitializer(
            IslandProvider islandProvider,
            // IslandMenuView islandMenuView,
            SceneLoader sceneLoader,
            CameraProvider cameraProvider
        )
        {
            _islandProvider = islandProvider;
            // _islandMenuView = islandMenuView;
            _sceneLoader = sceneLoader;
            _cameraProvider = cameraProvider;
        }

        public void Initialize()
        {
            // Observable
            //     .EveryUpdate(UnityFrameProvider.Update)
            //     .Where(_ => Input.GetMouseButtonDown(0))
            //     .Subscribe(_ => OnMouseDown());

            foreach (var islandView in _islandProvider.IslandViews)
            {
                islandView.OnClick += (island) => { IslandViewOnClick(island).Forget(); };
            }
        }

        private async UniTask IslandViewOnClick(IslandView islandView)
        {
            var cameraTransition = _cameraProvider.Camera.GetComponent<CameraTransition>();
            
            await cameraTransition.TransitionToTargetAsync(islandView.TargetPosition);
            
            _sceneLoader.LoadRegionScene(islandView.CountryConfig.countryId).Forget();
        }
    }
}