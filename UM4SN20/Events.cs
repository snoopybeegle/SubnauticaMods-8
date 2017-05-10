
using UnityEngine;

namespace UM4SN
{
    public class Events
    {
        public void potato()
        {
            MonoBehaviour mb = GameObject.Find("Precursor_PurpleKeyTerminal(Clone)").GetComponent<MonoBehaviour>();

            StackTrace st = new StackTrace(true);
            for (int i = 0; i < st.FrameCount; i++)
            {
                // Note that high up the call stack, there is only 
                // one stack frame.
                StackFrame sf = st.GetFrame(i);
                Console.WriteLine();
                UnityEngine.Debug.Log("STK " + "High up the call stack, Method: " + sf.GetMethod());

                UnityEngine.Debug.Log("STK " + "High up the call stack, Line Number: " + sf.GetFileLineNumber());
            }
            UM4SN.PluginLoader.ModLoaderCustomEventI();
            base.transform.parent.BroadcastMessage("ToggleDoor", true);
        } 
    }
}
