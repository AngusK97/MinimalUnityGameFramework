using UI;

namespace Core
{
    public partial class GameCore
    {
        private void RegisterAllViewModules()
        {
            // todo: register all views here
            UI.RegisterUiData(new UIData(UIName.Menu, "Assets/GameResources/Prefab/UI/MenuView.prefab"));
            // UI.RegisterUiData(new UIData(UIName.LevelHud, "Assets/GameResources/Prefab/UI/LevelHudView.prefab"));
            // UI.RegisterUiData(new UIData(UIName.Plot, "Assets/GameResources/Prefab/UI/PlotView.prefab"));
            // UI.RegisterUiData(new UIData(UIName.SpecialHud, "Assets/GameResources/Prefab/UI/SpecialHudView.prefab"));
        }
    }
}