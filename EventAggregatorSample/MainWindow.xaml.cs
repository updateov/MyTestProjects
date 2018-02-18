using System;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;

namespace EventAggregatorSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class test
    {
    }

    public partial class MainWindow : Window, IDisposable
    {
        readonly IEventAggregator _aggregator = new EventAggregator();

        private SubscriptionToken _intInt1SubscriptionToken;
        private SubscriptionToken _intInt2SubscriptionToken;

        public MainWindow()
        {
            InitializeComponent();

            // Next line only shows can unsubscribe before subscribing, like for events.
            _aggregator.GetEvent<SampleStringEvent>().Unsubscribe(StringAction1);

            // 4 subscriptions
            _aggregator.GetEvent<SampleStringEvent>().Subscribe(StringAction1);
            // Example with filtering
            _aggregator.GetEvent<SampleStringEvent>().Subscribe(StringAction2,
                ThreadOption.UIThread, false, i => i.Contains("b"));
            // Next line shows saving the subscription token, used to unsubscribe
            _intInt1SubscriptionToken = _aggregator.GetEvent<SampleIntEvent>().
                Subscribe(IntAction1, ThreadOption.UIThread, false, i => i > 0);
            _intInt2SubscriptionToken = _aggregator.GetEvent<SampleIntEvent>().
                    Subscribe(IntAction1, ThreadOption.UIThread, false, i => i < 0);
            // Extra Credit
            _aggregator.GetEvent<SampleStringEvent>().
                    Subscribe(StringAction3, ThreadOption.UIThread, false,
                                    s =>
                                    {
                                        int i;
                                        return int.TryParse(s, out i);
                                    });
        }

        // EventAggregator Event handlers for the 4 subscriptions
        private void StringAction1(string s)
        {
            TextBoxReceiveString1.Text = s;
        }

        private void StringAction2(string s)
        {
            TextBoxReceiveString2.Text = s;
        }

        private void StringAction3(string s)
        {
            TextBoxReceiveStringInt.Text = s;
        }

        private void IntAction1(int i)
        {
            TextBoxReceiveInt.Text = i.ToString();
        }

        // Event handlers for the 3 "Send" buttons 
        private void ButtonSendString1(object sender, RoutedEventArgs e)
        {
            _aggregator.GetEvent<SampleStringEvent>().Publish(TextBoxSendString1.Text);
        }

        private void ButtonSendString2(object sender, RoutedEventArgs e)
        {
            _aggregator.GetEvent<SampleStringEvent>().Publish(TextBoxSendString2.Text);
        }

        private void ButtonSendInt(object sender, RoutedEventArgs e)
        {
            int output;
            _aggregator.GetEvent<SampleStringEvent>().Publish(TextBoxSendInt.Text);
            if (int.TryParse(TextBoxSendInt.Text, out output))
            {
                _aggregator.GetEvent<SampleIntEvent>().Publish(output);
                if (output < 0) // Only allow negative number once
                    _aggregator.GetEvent<SampleIntEvent>().Unsubscribe(_intInt2SubscriptionToken);
                if (output < -1000) // Clear all 
                    _aggregator.GetEvent<SampleIntEvent>().Unsubscribe(IntAction1);
                if (output == 0) // reset all 
                {
                    //Need to clear first...
                    _aggregator.GetEvent<SampleIntEvent>().Unsubscribe(IntAction1);
                    // Next line shows saving the subscription token, used to unsubscribe
                    _intInt1SubscriptionToken = _aggregator.GetEvent<SampleIntEvent>().
                        Subscribe(IntAction1, ThreadOption.UIThread, false, i => i > 0);
                    _intInt2SubscriptionToken = _aggregator.GetEvent<SampleIntEvent>().
                            Subscribe(IntAction1, ThreadOption.UIThread, false, i => i < 0);
                }
            }
            else
            {
                MessageBox.Show("The value is not an integer");
            }
        }

        // Dispose of the 4 subscriptions (really need to also have the Destructor)
        // see http://bytes.com/topic/c-sharp/answers/226921-dispose-destructor-guidance-long
        public void Dispose()
        {
            _aggregator.GetEvent<SampleStringEvent>().Unsubscribe(StringAction1);
            _aggregator.GetEvent<SampleStringEvent>().Unsubscribe(StringAction2);
            _aggregator.GetEvent<SampleIntEvent>().Unsubscribe(_intInt1SubscriptionToken);
            _aggregator.GetEvent<SampleIntEvent>().Unsubscribe(_intInt2SubscriptionToken);
            _aggregator.GetEvent<SampleStringEvent>().Unsubscribe(StringAction3);
        }
    }

    public class SampleStringEvent : CompositePresentationEvent<string> { }
    public class SampleIntEvent : CompositePresentationEvent<int> { }
}
