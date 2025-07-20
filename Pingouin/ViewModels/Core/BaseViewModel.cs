using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Pingouin.ViewModels
{
    /// <summary>
    /// A base class for view models that implements the INotifyPropertyChanged interface.
    /// This allows the UI to automatically update when a property value changes.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Fired when a property value changes, notifying the UI.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event to notify listeners that a property has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. 
        /// The [CallerMemberName] attribute automatically provides this value from the property's setter.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// A helper method for property setters. It checks if the new value is different from the old one.
        /// If it is, it updates the backing field and raises the PropertyChanged event.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">A reference to the backing field of the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="propertyName">The name of the property. Automatically provided by the [CallerMemberName] attribute.</param>
        /// <returns>True if the value was changed, false otherwise. This prevents unnecessary updates and potential infinite loops.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            // If the old and new values are the same, do nothing.
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;

            OnPropertyChanged(propertyName);

            return true;
        }
    }
}