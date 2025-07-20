namespace Pingouin.ViewModels
{
    public partial class MainViewModel
    {
        private bool _isSaving;
        /// <summary>
        /// Gets or sets a value indicating whether a save operation is in progress
        /// This property controls the visibility of the loading overlay
        /// </summary>
        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        private int _saveProgressPercentage;
        /// <summary>
        /// Gets or sets the save progress percentage (0-100).
        /// </summary>
        public int SaveProgressPercentage
        {
            get => _saveProgressPercentage;
            set => SetProperty(ref _saveProgressPercentage, value);
        }
    }
}