﻿namespace DotNet.Clipboard.Infrastructure
{
    using DotNetToolkit.Wpf.Commands;
    using DotNetToolkit.Wpf.Mvvm;
    using FluentValidation;
    using System;
    using System.Threading.Tasks;

    public abstract class FormViewModelBase<TValidator> : ViewModelBase where TValidator : IValidator
    {
        #region Fields

        private bool _isDirty;
        private readonly IValidator _validator;

        #endregion

        #region Properties

        public bool IsDirty
        {
            get { return _isDirty; }
            set { SetProperty(ref _isDirty, value); }
        }

        #endregion

        #region Commands

        public RelayCommand CloseCommand { get; set; }
        public RelayCommand SubmitCommand { get; set; }

        #endregion

        #region Events

        public event EventHandler Submitted;
        public event EventHandler Closed;

        #endregion

        #region Constructors

        protected FormViewModelBase()
        {
            _validator = Activator.CreateInstance<TValidator>();

            PropertyChanged += (sender, args) =>
            {
                if (!args.PropertyName.Equals(nameof(IsDirty)) &&
                    !args.PropertyName.Equals(nameof(IsInitialized)))
                {
                    IsDirty = true;
                }
            };

            CloseCommand = new RelayCommand(OnClose);
            SubmitCommand = new RelayCommand(OnSubmitAsync);
        }

        #endregion

        #region Public Methods

        public virtual async Task<bool> ValidateAsync()
        {
            ClearErrors();

            var result = await _validator.ValidateAsync(this);
            if (result == null)
                return false;

            foreach (var error in result.Errors)
            {
                SetError(error.PropertyName, error.ErrorMessage);
            }

            return result.IsValid;
        }

        #endregion

        #region Protected Methods

        protected virtual async void OnSubmitAsync()
        {
            if (await ValidateAsync())
            {
                Submitted?.Invoke(this, EventArgs.Empty);

                OnClose();
            }
        }

        protected virtual void OnClose()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
