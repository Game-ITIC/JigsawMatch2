using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Itic.Scopes
{
    public class ScopeInstaller : MonoBehaviour
    {
        [SerializeField] private LifetimeScope _lifetimeScope;

        private InitializationQueue _executor;
        
        public async UniTask InstallScopeAsync()
        {
            _lifetimeScope.Build();
            
            await _executor.Execute();
        }
        
        [Inject]
        private void Inject(InitializationQueue executor)
        {
            _executor = executor;
        }
    }
}