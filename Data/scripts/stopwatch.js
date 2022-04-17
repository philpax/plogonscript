let stopwatch = new System.Diagnostics.Stopwatch();

function onDraw()
{
    if (ImGui.Begin("Stopwatch"))
    {
        let ts = stopwatch.Elapsed;

        let elapsedTime = System.String.Format(
            "{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10
        );

        ImGui.Text(elapsedTime);

        if (!stopwatch.IsRunning)
        {
            if (ImGui.Button("Start"))
                stopwatch.Start();
        }
        else
        {
            if (ImGui.Button("Pause"))
                stopwatch.Stop();
        }

        ImGui.SameLine();

        if (ImGui.Button("Reset"))
            stopwatch.Reset();
    }
    ImGui.End();
}