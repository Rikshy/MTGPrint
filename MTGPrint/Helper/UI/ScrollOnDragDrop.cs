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
        #region EnableProperty
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached("Enable",
                typeof(bool),
                typeof(ScrollOnDragDrop),
                new PropertyMetadata(false, HandleEnableChanged)); 

        public static bool GetEnable(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (bool)element.GetValue(EnableProperty);
        }

        public static void SetEnable(DependencyObject element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(EnableProperty, value);
        }

        private static void HandleEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement container))
                return;

            Unsubscribe(container);

            if (true.Equals(e.NewValue))
                Subscribe(container);
        }

        private static void Subscribe(FrameworkElement container)
            => container.PreviewDragOver += OnContainerPreviewDragOver;

        private static void Unsubscribe(FrameworkElement container)
            => container.PreviewDragOver -= OnContainerPreviewDragOver;
        #endregion

        #region SearchScrollViewerParentProperty
        public static readonly DependencyProperty SearchScrollViewerOnParentProperty
            = DependencyProperty.RegisterAttached("SearchScrollViewerOnParent",
                                                  typeof(bool),
                                                  typeof(ScrollOnDragDrop),
                                                  new UIPropertyMetadata(false));

        /// <summary>
        /// Gets whether the control can be used as drop target.
        /// </summary>
        public static bool GetSearchScrollViewerParent(DependencyObject element)
            => (bool)element.GetValue(SearchScrollViewerOnParentProperty);

        /// <summary>
        /// Sets whether the control can be used as drop target.
        /// </summary>
        public static void SetSearchScrollViewerParent(DependencyObject element, bool value)
            => element.SetValue(SearchScrollViewerOnParentProperty, value);
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
            if (!(sender is FrameworkElement container))
                return;

            var scrollViewer = GetSearchScrollViewerParent(container)
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
                    if (child != null && child is T)
                        return (T)child;

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
                if (pain != null && pain is T)
                    return (T)pain;

                T pItem = GetFirstVisualParent<T>(pain);
                if (pItem != null)
                    return pItem;
            }

            return null;
        }
    }
}
