using ImGuiNET;
using System.Diagnostics;

class Script
{
    Stopwatch Stopwatch = new Stopwatch();

    public void Draw()
    {
        if (ImGui.Begin("Stopwatch"))
        {
            var ts = Stopwatch.Elapsed;

            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            ImGui.Text(elapsedTime);

            if (!Stopwatch.IsRunning)
            {
                if (ImGui.Button("Start"))
                {
                    Stopwatch.Start();
                }
            }
            else
            {
                if (ImGui.Button("Pause"))
                {
                    Stopwatch.Stop();
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Reset"))
            {
                Stopwatch.Reset();
            }

        }
        ImGui.End();
    }
}