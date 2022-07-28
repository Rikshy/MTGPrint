using Caliburn.Micro;

using MTGPrint.EventModels;

namespace MTGPrint.Helper.UI
{
    interface IEventHandler : IHandle<OpenDeckEvent>, 
        IHandle<EditLocalDataEvent>, 
        IHandle<CreateDeckEvent>, 
        IHandle<CloseScreenEvent>, 
        IHandle<UpdateStatusEvent>
    {
    }
}
