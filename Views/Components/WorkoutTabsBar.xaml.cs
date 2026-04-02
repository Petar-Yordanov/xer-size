using CommunityToolkit.Maui.Behaviors;
using Microsoft.Maui.Controls.Shapes;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using XerSize.Models;

namespace XerSize.Views.Components;

public partial class WorkoutTabsBar : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(WorkoutTabsBar),
            propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(
            nameof(SelectedItem),
            typeof(object),
            typeof(WorkoutTabsBar),
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnSelectedItemChanged);

    public static readonly BindableProperty AddCommandProperty =
        BindableProperty.Create(
            nameof(AddCommand),
            typeof(ICommand),
            typeof(WorkoutTabsBar));

    public static readonly BindableProperty LongPressCommandProperty =
        BindableProperty.Create(
            nameof(LongPressCommand),
            typeof(ICommand),
            typeof(WorkoutTabsBar));

    private INotifyCollectionChanged? _observableItemsSource;

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public ICommand? AddCommand
    {
        get => (ICommand?)GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public ICommand? LongPressCommand
    {
        get => (ICommand?)GetValue(LongPressCommandProperty);
        set => SetValue(LongPressCommandProperty, value);
    }

    public WorkoutTabsBar()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        RebuildTabs();
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not WorkoutTabsBar bar)
            return;

        if (bar._observableItemsSource is not null)
            bar._observableItemsSource.CollectionChanged -= bar.OnItemsCollectionChanged;

        bar._observableItemsSource = newValue as INotifyCollectionChanged;

        if (bar._observableItemsSource is not null)
            bar._observableItemsSource.CollectionChanged += bar.OnItemsCollectionChanged;

        bar.RebuildTabs();
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is WorkoutTabsBar bar)
            bar.RebuildTabs();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildTabs();
    }

    private async void OnAddTapped(object? sender, TappedEventArgs e)
    {
        if (AddCommand?.CanExecute(null) != true)
            return;

        try
        {
            await AddSurface.ScaleToAsync(0.985, 45);
            await AddSurface.ScaleToAsync(1.0, 45);
        }
        catch
        {
            // Ignore animation interruptions.
        }

        AddCommand.Execute(null);
    }

    private void RebuildTabs()
    {
        if (TabsHost is null)
            return;

        TabsHost.Children.Clear();

        if (ItemsSource is null)
            return;

        foreach (var item in ItemsSource)
        {
            TabsHost.Children.Add(CreateTabView(item));
        }
    }

    private View CreateTabView(object item)
    {
        var resources = Application.Current?.Resources;

        var isSelected = ReferenceEquals(SelectedItem, item);

        var surfaceMutedColor = GetColor(resources, "SurfaceMutedColor", Colors.LightGray);
        var surfaceColor = GetColor(resources, "SurfaceColor", Colors.White);
        var borderColor = GetColor(resources, "BorderColor", Colors.Gray);
        var primaryTextColor = GetColor(resources, "PrimaryTextColor", Colors.Black);
        var secondaryTextColor = GetColor(resources, "SecondaryTextColor", Colors.Gray);
        var primaryColor = GetColor(resources, "PrimaryColor", Colors.Blue);

        var border = new Border
        {
            Padding = new Thickness(14, 9),
            BackgroundColor = isSelected ? surfaceColor : surfaceMutedColor,
            Stroke = isSelected ? primaryColor : borderColor,
            StrokeThickness = 1,
            MinimumWidthRequest = 92,
            MinimumHeightRequest = 34,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) }
        };

        var label = new Label
        {
            Text = GetTabText(item),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1,
            FontFamily = "OpenSansSemibold",
            FontSize = 13,
            TextColor = isSelected ? primaryTextColor : secondaryTextColor
        };

        border.Content = label;

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            try
            {
                await border.ScaleToAsync(0.985, 45);
                await border.ScaleToAsync(1.0, 45);
            }
            catch
            {
                // Ignore animation interruptions.
            }

            SelectedItem = item;
        };

        border.GestureRecognizers.Add(tap);

        if (LongPressCommand is not null)
        {
            border.Behaviors.Add(new TouchBehavior
            {
                LongPressDuration = 650,
                LongPressCommand = LongPressCommand,
                LongPressCommandParameter = item
            });
        }

        return border;
    }

    private static string GetTabText(object item)
    {
        if (item is Workout workout)
            return workout.Name;

        var nameProperty = item.GetType().GetProperty("Name");
        if (nameProperty?.GetValue(item) is string text && !string.IsNullOrWhiteSpace(text))
            return text;

        return item.ToString() ?? string.Empty;
    }

    private static Color GetColor(ResourceDictionary? resources, string key, Color fallback)
    {
        return resources is not null
               && resources.TryGetValue(key, out var value)
               && value is Color color
            ? color
            : fallback;
    }
}