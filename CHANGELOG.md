09/12/2015

- Improved the multithreading code a lot. Now the 2 frame spread gets a 14% speed increase and the 3 frame spread gets an 11% speed increase. The rest stays the same.
- Fixed a bug where when using 2 shader lods the 1st lod material wouldn't get assigned.
- Fixed one more issue with gaps being visible in far away tiles.
- Revamped the editor and exposed the ability to alter the width of the foam trail produced by the boat and to specify if the lod meshes should skip frames to update.
- Modified shaders to use floats instead of halfs because this causes precision artefacts on weak devices.
- started adding to code to improve the buoyancy.
- optimized some math code.
- added mask mesh to the fishing boat.


08/12/2015

 - Restructured the multithreading code.
 - made the FFT plugin multithreading friendly.
 - The above optimizations give extra speed boost on low spread job frames. This gives now smooth results and very good performance with vsync and 3 frames job spread.
 
 - Had to fix the shader again because some previous hack made the seams of the tiles visible.
 
 - The ocean is now precalculated at the Start() function. This eliminates the annoyance having the boats jumping ugly at the beginning, because now they find their position on the sea.
 

06/12/2015

  - Ability to skip renders of reflection/refraction. Since reflection and refraction are not easy for the eye to catch their changes, we can update them every x frames to gain performance.
    (this gives around 10-20% speed increase, but depends on the complexity of the rendered reflection/refraction layer.)
	
  -Preallocated LOD mesh buffers. This eliminates garbage generation and the kicking in of the GarbageCollector. Gives a small speed increase.
  
  -Added the ability to run the calculation of the waves using the FixedUpdate() function. This almost doubles the framerate compared to regular Update(), but disables the 'spread along frames' functionality."
  
  - Made the system WEBGL compliant. (Threads do not work in WEBGL for now.)
  
  - Yet another shader modification:
    --You can change the tiling of the foam and bump map now.
    --Corrected a glitch with foam/water bump animation.
   
  - Unhooked the boyancy.cs script from the boat controller. This was limiting the use of the boyancy script.
 
  - Optimized the boat controller script to avoid garbage generation.
 
  - Optimized the boyancy script:
     --to avoid garbage generation. 
	 --general optimizations to avoid GetComponent calls.
	 --exposed y-offset and x/z slices in the inspector.
	 --more importantly: added support for interpolating buoyancy among xx frames. This gives a much smoother movement.
	 --with the optimizations the script has become around 20% faster.
  
  - Editor:
     --Ability to save the new additions (render reflection/refraction every xx frames, foam map tiling, bump map tiling)
	 --Saves and loads the sun's rotation. Made loading of sun's rotation optional.
	 --Sun direction vector gets updated in realtime now in the editor.
	 
  -Demos: updated the demos to reflect realworld scales. The previous demo had exaggerated proportions.
     --Added bigger ships demonstrating how to set them up for buoyancy.
	 
  -Optimized the GetWaterHeightAtLocation function to run 40% faster. (used for buoyancy	).
	 
  -Allowed the spreading of the wave calculations to drop up to 2 frames. This should eliminate all jerkiness when vsync is on.
  
  -Restructured the thread balancing. This gave some good speed increasings depending on the frame spread selected. (10-25%).
  
  
  

05/12/2015
The shaders got one more overhaul, by removing yet more unnecessary calculations and by moving calculations from the fragment to the vertex shader.
This gives room to squeeze in translucency and/or shadows on the hq shader lod.

That solved some issues on my android device.

In general compared to the old system my old XperiaS went from 12 fps to 30 fps.





05/12/2015
- Some extra optimizations.

- Ability to save and load presets of oceans. This will save you a lot of time.
You can load the preset also on runtime, but some of the variables (like grid size) will not get updated for obvious reasons. Right now the directory for the presets is in the Assets/Ocean/Editor location
. You can modify the editor script to your own needs.

- Ability to have 1 or 2 extra lod materials/shaders for the far tiles. This will give around 15% extra speed increase.

- Ability to choose the default LOD on the main shader.

- Switch on runtime between high and low lod on the shader.

- The shader has 3 lods and 1 extra with alpha transparency if you need it.

- Shader rewritten for performance gains. (full SM2 support.)

- uv scale of bump and foam maps stays consistent on all scales now.

- Optimized the buoyancy script a bit and on the boat controller added a condition to raise the damp coefficient when idle.

- The editor had an extra overhaul to reflect the new functionality. I added also some help buttons that explain the new functions.




02/12/2015
Modifications on the Ocean.cs file.

Besides the visibility checks, i am spreading the wave calculations among x frames and also added support for multi-threading.

For devices that want vsync enabled a low number of spreading frames is recommended (3-4).
If you go for higher frame rates then 5-7 is ok.

You can disable multithreading by commenting out a preprocessor directive in the Ocean.cs file.

I have optimized the Ocean.shader a bit more.

Modified the Editor script to reflect and support the new additions and changes.

SHADER:
I have updated the shader for a better foam effect. However this adds one more texture fetch.

EDITOR:
I have revamped the editor and made most of the variables to affect the ocean in realtime.
Added variables in the editor that you could only change on the material.
Added the ability to offset the position of far away lods (lod4, lod5). This is useful when you have large waves and you could see gaps in the horizon.

OCEAN SCRIPT:

- Skipping update every second tick of meshes belonging to lods>0. This gives an extra 5-10% speed increase.
- General cleanup and avoiding unnecessary calculations.




01/12/2015
Optimized Ocean.cs script.

- It updates in realtime the sun light color.

- Enlarges by 5% the tile bounds to eliminate the random disappearance of tiles.

- updates the meshes of tiles only when they are visible by the camera and adds around 15% speed increase. 

