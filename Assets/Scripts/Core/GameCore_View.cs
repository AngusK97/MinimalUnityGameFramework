using UI;

namespace Core
{
    public partial class GameCore
    {
        private void RegisterAllViewModules()
        {
            // todo: register all views here
            UI.RegisterUiData(new UIData(UIName.Menu, "Assets/GameResources/Prefab/UI/MenuView.prefab"));
        }
    }
}