using CommunityToolkit.Mvvm.Input;
using XerSize.Models;
using XerSize.Services.Interfaces;
using XerSize.ViewModels.Base;

namespace XerSize.ViewModels.Exercises;

public partial class ExercisesPageViewModel : ViewModelBase
{
    private readonly IExerciseCatalogService _catalogService;

    public ObservableCollection<ExerciseCatalogItem> Items { get; } = new();

    public ExercisesPageViewModel(IExerciseCatalogService catalogService)
    {
        _catalogService = catalogService;
        Title = "Exercises";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var items = await _catalogService.GetAllAsync();

        Items.Clear();
        foreach (var item in items.OrderBy(x => x.Name))
            Items.Add(item);
    }
}