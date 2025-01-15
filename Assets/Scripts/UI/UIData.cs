namespace UI
{
    public class UIData
    {
        public UIName UIName { get; set; }
        public string ResourcePath { get; set; }
    
        public UIData(UIName uiName, string resourcePath)
        {
            UIName = uiName;
            ResourcePath = resourcePath;
        }
    }
}