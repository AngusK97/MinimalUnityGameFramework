namespace Resource.ResourceModules
{
    public enum MainResource
    {
        None,
        // PlayerObj,
        // JumpSound,
    }

    public class MainResourceModule : BaseResourceModule
    {
        public override ResourceModuleName GetName()
        {
            return ResourceModuleName.Main;
        }

        protected override void RecordAllResourceInfos()
        {
            // todo: register main resources here
            // AddResourceInfo(ResourceType.GameObject, (int) MainResource.PlayerObj, "Assets/GameResources/Prefab/Character/Player.prefab");
            // AddResourceInfo(ResourceType.AudioClip, (int) MainResource.JumpSound, "Assets/GameResources/Sound/jump.wav");
        }

        public T GetRes<T>(MainResource name)
        {
            return GetResource<T>((int)name);
        }
    }
}