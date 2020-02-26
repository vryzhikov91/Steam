namespace SteamGameParse.Data.Models
{
    public class SearchApp: Entity
    {
        public string Name { get; private set; }
        public string Appid { get; private set; }
        public bool ExecutionResult { get; private set; }

        public SearchApp(string name, string appid)
        {
            Name = name;
            Appid = appid;
            
        }

        protected SearchApp()
        {
            
        }

        public void UploadResult(bool executionResult)
        {
            this.ExecutionResult = executionResult;
        }
    }
}