
@script ExecuteInEditMode

private var gui : GUIText;

private var updateInterval = 1.0;
private var lastInterval : double; // Last interval end time
private var frames = 0; // Frames over current interval

private var framesavtick = 0;
private var framesav = 0.0;

function Start()
{
    lastInterval = Time.realtimeSinceStartup;
    frames = 0;
	framesav = 0;

	Screen.sleepTimeout = SleepTimeout.NeverSleep;

}

function OnDisable ()
{
	if (gui)
		DestroyImmediate (gui.gameObject);
}

function Update()
{
    ++frames;

    var timeNow = Time.realtimeSinceStartup;
	
    if (timeNow > lastInterval + updateInterval)
    {
		if (!gui)
		{
			var go : GameObject = new GameObject("FPS Display", GUIText);
			go.hideFlags = HideFlags.HideAndDontSave;
			go.transform.position = Vector3(0,0,0);
			gui = go.GetComponent.<GUIText>();
			gui.pixelOffset = Vector2(Screen.width-Screen.width*0.5,50);
		}
		
			
        var fps : float = frames / (timeNow - lastInterval);
		var ms : float = 1000.0f / Mathf.Max (fps, 0.00001);

			++framesavtick;
			framesav+=fps;
		var fpsav : float = framesav/framesavtick;
		
		gui.text = ms.ToString("f1") + "ms " + fps.ToString("f2") + "FPS     AvgFps : " + fpsav.ToString("f2");
        frames = 0;
        lastInterval = timeNow;
    }
	
	
}
