using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NeewerLightControlBT.Animation
{
    public enum AnimationStyle
    {
        Cut,
        ColorInterpolation
    }

    public class TimedNeewerLightInstruction
    {
        public NeewerLightInstruction Instruction { get; set; }
        public int DelayAfter { get; set; }
        public AnimationStyle Style { get; set; } = AnimationStyle.Cut; // Default to Cut

        public List<string> Lights { get; set; } = new List<string>();

        public TimedNeewerLightInstruction(NeewerLightInstruction instruction, int delayAfter, AnimationStyle style = AnimationStyle.Cut)
        {
            Instruction = instruction;
            DelayAfter = delayAfter;
            Style = style;
        }
    }
    public class LightAnimationSystem
    {
        private BlockingCollection<TimedNeewerLightInstruction> instructionQueue =
            new BlockingCollection<TimedNeewerLightInstruction>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private ManualResetEvent queueEmptyEvent = new ManualResetEvent(false);

        private NeewerBTCommunicationManager CommMgr = null;
        // Constructor starts the processing task
        public LightAnimationSystem(NeewerBTCommunicationManager mgr)
        {
            CommMgr = mgr;
            Task.Run(() => ProcessInstructions(cancellationTokenSource.Token));
        }

        // Call this method to add new instructions
        public void EnqueueInstruction(List<TimedNeewerLightInstruction> instructions)
        {
            foreach (var item in instructions) 
                instructionQueue.Add(item);
        }

        // Dedicated thread method for processing the queue
        private void ProcessInstructions(CancellationToken cancellationToken)
        {
            try
            {
                // Continue processing until the cancellation is requested
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Take will block if the collection is empty
                        var timedInstruction = instructionQueue.Take(cancellationToken);
                        queueEmptyEvent.Reset();
                        foreach (var light in timedInstruction.Lights)
                        {
                            Task.Run(() => SendInstructionToLight(light, timedInstruction.Instruction));
                            //System.Diagnostics.Debug.WriteLine($"<{timedInstruction.Instruction.RGB.R},{timedInstruction.Instruction.RGB.G},{timedInstruction.Instruction.RGB.B}>");
                        }
                        if (instructionQueue.Count == 0)
                        {
                            queueEmptyEvent.Set();
                        }

                        // Use the delay specified in the instruction
                        if (timedInstruction.DelayAfter > 0)
                        {
                            Task.Delay(timedInstruction.DelayAfter, cancellationToken).Wait();
                        }
                    }
                    catch(NullReferenceException)
                    {
                        return;
                    }
                    catch(ArgumentNullException)
                    {
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation of the queue processing
            }
            finally
            {
                
                queueEmptyEvent.Set();
            }
        }

        // Call this method to stop processing and clean up
        public void Stop()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            instructionQueue.Dispose();
        }
        public bool WaitUntilQueueIsEmptyOrTimeout(int TimeoutMilliseconds)
        {
            return queueEmptyEvent.WaitOne(TimeoutMilliseconds);
        }
        private async Task SendInstructionToLight(string light, NeewerLightInstruction instruction)
        {
            await CommMgr.ExecuteLightInstruction(light, instruction);
            // Implementation of sending the instruction to the light
            
        }
    }

    public static class Utility
    {
        public static List<TimedNeewerLightInstruction> GenerateInterpolatedColors(NeewerColor startColor, NeewerColor endColor, int durationMs, int stepMs, List<string> Lights)
        {
            return GenerateInterpolatedColors(startColor, 100f, endColor, 100f, durationMs, stepMs, Lights);
        }
        public static List<TimedNeewerLightInstruction> GenerateInterpolatedColors(NeewerColor startColor, float startBrightness, NeewerColor endColor, float endBrightness, int durationMs, int stepMs, List<string> Lights)
        {
            List<TimedNeewerLightInstruction> colorSteps = new List<TimedNeewerLightInstruction>();
            stepMs -= 15;  // There's a built in bluetooth delay of 15 seconds.
            if (stepMs < 1)
                stepMs = 1;

            int steps = durationMs / stepMs;
            if (steps < 0)
                steps = 1;
            for (int i = 0; i <= steps; i++)
            {
                float ratio = i / (float)steps;
                byte r = (byte)(startColor.R + ratio * (endColor.R - startColor.R));
                byte g = (byte)(startColor.G + ratio * (endColor.G - startColor.G));
                byte b = (byte)(startColor.B + ratio * (endColor.B - startColor.B));
                float brightness = startBrightness + ratio * (endBrightness - startBrightness);
                colorSteps.Add( new TimedNeewerLightInstruction(new NeewerLightInstruction
                {
                    RGB = new NeewerColor() { R = r, G = g, B = b },
                    LightMode = nLightMode.HSIMode,
                    brightness = brightness

                }, stepMs, AnimationStyle.ColorInterpolation)
                { Lights = Lights }
                );
            }
            return colorSteps;
        }
    }
}
