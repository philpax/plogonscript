let stopwatch = new System.Diagnostics.Stopwatch();
let visible = false;

const key = VirtualKey.F4;
let keyWasPressed = false;

function onUpdate() {
  if (!KeyState[key] && keyWasPressed) {
    visible = !visible;
  }
  keyWasPressed = KeyState[key];
}

function onDraw() {
  if (!visible) return;

  if (ImGui.Begin("Stopwatch")) {
    const { Hours, Minutes, Seconds, Milliseconds } = stopwatch.Elapsed;
    ImGui.Text(
      System.String.Format(
        "{0:00}:{1:00}:{2:00}.{3:00}",
        Hours,
        Minutes,
        Seconds,
        Milliseconds / 10
      )
    );

    if (!stopwatch.IsRunning) {
      if (ImGui.Button("Start")) stopwatch.Start();
    } else {
      if (ImGui.Button("Pause")) stopwatch.Stop();
    }
    ImGui.SameLine();
    if (ImGui.Button("Reset")) stopwatch.Reset();
  }
  ImGui.End();
}
