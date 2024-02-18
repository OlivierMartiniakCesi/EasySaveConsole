using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using System.Configuration;
using EasySaveV2.MVVM.Models;
using EasySaveV2.MVVM.Views;
using EasySaveV2.MVVM.ViewModels;

namespace EasySaveV2.MVVM.ViewModels
{
    class SettingsViewModels
    {

        public bool ToggleButtonState
        {
            get => bool.Parse(ConfigurationManager.AppSettings["ToggleButtonState"] ?? "false");
            set => ConfigurationManager.AppSettings["ToggleButtonState"] = value.ToString();
        }

        public void TraductorEnglish()
        {
            Application.Current.Resources.MergedDictionaries[0].Source = new Uri("Language/DictionaryEnglish.xaml", UriKind.RelativeOrAbsolute);
        }
        public void TraductorFrench()
        {
            Application.Current.Resources.MergedDictionaries[0].Source = new Uri("Language/DictionaryFrench.xaml", UriKind.RelativeOrAbsolute);
        }

    }
}
