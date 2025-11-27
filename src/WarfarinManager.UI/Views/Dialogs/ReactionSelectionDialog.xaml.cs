using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.UI.Views.Dialogs
{
    /// <summary>
    /// Dialog per la selezione del tipo di reazione avversa con color-coding
    /// </summary>
    public partial class ReactionSelectionDialog : Window
    {
        public AdverseReactionType? SelectedReaction { get; private set; }

        public ReactionSelectionDialog()
        {
            InitializeComponent();
            LoadReactions();
        }

        /// <summary>
        /// Carica tutte le reazioni raggruppate per gravità
        /// </summary>
        private void LoadReactions()
        {
            ReactionsPanel.Children.Clear();

            // Raggruppa le reazioni per gravità
            var reactionGroups = Enum.GetValues<AdverseReactionType>()
                .Select(r => new
                {
                    Reaction = r,
                    Info = GetReactionInfo(r),
                    Description = GetDescription(r)
                })
                .GroupBy(x => x.Info.Severity)
                .OrderBy(g => g.Key);

            foreach (var group in reactionGroups)
            {
                // Header del gruppo
                var groupHeader = CreateGroupHeader(group.Key);
                ReactionsPanel.Children.Add(groupHeader);

                // Reazioni del gruppo
                foreach (var reaction in group.OrderBy(r => r.Description))
                {
                    var reactionButton = CreateReactionButton(
                        reaction.Reaction,
                        reaction.Description,
                        reaction.Info);
                    ReactionsPanel.Children.Add(reactionButton);
                }

                // Spazio tra gruppi
                ReactionsPanel.Children.Add(new Separator { Margin = new Thickness(0, 10, 0, 10) });
            }
        }

        /// <summary>
        /// Crea l'header del gruppo di gravità
        /// </summary>
        private Border CreateGroupHeader(AdverseReactionSeverity severity)
        {
            var (title, color, icon) = severity switch
            {
                AdverseReactionSeverity.Critical => ("COMPLICAZIONI CRITICHE", "#D13438", "🔴"),
                AdverseReactionSeverity.Serious => ("COMPLICAZIONI GRAVI", "#FFB900", "🟡"),
                AdverseReactionSeverity.Common => ("EFFETTI AVVERSI COMUNI", "#107C10", "🟢"),
                _ => ("ALTRO", "#666666", "⚪")
            };

            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 10, 0, 10)
            };

            var stack = new StackPanel { Orientation = Orientation.Horizontal };

            var iconText = new TextBlock
            {
                Text = icon,
                FontSize = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };

            stack.Children.Add(iconText);
            stack.Children.Add(titleText);
            border.Child = stack;

            return border;
        }

        /// <summary>
        /// Crea il pulsante per una singola reazione
        /// </summary>
        private Border CreateReactionButton(
            AdverseReactionType reaction,
            string description,
            AdverseReactionInfoAttribute info)
        {
            var outerBorder = new Border
            {
                Margin = new Thickness(0, 4, 0, 4),
                CornerRadius = new CornerRadius(4),
                Background = Brushes.White,
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            // Se può insorgere all'inizio, evidenzia con bordo arancione
            if (info.CanOccurAtStart)
            {
                outerBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B00"));
                outerBorder.BorderThickness = new Thickness(2);
            }

            var button = new Button
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(12, 10, 12, 10),
                Tag = reaction,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            button.Click += ReactionButton_Click;

            // Effetto hover
            button.MouseEnter += (s, e) =>
            {
                outerBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
            };
            button.MouseLeave += (s, e) =>
            {
                outerBorder.Background = Brushes.White;
            };

            var stack = new StackPanel();

            var headerStack = new StackPanel { Orientation = Orientation.Horizontal };

            // Descrizione
            var descriptionText = new TextBlock
            {
                Text = description,
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center
            };
            headerStack.Children.Add(descriptionText);

            // Badge "Inizio terapia" se applicabile
            if (info.CanOccurAtStart)
            {
                var earlyBadge = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B00")),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(6, 2, 6, 2),
                    Margin = new Thickness(10, 0, 0, 0)
                };

                var badgeText = new TextBlock
                {
                    Text = "⚠️ Inizio terapia",
                    FontSize = 10,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold
                };

                earlyBadge.Child = badgeText;
                headerStack.Children.Add(earlyBadge);
            }

            stack.Children.Add(headerStack);

            // Incidenza
            var incidenceText = new TextBlock
            {
                Text = $"Incidenza: {info.Incidence}",
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                Margin = new Thickness(0, 4, 0, 0)
            };
            stack.Children.Add(incidenceText);

            button.Content = stack;
            outerBorder.Child = button;

            return outerBorder;
        }

        /// <summary>
        /// Gestisce il click su una reazione
        /// </summary>
        private void ReactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AdverseReactionType reaction)
            {
                SelectedReaction = reaction;
                DialogResult = true;
                Close();
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante Annulla
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Ottiene le informazioni di una reazione
        /// </summary>
        private AdverseReactionInfoAttribute GetReactionInfo(AdverseReactionType reaction)
        {
            var field = reaction.GetType().GetField(reaction.ToString());
            var attribute = field?.GetCustomAttribute<AdverseReactionInfoAttribute>();
            return attribute ?? new AdverseReactionInfoAttribute(AdverseReactionSeverity.Common, "N/D", false);
        }

        /// <summary>
        /// Ottiene la descrizione di una reazione
        /// </summary>
        private string GetDescription(AdverseReactionType reaction)
        {
            var field = reaction.GetType().GetField(reaction.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? reaction.ToString();
        }
    }
}
