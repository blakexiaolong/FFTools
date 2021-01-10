using ModManager.Models;
using System.Linq;

namespace ModManager.ViewModels
{
    public class ModDataViewModel
    {
        private ModData _model;
        public ModDataViewModel(string url)
        {
            _model = new ModData(url);
        }
        public void Import(string dirPath) => _model.Import(dirPath);

        public string Name => _model.Name;
        public string Author => _model.Author;
        public string Description => _model.Description;
        public string Image => _model.ImageUrls.Length >= 1 ? _model.ImageUrls[0] : null;
        public string[] Files => _model.Files.Select(x => $"{x.Key} ({x.Value.Split('/')[2]})")
            .Concat(_model.OtherFiles.Select(x => $"{x.Key} ({x.Value.Split('/')[2]})")).ToArray();
    }
}
