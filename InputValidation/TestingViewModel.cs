namespace InputValidation
{
    class TestingViewModel : BaseViewModel
    {
        private string text;

        [Validated("You have to enter something")]
        public string Text {
            get => text;
            set => SetValue(ref text, value);
        }

        protected override void InitializeValidators()
        {
            AddValidator(nameof(Text), () => Text?.Length > 0);
        }
    }
}
