using Resource.ResourceModules;

namespace Core
{
    public partial class GameCore
    {
        private void RegisterAllResourceModules()
        {
            // todo: register all resource modules here
            Resource.RegisterResourceModule(new MainResourceModule());
        }
    }
}