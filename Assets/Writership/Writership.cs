using System;

namespace Writership
{
    internal class Writership
    {
        private string fileName;
        private int lineNumber;

        public void Mark()
        {
            var frame = new System.Diagnostics.StackFrame(2, true);
            string nowFileName = frame.GetFileName();
            int nowLineNumber = frame.GetFileLineNumber();

            if (fileName == null)
            {
                fileName = nowFileName;
                lineNumber = nowLineNumber;
            }
            else if (fileName != nowFileName || lineNumber != nowLineNumber)
            {
                UnityEngine.Debug.Log("Now write: " + nowFileName + ":" + nowLineNumber);
                UnityEngine.Debug.Log("Org write: " + fileName + ":" + lineNumber);
                throw new InvalidOperationException("Cannot write to same at different places");
            }
        }
    }
}
