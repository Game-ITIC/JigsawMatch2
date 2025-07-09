using Cysharp.Threading.Tasks;

namespace Interfaces
{
    public interface IPreload
    {
        UniTask Warmup();
    }
}