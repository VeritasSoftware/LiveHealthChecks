using LiveHealthChecks.UI.Components;
using LiveHealthChecks.UI.Models;
using MudBlazor;

namespace LiveHealthChecks.UI.Services
{
    public interface IMyDialogService
    {
        void OpenMessageDialog(string message, bool isError = true);
        void OpenViewHealthCheckDialog(HealthCheck healthCheck);
    }

    public class MyDialogService : IMyDialogService
    {
        private readonly IDialogService _dialogService;
        
        public MyDialogService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public void OpenMessageDialog(string message, bool isError = true)
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.ExtraSmall,
                Position = DialogPosition.Center,
                CloseButton = true
            };

            var parameters = new DialogParameters<MessageDialog>();
            parameters.Add(x => x.Message, message);
            parameters.Add(x => x.IsError, isError);

            _dialogService.Show<MessageDialog>("Message", parameters, options);

            Console.WriteLine(message);
        }

        public void OpenViewHealthCheckDialog(HealthCheck healthCheck)
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.ExtraSmall,
                FullWidth = true,
                Position = DialogPosition.Center,
                CloseButton = true,
            };
            var parameters = new DialogParameters<ViewDialog>();
            parameters.Add(x => x.HealthCheck, healthCheck);

            _dialogService.Show<ViewDialog>("View Health Check", parameters, options);
        }
    }
}
