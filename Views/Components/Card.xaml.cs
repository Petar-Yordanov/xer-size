namespace XerSize.Views.Components;

public partial class Card : ContentView
{
    public static readonly BindableProperty CardStyleProperty =
        BindableProperty.Create(
            nameof(CardStyle),
            typeof(Style),
            typeof(Card),
            defaultValue: null,
            defaultValueCreator: _ => Application.Current?.Resources["CardBorderStyle"] as Style);

    public Style? CardStyle
    {
        get => (Style?)GetValue(CardStyleProperty);
        set => SetValue(CardStyleProperty, value);
    }

    public Card()
    {
        InitializeComponent();
    }
}