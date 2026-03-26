using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XerSize.ViewModels.Base;

namespace XerSize.ViewModels.Routines;

public partial class RoutineEditorViewModel : ValidatableViewModelBase
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Routine name is required.")]
    [MinLength(2, ErrorMessage = "Routine name must be at least 2 characters.")]
    [MaxLength(40, ErrorMessage = "Routine name must be 40 characters or less.")]
    public partial string RoutineName { get; set; } = string.Empty;

    partial void OnRoutineNameChanged(string value)
    {
        ValidateProperty(value, nameof(RoutineName));
        OnPropertyChanged(nameof(CanSubmit));
        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private Task SaveAsync() => Task.CompletedTask;

    private bool CanSave() => !HasErrors && !string.IsNullOrWhiteSpace(RoutineName);
}
