namespace MTGPrint.EventModels
{
    public class UpdateStatusEvent
    {
        public bool? IsLoading { get; set; } = null;
        public bool? IsWndEnabled { get; set; } = null;
        public string Info { get; set; } = null;
        public string Status { get; set; } = null;
        public string Errors { get; set; } = null;
    }
}
