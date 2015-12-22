22/12/2015 Major update

- Shaders almost totally rewritten.
- Added per vertex translucency shader lod.
- Added shore line foam support (It can be switched on of by code or by the editor, per lod).
  (You can define the shore length and the shore foam power.)
- Shaders calculate their own linear fog (if enabled). This was done to save shader instructions.
   (That means that only linear fog is valid now. Use the fog far and fog near in the unity editor to set the values.)
- Fog can be switched on on or off per lod.
- Added the ability to display only fog color after a certain range. (Called distance cancellation.) Can be switched on or off per lod.
- Added Specular Power slider. (Specular didn't had power factor before.)
- Fresnel calculations have been simplified in the shaders to reduce shader instructions.
- Added the ability to set render queues for the ocean and the boats. This is useful if you don't want to show shore foam around the boat.
- Reordered and revamped the inspector.
- Simplified the Exocortex plugin even more.
- Added more materials and ocean presets to demonstrate the new features.
- Added a new scene with an island to test the shore foam.   

issues: 	1. If you set the render queue of the boat to 2501 and higher to avoid the shore foam around it you lose shadows.
			2. If you have a render queue that shows foam around the boat some white pixels appear around the silhouette of the boat.
			    That is because the depth buffer is used. Will look to smooth this out in a next update.

16/12/2015 second part

- Added ability to adjust the reflection strength.
- Fixed some issues with the new alpha shader switching.
- Slimmed down more the Exocortex plugin.
- Updated and added more presets.


16/12/2015

- Changed the bump and foam scrolling in the shaders. WaveOffset speed parameter has been removed.
- Improved specularity in the shaders.
- Added extra shaders with alpha support.
- Added shader with Reflection and alpha.
- Added extra lod materials to support the new alpha shaders.
- Simplified the far lod shaders to improve performance.
- The buoyancy editor saves now also boat controller parameters if the boat controller is attached to the object.


15/12/2015 second part

- Uploaded project with 'Library' folder to avoid losing prefab connections.
- Small bug fixes.
- Small performance increase. (~5%)
- Slimmed down the Exocortex plugin.
- WebGL compatibility fixes.
- Buoyancy update.
- Vector Normalization performance increase.


15/12/2015

- Major Bugfix in the buoyancy script. Now Buoyancy will work much better.
- Buoyancy fixes on boats to work well with various sizes of the ocean.
- Ability to run the buoyancy simulation of an object in fixed or regular update.
- More shader modifications. Added one more lod and improved foam on higher quality lods.
- Math optimizations on Vector3 Normalize gave some extra performance boost.
- Added 2 more lod materials and shaders that support reflection and refraction.
- Now the highest quality shader lod is lod5.
- Now the ocean materials  are saved in the ocean presets. This required to put them in the Resources folder!


13/12/2015 Hotfix update

- I had to rewrite the Get water height and get choppiness functions. They had some bugs and were not accurate enough.
- Added an option to the buoyancy script to use a more accurate but slower function.
- The simple buoyant object can work also with rigidbodies now.


12/12/2015 Major update

- Buoyancy has been rewritten with very good and fast results.
- Custom inspector for buoyancy with additional variables.
- You can save/load a buoyancy setup for a vessel.
- Partially rewrite of the boat controller script. Boats behave more realistic now.
- Choppy waves, wind and wave slopes are able to influence the vessel's condition. This adds a lot of realism.
- The shader has been rechecked again. Higher performance is back in now. The previous mobile fix added some unneeded overhead.
- More multithreading optimizations.
- Math optimizations.
- The Editor Inspector has been improved and prepared for the future additions.


09/12/2015

- Improved the multithreading code a lot. Now the 2 frame spread gets a 14% speed increase and the 3 frame spread gets an 11% speed increase. The rest stays the same.
- Fixed a bug where when using 2 shader lods the 1st lod material wouldn't get assigned.
- Fixed one more issue with gaps being visible in far away tiles.
- Revamped the editor and exposed the ability to alter the width of the foam trail produced by the boat and to specify if the lod meshes should skip frames to update.
- Modified shaders to use floats instead of halfs because this causes precision artefacts on weak devices.
- started adding code to improve the buoyancy.
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

