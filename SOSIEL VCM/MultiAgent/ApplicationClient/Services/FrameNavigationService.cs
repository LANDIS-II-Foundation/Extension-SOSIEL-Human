using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;

namespace ApplicationClient.Services
{
    public class FrameNavigationService : INavigationService
    {
        private Dictionary<string, Uri> _views;
        private Queue<string> _historyPages;
        private bool _firstTime = true;

        public void RegisterService()
        {
            _views = new Dictionary<string, Uri>();
            _historyPages = new Queue<string>();
        }

        public void RegisterViewModel(string viewModelName, Uri viewPath)
        {
            _views.Add(viewModelName, viewPath);
            if (_firstTime)
            {
                CurrentPageKey = viewModelName;
                _historyPages.Enqueue(viewModelName);
                _firstTime = false;
            }
        }

        public void GoBack()
        {
            NavigateTo(_views.First().Key);
        }

        public void NavigateTo(string pageKey)
        {
            NavigateTo(pageKey, null);
        }

        public void NavigateTo(string pageKey, object parameter)
        {
            var frame = GetDescendantFromName(Application.Current.MainWindow, "MainFrame") as Frame;

            if (frame == null)
                return;

            frame.Source = _views[pageKey];

            _historyPages.Enqueue(pageKey);
            CurrentPageKey = pageKey;

        }

        private static FrameworkElement GetDescendantFromName(DependencyObject parent, string name)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);

            if (count < 1)
            {
                return null;
            }

            for (var i = 0; i < count; i++)
            {
                var frameworkElement = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                if (frameworkElement != null)
                {
                    if (frameworkElement.Name == name)
                    {
                        return frameworkElement;
                    }

                    frameworkElement = GetDescendantFromName(frameworkElement, name);
                    if (frameworkElement != null)
                    {
                        return frameworkElement;
                    }
                }
            }
            return null;
        }

        public string CurrentPageKey { get; private set; }
    }
}
