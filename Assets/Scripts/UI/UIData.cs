namespace UI
{
    public class UIData
    {
        public string UIName { get; set; }
        public string ResourcePath { get; set; }

        public UIData(string uiName, string resourcePath)
        {
            UIName = uiName;
            ResourcePath = resourcePath;
        }
    }
}
