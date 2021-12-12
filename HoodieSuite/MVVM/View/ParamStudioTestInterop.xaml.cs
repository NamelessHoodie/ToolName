using System.Windows;

namespace HoodieSuite.MVVM.View
{
    public partial class ParamStudioTestInterop : Window
    {
        private StudioClient _client;
        public ParamStudioTestInterop()
        {
            InitializeComponent();
        }

        private void StartClient_OnClick(object sender, RoutedEventArgs e)
        {
            _client = StudioClient.StartClient(); 
        }

        private void StartParamStudio_OnClick(object sender, RoutedEventArgs e)
        {
            StudioClient.StartParamStudio();
            //_client.OpenParam(true, "EquipParamWeapon", "110000");
        }
    }
}