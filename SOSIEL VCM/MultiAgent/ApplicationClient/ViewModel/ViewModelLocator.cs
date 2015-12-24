/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:ApplicationClient.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using System;
using ApplicationClient.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;

namespace ApplicationClient.ViewModel
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            IninializeNavigationService();

            SimpleIoc.Default.Register<MainViewModel>();
            ServiceLocator.Current.GetInstance<MainViewModel>();
            SimpleIoc.Default.Register<CommunicationViewModel>();
            ServiceLocator.Current.GetInstance<CommunicationViewModel>();
            SimpleIoc.Default.Register<StrategiesBuilderViewModel>();
            ServiceLocator.Current.GetInstance<StrategiesBuilderViewModel>();
            SimpleIoc.Default.Register<AgentsBuilderViewModel>();
            ServiceLocator.Current.GetInstance<AgentsBuilderViewModel>();
        }

        /// <summary>
        ///     Gets the Main property.
        /// </summary>
        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        /// <summary>
        ///     Gets the Communication property.
        /// </summary>
        public CommunicationViewModel CommunicationMap
        {
            get { return ServiceLocator.Current.GetInstance<CommunicationViewModel>(); }
        }

        /// <summary>
        ///     Gets the StrategiesBuilderViewModel
        /// </summary>
        public StrategiesBuilderViewModel StrategiesBuilder
        {
            get { return ServiceLocator.Current.GetInstance<StrategiesBuilderViewModel>(); }
        }

        /// <summary>
        ///     Gets the AgentsBuilderViewModel
        /// </summary>
        public AgentsBuilderViewModel AgentsBuilder
        {
            get { return ServiceLocator.Current.GetInstance<AgentsBuilderViewModel>(); }
        }

        private static void IninializeNavigationService()
        {
            var nav = new FrameNavigationService();
            nav.RegisterService();

            nav.RegisterViewModel("Main", new Uri("pack://application:,,,/Views/StartFrame.xaml"));
            nav.RegisterViewModel("CommunicationMap", new Uri("pack://application:,,,/Views/CommunicationView.xaml"));
            nav.RegisterViewModel("StrategiesBuilder", new Uri("pack://application:,,,/Views/StrategiesBuilderView.xaml"));
            nav.RegisterViewModel("AgentsBuilder", new Uri("pack://application:,,,/Views/AgentsBuilder.xaml"));

            SimpleIoc.Default.Register<INavigationService>(() => nav);
        }
    }
}