using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System;

namespace MTGPrint.Helper.UI
{
    /// <summary>
    /// Provides extended support for drag drop operation
    /// </summary>
    public static class ScrollOnDragDrop
    {
        #region ScrollViewerSearchLocation
        public static readonly DependencyProperty ScrollViewerSearchLocationProperty =
            DependencyProperty.RegisterAttached("ScrollViewerSearchLocation",
                typeof(ScrollViewerSearchLocation),
                typeof(ScrollOnDragDrop),
                new PropertyMetadata(ScrollViewerSearchLocation.Disable, HandleEnableChanged));

        public static ScrollViewerSearchLocation GetScrollViewerSearchLocation(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return (ScrollViewerSearchLocation)element.GetValue(ScrollViewerSearchLocationProperty);
        }

        public static void SetScrollViewerSearchLocation(DependencyObject element, ScrollViewerSearchLocation value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            element.SetValue(ScrollViewerSearchLocationProperty, value);
        }

        private static void HandleEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement container)
                return;

            container.PreviewDragOver -= OnContainerPreviewDragOver;

            if (((ScrollViewerSearchLocation)e.NewValue) != ScrollViewerSearchLocation.Disable)
                container.PreviewDragOver += OnContainerPreviewDragOver;
        }
        #endregion

        #region ToleranceProperty
        public static readonly DependencyProperty ToleranceProperty
            = DependencyProperty.RegisterAttached("Tolerance",
                                                  typeof(double),
                                                  typeof(ScrollOnDragDrop),
                                                  new UIPropertyMetadata(60D));

        /// <summary>
        /// Gets whether the control can be used as drop target.
        /// </summary>
        public static double GetTolerance(DependencyObject element)
            => (double)element.GetValue(ToleranceProperty);

        /// <summary>
        /// Sets whether the control can be used as drop target.
        /// </summary>
        public static void SetTolerance(DependencyObject element, double value)
            => element.SetValue(ToleranceProperty, value);
        #endregion

        #region ScrollSpeedProperty
        public static readonly DependencyProperty ScrollSpeedProperty
            = DependencyProperty.RegisterAttached("ScrollSpeed",
                                                  typeof(double),
                                                  typeof(ScrollOnDragDrop),
                                                  new UIPropertyMetadata(20D));

        /// <summary>
        /// Gets whether the control can be used as drop target.
        /// </summary>
        public static double GetScrollSpeed(DependencyObject element)
            => (double)element.GetValue(ScrollSpeedProperty);

        /// <summary>
        /// Sets whether the control can be used as drop target.
        /// </summary>
        public static void SetScrollSpeed(DependencyObject element, double value)
            => element.SetValue(ScrollSpeedProperty, value);
        #endregion

        private static void OnContainerPreviewDragOver(object sender, DragEventArgs e)
        {
            if (sender is not FrameworkElement container)
                return;

            var scrollViewer = GetScrollViewerSearchLocation(container) == ScrollViewerSearchLocation.Parent
                ? GetFirstVisualParent<ScrollViewer>(container)
                : GetFirstVisualChild<ScrollViewer>(container);

            if (scrollViewer == null)
                return;

            double tolerance = GetTolerance(container);
            double verticalPos = e.GetPosition(scrollViewer).Y;
            double speed = GetScrollSpeed(container);

            if (verticalPos < tolerance) // Top of visible list? 
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - speed); //Scroll up.
            else if (verticalPos > scrollViewer.ActualHeight - tolerance) //Bottom of visible list? 
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + speed); //Scroll down.
        }

        public static T GetFirstVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                        return t;

                    T childItem = GetFirstVisualChild<T>(child);

                    if (childItem != null)
                        return childItem;
                }
            }

            return null;
        }

        public static T GetFirstVisualParent<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                var pain = VisualTreeHelper.GetParent(depObj);
                if (pain != null && pain is T t)
                    return t;

                T pItem = GetFirstVisualParent<T>(pain);
                if (pItem != null)
                    return pItem;
            }

            return null;
        }
    }

    public enum ScrollViewerSearchLocation
    {
        Disable,
        Child,
        Parent
    }
}
