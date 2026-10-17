using Resource;

namespace Demo
{
    public enum MainResource
    {
        None,
        Cue,
    }

    public class MainResourceModule : BaseResourceModule
    {
        public override string GetName()
        {
            return DemoResources.Main;
        }

        protected override void RecordAllResourceInfos()
        {
            AddResourceInfo(ResourceType.AudioClip, (int)MainResource.Cue, "Assets/GameContent/Demo/Content/Audio/cue.wav");
        }

        public T GetRes<T>(MainResource name)
        {
            return GetResource<T>((int)name);
        }
    }
}
