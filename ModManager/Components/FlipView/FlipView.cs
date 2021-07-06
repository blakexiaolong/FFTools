using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ModManager.Components
{
    public class FlipView : Selector
    {
        #region Private Fields
        private ContentControl PART_CurrentItem;
        private ContentControl PART_PreviousItem;
        private ContentControl PART_NextItem;
        private FrameworkElement PART_Root;
        private FrameworkElement PART_Container;
        private double fromValue = 0.0;
        private double elasticFactor = 1.0;
        #endregion

        #region Constructor
        static FlipView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlipView), new FrameworkPropertyMetadata(typeof(FlipView)));
            SelectedIndexProperty.OverrideMetadata(typeof(FlipView), new FrameworkPropertyMetadata(-1, OnSelectedIndexChanged));
        }

        public FlipView()
        {
            CommandBindings.Add(new CommandBinding(NextCommand, OnNextExecuted, OnNextCanExecute));
            CommandBindings.Add(new CommandBinding(PreviousCommand, OnPreviousExecuted, OnPreviousCanExecute));
        }
        #endregion

        #region Private methods
        private void OnRootManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            fromValue = e.TotalManipulation.Translation.X;
            if (fromValue > 0)
            {
                if (SelectedIndex > 0)
                {
                    SelectedIndex -= 1;
                }
            }
            else
            {
                if (SelectedIndex < Items.Count - 1)
                {
                    SelectedIndex += 1;
                }
            }

            if (elasticFactor < 1)
            {
                RunSlideAnimation(0, ((MatrixTransform)PART_Root.RenderTransform).Matrix.OffsetX);
            }
            elasticFactor = 1.0;
        }

        private void OnRootManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!(PART_Root.RenderTransform is MatrixTransform))
            {
                PART_Root.RenderTransform = new MatrixTransform();
            }

            Matrix matrix = ((MatrixTransform)PART_Root.RenderTransform).Matrix;
            var delta = e.DeltaManipulation;

            if ((SelectedIndex == 0 && delta.Translation.X > 0 && elasticFactor > 0) || (SelectedIndex == Items.Count - 1 && delta.Translation.X < 0 && elasticFactor > 0))
            {
                elasticFactor -= 0.05;
            }

            matrix.Translate(delta.Translation.X * elasticFactor, 0);
            PART_Root.RenderTransform = new MatrixTransform(matrix);

            e.Handled = true;
        }

        private void OnRootManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = PART_Container;
            e.Handled = true;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshViewPort(SelectedIndex);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex > -1)
            {
                RefreshViewPort(SelectedIndex);
            }
        }
        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FlipView;

            control.OnSelectedIndexChanged(e);
        }

        private void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!EnsureTemplateParts())
            {
                return;
            }

            if ((int)e.NewValue >= 0 && (int)e.NewValue < Items.Count)
            {
                double toValue = (int)e.OldValue < (int)e.NewValue ? -ActualWidth : ActualWidth;
                RunSlideAnimation(toValue, fromValue);
            }
        }

        private void RefreshViewPort(int selectedIndex)
        {
            if (!EnsureTemplateParts())
            {
                return;
            }

            Canvas.SetLeft(PART_PreviousItem, -ActualWidth);
            Canvas.SetLeft(PART_NextItem, ActualWidth);
            PART_Root.RenderTransform = new TranslateTransform();

            var currentItem = GetItemAt(selectedIndex);
            var nextItem = GetItemAt(selectedIndex + 1);
            var previousItem = GetItemAt(selectedIndex - 1);

            PART_CurrentItem.Content = currentItem;
            PART_NextItem.Content = nextItem;
            PART_PreviousItem.Content = previousItem;
        }

        public void RunSlideAnimation(double toValue, double fromValue = 0)
        {
            if (!(PART_Root.RenderTransform is TranslateTransform))
            {
                PART_Root.RenderTransform = new TranslateTransform();
            }

            var story = AnimationFactory.Instance.GetAnimation(PART_Root, toValue, fromValue);
            story.Completed += (s, e) =>
            {
                RefreshViewPort(SelectedIndex);
            };
            story.Begin();
        }

        private object GetItemAt(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                return null;
            }

            return Items[index];
        }

        private bool EnsureTemplateParts()
        {
            return PART_CurrentItem != null && PART_NextItem != null && PART_PreviousItem != null && PART_Root != null;
        }

        private void OnPreviousCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedIndex > 0;
        }

        private void OnPreviousExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedIndex -= 1;
        }

        private void OnNextCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedIndex < (Items.Count - 1);
        }

        private void OnNextExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedIndex += 1;
        }
        #endregion

        #region Commands

        public static RoutedUICommand NextCommand = new RoutedUICommand("Next", "Next", typeof(FlipView));
        public static RoutedUICommand PreviousCommand = new RoutedUICommand("Previous", "Previous", typeof(FlipView));

        #endregion

        #region Override methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_PreviousItem = GetTemplateChild("PART_PreviousItem") as ContentControl;
            PART_NextItem = GetTemplateChild("PART_NextItem") as ContentControl;
            PART_CurrentItem = GetTemplateChild("PART_CurrentItem") as ContentControl;
            PART_Root = GetTemplateChild("PART_Root") as FrameworkElement;
            PART_Container = GetTemplateChild("PART_Container") as FrameworkElement;

            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
            PART_Root.ManipulationStarting += OnRootManipulationStarting;
            PART_Root.ManipulationDelta += OnRootManipulationDelta;
            PART_Root.ManipulationCompleted += OnRootManipulationCompleted;
        }
        #endregion

    }
}
