using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Pingouin.Properties;

namespace Pingouin
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            // Sauvegarder les paramètres une dernière fois avant de fermer
            Settings.Default.Save();
            base.OnExit(e);
        }
    }
}