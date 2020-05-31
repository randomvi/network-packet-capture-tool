using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Speech;
namespace NETAPP
{
    class System19
    {
        // More asbraction methods to take place
        public static void Say(string message)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
         //   synth.SetOutputToDefaultAudioDevice();
         //   synth.SpeakAsync(message);
            synth.Dispose();
        }

        public static void Say(string message,string input)
        {
            // will be able to go to a certain step usig GOTO: method  ,when the admin says so
            SpeechSynthesizer synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();
        //    synth.Speak(message);
            // asks admin what to do
            // addmin talks then whatever happans
        }
    }
}
