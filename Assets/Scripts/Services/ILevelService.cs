using Core.Grid.Interfaces;
using Cysharp.Threading.Tasks;

namespace Services
{
    public interface ILevelService
    {
        UniTask<IGrid> InitializeLevel(int levelId);
    }
}