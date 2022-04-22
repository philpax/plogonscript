//m:{"Name":"Stopwatch","Author":"Philpax"}
let visible = false;
const stopwatch = new System.Diagnostics.Stopwatch();

function onKeyUp({ key }) {
  if (key == VirtualKey.F4) {
    visible = !visible;
  }
}

function onDraw() {
  if (!visible) return;

  UIHelpers.withWindow("Stopwatch", () => {
    const Format = "{0:00}:{1:00}:{2:00}.{3:00}";
    const { Hours, Minutes, Seconds, Milliseconds } = stopwatch.Elapsed;
    ImGui.Text(
      System.String.Format(Format, Hours, Minutes, Seconds, Milliseconds / 10)
    );

    if (!stopwatch.IsRunning) {
      if (ImGui.Button("Start")) stopwatch.Start();
    } else {
      if (ImGui.Button("Pause")) stopwatch.Stop();
    }
    ImGui.SameLine();
    if (ImGui.Button("Reset")) stopwatch.Reset();
  });
}
