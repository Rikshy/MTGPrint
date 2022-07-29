using System;

using MTGPrint.Helper;

namespace MTGPrint.ViewModels
{
    public class InfoViewModel
    {
        private readonly Uri uri = new("https://github.com/Rikshy/MTGPrint");

        public void OpenProjectUrl()
            => uri.Open();
    }
}
