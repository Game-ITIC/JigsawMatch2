using VContainer.Unity;

namespace Initializers
{
    public class CountryInitializer : IInitializable
    {
        private readonly MenuView _menuView;

        public CountryInitializer(MenuView menuView)
        {
            _menuView = menuView;
        }

        public void Initialize()
        {
            _menuView.Warmup();
        }
    }
}